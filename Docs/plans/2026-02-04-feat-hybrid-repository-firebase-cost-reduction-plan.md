---
title: "feat: HybridRepository로 Firebase 호출 비용 절감"
type: feat
date: 2026-02-04
brainstorm: Docs/brainstorms/2026-02-04-hybrid-repository-brainstorm.md
revision: v3 (SyncCoordinator + WriteBatch 추가)
---

# feat: HybridRepository로 Firebase 호출 비용 절감

## Overview

로컬 저장소를 기본으로 사용하되, 디바운스 + 횟수 임계값 조건에 따라 Firebase에 동기화하는 `HybridRepository<T>` 시스템을 구현한다. 이를 통해 Firebase Firestore API 호출 횟수를 대폭 줄여 운영 비용을 절감한다.

## Problem Statement

현재 아키텍처에서는 Firebase 또는 Local 중 하나만 사용하는 fallback 패턴이다. Firebase를 사용할 때 매 `Save()` 호출마다 Firestore에 쓰기가 발생한다. 클리커 게임 특성상 `CurrencyManager.Add()`가 초당 여러 번 호출되어 Firebase 쓰기 비용이 과다하다.

## Proposed Solution

`HybridRepository<T>`가 Local과 Firebase 레포지토리를 내부적으로 조합하여:
- **Save**: 항상 로컬 즉시 저장 + SyncCoordinator에 pending 등록
- **Load**: 양쪽 모두 로드하여 타임스탬프(초 단위) 비교 후 최신 데이터 사용
- **Sync**: SyncCoordinator가 디바운스(1초) AND 카운트(5회) 조건 충족 시 **WriteBatch**로 원자적 커밋

> **v3 변경**: SyncCoordinator + WriteBatch 패턴 추가 (Currency + Upgrade 원자적 동기화)

## Technical Approach

### Architecture (v3)

```
┌─────────────────────────────────────────────────────────┐
│          Manager (Currency/Upgrade)                      │
│          ServiceLocator.Get<IRepository>()               │
└────────────────────┬────────────────────────────────────┘
                     │
     ┌───────────────┴───────────────┐
     ▼                               ▼
┌─────────────────────┐    ┌─────────────────────┐
│HybridCurrencyRepo   │    │HybridUpgradeRepo    │
│                     │    │                     │
│ - Local 즉시 저장    │    │ - Local 즉시 저장    │
│ - Pending 등록       │    │ - Pending 등록       │
└─────────┬───────────┘    └─────────┬───────────┘
          │                          │
          │    RegisterPending()     │
          └──────────┬───────────────┘
                     ▼
┌─────────────────────────────────────────────────────────┐
│                  SyncCoordinator                         │
│                                                          │
│  - 단일 Debounce 타이머 (1초)                            │
│  - 전체 Save 카운트 (5회 임계값)                          │
│  - WriteBatch로 Currency + Upgrade 원자적 커밋           │
│                                                          │
│  FlushAll() → WriteBatch.Set(...) → CommitAsync()       │
└─────────────────────────────────────────────────────────┘
```

**v3 핵심 변경:**
- `HybridRepository<T>`는 로컬 저장 + pending 관리만 담당
- `SyncCoordinator`가 **모든 Repository의 pending items**를 모아서 **단일 WriteBatch**로 커밋
- 업그레이드 구매 시 Currency + Upgrade가 **원자적으로** Firebase에 저장됨

---

### Implementation Phases

#### Phase 1: 도메인 모델 확장

SaveData에 타임스탬프 필드를 추가하고, 인터페이스를 정의한다.

**1-1. `ITimestamped` 인터페이스 생성**

```csharp
// Assets/02.Scripts/Core/Common/ITimestamped.cs (신규)
public interface ITimestamped
{
    long LastModified { get; set; }
}
```

**1-2. `ISyncable<T>` 인터페이스 생성**

```csharp
// Assets/02.Scripts/Core/Common/ISyncable.cs (신규)
public interface ISyncable<T> where T : IIdentifiable
{
    /// <summary>
    /// Pending items를 반환하고 내부 큐를 비움
    /// </summary>
    IReadOnlyDictionary<string, T> TakeAndClearPendingItems();

    /// <summary>
    /// Sync 실패 시 아이템을 다시 큐에 추가
    /// </summary>
    void RequeueItems(IEnumerable<KeyValuePair<string, T>> items);
}
```

> **v3 추가**: `ISyncable<T>`는 SyncCoordinator가 HybridRepository의 pending items에 접근하기 위한 계약

**1-3. `CurrencySaveData` 수정**

```csharp
// Assets/02.Scripts/Outgame/Currency/Domain/CurrencySaveData.cs (수정)
// LastModified 필드 추가
[SerializeField] private long _lastModified;

[FirestoreProperty("lastModified")]
public long LastModified { get => _lastModified; set => _lastModified = value; }
```

- `ITimestamped` 인터페이스 구현 추가
- `[Serializable]` + `[FirestoreData]` 듀얼 직렬화 유지

**1-4. `UpgradeSaveData` 수정**

```csharp
// Assets/02.Scripts/Outgame/Upgrade/Domain/UpgradeSaveData.cs (수정)
// CurrencySaveData와 동일하게 LastModified 추가, ITimestamped 구현
```

**1-5. `IFirebaseStoreService` 확장 (WriteBatch 지원)**

```csharp
// Assets/02.Scripts/Core/Firebase/Service/IFirebaseStoreService.cs (수정)
public interface IFirebaseStoreService
{
    // 기존 메서드들...

    /// <summary>
    /// WriteBatch 인스턴스 생성
    /// </summary>
    IWriteBatchWrapper CreateWriteBatch();
}

/// <summary>
/// Firestore WriteBatch 래퍼 인터페이스
/// </summary>
public interface IWriteBatchWrapper
{
    void Set<T>(string collection, string documentId, T data);
    UniTask CommitAsync();
}
```

**1-6. `FirebaseStoreService` 수정 (WriteBatch 구현)**

```csharp
// Assets/02.Scripts/Core/Firebase/Service/FirebaseStoreService.cs (수정)
public IWriteBatchWrapper CreateWriteBatch()
{
    return new FirestoreWriteBatchWrapper(_firestore.StartBatch());
}

private class FirestoreWriteBatchWrapper : IWriteBatchWrapper
{
    private readonly WriteBatch _batch;

    public FirestoreWriteBatchWrapper(WriteBatch batch) => _batch = batch;

    public void Set<T>(string collection, string documentId, T data)
    {
        var docRef = _firestore.Collection(collection).Document(documentId);
        _batch.Set(docRef, data);
    }

    public async UniTask CommitAsync()
    {
        await _batch.CommitAsync();
    }
}
```

**수정 대상 파일:**
- `Assets/02.Scripts/Core/Common/ITimestamped.cs` (신규)
- `Assets/02.Scripts/Core/Common/ISyncable.cs` (신규)
- `Assets/02.Scripts/Outgame/Currency/Domain/CurrencySaveData.cs:8-23` (수정)
- `Assets/02.Scripts/Outgame/Upgrade/Domain/UpgradeSaveData.cs:8-19` (수정)
- `Assets/02.Scripts/Core/Firebase/Service/IFirebaseStoreService.cs:6-13` (수정)
- `Assets/02.Scripts/Core/Firebase/Service/FirebaseStoreService.cs` (수정)

---

#### Phase 2: HybridRepository\<T\> 핵심 구현

로컬 저장 + pending 관리를 담당하는 제네릭 레포지토리 클래스.

**2-1. `HybridRepository<T>` 클래스 (v3 - sync 로직 분리)**

```csharp
// Assets/02.Scripts/Outgame/Common/Repository/HybridRepository.cs (신규)
public class HybridRepository<T> : IRepository<T>, ISyncable<T>, IDisposable
    where T : IIdentifiable, ITimestamped
{
    private readonly IRepository<T> _localRepository;
    private readonly IRepository<T> _firebaseRepository;
    private readonly Action _onPendingAdded; // SyncCoordinator에 알림
    private readonly object _syncLock = new object();

    private Dictionary<string, T> _pendingItems = new();

    public HybridRepository(
        IRepository<T> localRepository,
        IRepository<T> firebaseRepository,
        Action onPendingAdded)
    {
        _localRepository = localRepository;
        _firebaseRepository = firebaseRepository;
        _onPendingAdded = onPendingAdded;
    }

    // === Initialize ===
    public async UniTask<List<T>> Initialize()
    {
        // 1. 로컬 로드
        var localItems = await _localRepository.Initialize();

        // 2. Firebase 로드 (실패 시 로컬만 사용)
        List<T> firebaseItems;
        try
        {
            firebaseItems = await _firebaseRepository.Initialize();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Firebase 로드 실패, 로컬만 사용: {ex.Message}");
            return localItems;
        }

        // 3. 항목별 타임스탬프 비교 → 최신 데이터 선택
        return MergeByTimestamp(localItems, firebaseItems);
    }

    // === Save (로컬 + pending 등록만) ===
    public void Save(T item)
    {
        // 1. 타임스탬프 부여
        item.LastModified = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 2. 로컬 즉시 저장
        _localRepository.Save(item);

        // 3. Pending에 추가
        lock (_syncLock)
        {
            _pendingItems[item.Id] = item;
        }

        // 4. SyncCoordinator에 알림
        _onPendingAdded?.Invoke();
    }

    // === ISyncable<T> 구현 ===
    public IReadOnlyDictionary<string, T> TakeAndClearPendingItems()
    {
        lock (_syncLock)
        {
            var snapshot = _pendingItems;
            _pendingItems = new Dictionary<string, T>();
            return snapshot;
        }
    }

    public void RequeueItems(IEnumerable<KeyValuePair<string, T>> items)
    {
        lock (_syncLock)
        {
            foreach (var kvp in items)
            {
                if (!_pendingItems.ContainsKey(kvp.Key))
                    _pendingItems[kvp.Key] = kvp.Value;
            }
        }
    }

    // === Timestamp 기반 병합 ===
    private List<T> MergeByTimestamp(List<T> local, List<T> firebase)
    {
        var merged = new Dictionary<string, T>();

        foreach (var item in local)
            merged[item.Id] = item;

        foreach (var fbItem in firebase)
        {
            if (!merged.TryGetValue(fbItem.Id, out var localItem)
                || fbItem.LastModified >= localItem.LastModified)
            {
                merged[fbItem.Id] = fbItem;
            }
        }

        var result = merged.Values.ToList();
        foreach (var item in result)
            _localRepository.Save(item);

        return result;
    }

    public void Dispose() { /* 정리할 리소스 없음 - 타이머는 SyncCoordinator가 관리 */ }
}
```

**v3 변경 포인트:**
- debounce 타이머, sync 로직 **제거** → SyncCoordinator로 이동
- `ISyncable<T>` 구현으로 pending items 접근 제공
- `onPendingAdded` 콜백으로 SyncCoordinator에 알림

**수정 대상 파일:**
- `Assets/02.Scripts/Outgame/Common/Repository/HybridRepository.cs` (신규)

---

#### Phase 2.5: SyncCoordinator 구현 (v3 신규)

모든 HybridRepository의 pending items를 모아서 WriteBatch로 원자적 커밋.

**2.5-1. `SyncCoordinator` 클래스**

```csharp
// Assets/02.Scripts/Outgame/Common/Repository/SyncCoordinator.cs (신규)
public class SyncCoordinator : IDisposable
{
    private const float DEBOUNCE_DELAY_SECONDS = 1f;
    private const int SAVE_COUNT_THRESHOLD = 5;

    private readonly IFirebaseStoreService _storeService;
    private readonly ISyncable<CurrencySaveData> _currencySyncable;
    private readonly ISyncable<UpgradeSaveData> _upgradeSyncable;
    private readonly object _syncLock = new object();

    private int _totalSaveCount;
    private bool _isSyncInProgress;
    private CancellationTokenSource _debounceCts;

    public SyncCoordinator(
        IFirebaseStoreService storeService,
        ISyncable<CurrencySaveData> currencySyncable,
        ISyncable<UpgradeSaveData> upgradeSyncable)
    {
        _storeService = storeService;
        _currencySyncable = currencySyncable;
        _upgradeSyncable = upgradeSyncable;
    }

    /// <summary>
    /// HybridRepository.Save() 호출 시 실행됨
    /// </summary>
    public void OnPendingAdded()
    {
        lock (_syncLock)
        {
            _totalSaveCount++;
        }
        RestartDebounceTimer();
    }

    private void RestartDebounceTimer()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        StartDebounceTimer(_debounceCts.Token).Forget();
    }

    private async UniTaskVoid StartDebounceTimer(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(DEBOUNCE_DELAY_SECONDS),
                cancellationToken: cancellationToken
            );

            bool shouldSync;
            lock (_syncLock)
            {
                shouldSync = _totalSaveCount >= SAVE_COUNT_THRESHOLD && !_isSyncInProgress;
            }

            if (shouldSync)
            {
                await FlushAll();
            }
        }
        catch (OperationCanceledException)
        {
            // 타이머 취소됨 - 정상
        }
    }

    /// <summary>
    /// 모든 pending items를 WriteBatch로 원자적 커밋
    /// </summary>
    public async UniTask FlushAll()
    {
        Dictionary<string, CurrencySaveData> currencySnapshot;
        Dictionary<string, UpgradeSaveData> upgradeSnapshot;

        lock (_syncLock)
        {
            if (_isSyncInProgress)
                return;

            _isSyncInProgress = true;
            _totalSaveCount = 0;
        }

        // 스냅샷 획득 (각 Repository에서 pending 가져오기)
        currencySnapshot = new Dictionary<string, CurrencySaveData>(
            _currencySyncable.TakeAndClearPendingItems()
        );
        upgradeSnapshot = new Dictionary<string, UpgradeSaveData>(
            _upgradeSyncable.TakeAndClearPendingItems()
        );

        if (currencySnapshot.Count == 0 && upgradeSnapshot.Count == 0)
        {
            lock (_syncLock) { _isSyncInProgress = false; }
            return;
        }

        try
        {
            // WriteBatch로 원자적 커밋
            var batch = _storeService.CreateWriteBatch();

            foreach (var item in currencySnapshot.Values)
            {
                batch.Set("currencies", item.Id, item);
            }

            foreach (var item in upgradeSnapshot.Values)
            {
                batch.Set("upgrades", item.Id, item);
            }

            await batch.CommitAsync();

            Debug.Log($"[SyncCoordinator] WriteBatch 커밋 완료: " +
                      $"Currency {currencySnapshot.Count}건, Upgrade {upgradeSnapshot.Count}건");
        }
        catch (Exception ex)
        {
            // 실패 시 아이템을 다시 큐에 추가
            _currencySyncable.RequeueItems(currencySnapshot);
            _upgradeSyncable.RequeueItems(upgradeSnapshot);

            Debug.LogWarning($"[SyncCoordinator] WriteBatch 커밋 실패: {ex.Message}");
        }
        finally
        {
            lock (_syncLock)
            {
                _isSyncInProgress = false;
            }
        }
    }

    /// <summary>
    /// 앱 종료/일시정지 시 호출
    /// </summary>
    public void ForceFlush()
    {
        _debounceCts?.Cancel();
        FlushAll().Forget();
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;
    }
}
```

**핵심 설계:**

| 항목 | 결정 | 근거 |
|------|------|------|
| 단일 타이머 | SyncCoordinator가 전체 관리 | Repository별 타이머는 과도 |
| 전체 카운트 | Currency + Upgrade 합산 | 원자적 커밋이므로 통합 관리 |
| WriteBatch | 모든 pending을 한 번에 커밋 | 네트워크 1회, 원자적 일관성 |
| 스냅샷 방식 | TakeAndClear로 획득 | sync 중 새 Save 레이스 컨디션 방지 |

**수정 대상 파일:**
- `Assets/02.Scripts/Outgame/Common/Repository/SyncCoordinator.cs` (신규)

---

#### Phase 3: Provider 통합 + Bootstrap 연결

HybridRepositoryProvider에서 SyncCoordinator를 관리.

**3-1. `HybridRepositoryProvider` 클래스 (v3)**

```csharp
// Assets/02.Scripts/Outgame/Common/Repository/HybridRepositoryProvider.cs (신규)
public class HybridRepositoryProvider : IRepositoryProvider, IDisposable
{
    private readonly IFirebaseAuthService _authService;
    private readonly IFirebaseStoreService _storeService;

    private HybridCurrencyRepository _hybridCurrencyRepository;
    private HybridUpgradeRepository _hybridUpgradeRepository;
    private SyncCoordinator _syncCoordinator;

    public IAccountRepository AccountRepository { get; private set; }
    public ICurrencyRepository CurrencyRepository { get; private set; }
    public IUpgradeRepository UpgradeRepository { get; private set; }

    public HybridRepositoryProvider(
        IFirebaseAuthService authService,
        IFirebaseStoreService storeService)
    {
        _authService = authService;
        _storeService = storeService;
    }

    public UniTask Initialize()
    {
        // Account: Firebase 직접 사용 (Hybrid 대상 아님)
        AccountRepository = new FirebaseAccountRepository(_authService);

        // SyncCoordinator 생성 (먼저 생성해야 Repository가 참조 가능)
        // 하지만 Repository도 필요하므로 2단계로 진행

        // 1단계: Repository 생성 (SyncCoordinator 콜백은 나중에 연결)
        _hybridCurrencyRepository = new HybridCurrencyRepository(
            new LocalCurrencyRepository(),
            new FirebaseCurrencyRepository(_storeService)
        );
        CurrencyRepository = _hybridCurrencyRepository;

        _hybridUpgradeRepository = new HybridUpgradeRepository(
            new LocalUpgradeRepository(),
            new FirebaseUpgradeRepository(_storeService)
        );
        UpgradeRepository = _hybridUpgradeRepository;

        // 2단계: SyncCoordinator 생성 및 연결
        _syncCoordinator = new SyncCoordinator(
            _storeService,
            _hybridCurrencyRepository.GetSyncable(),
            _hybridUpgradeRepository.GetSyncable()
        );

        // 콜백 연결
        _hybridCurrencyRepository.SetSyncNotifier(_syncCoordinator.OnPendingAdded);
        _hybridUpgradeRepository.SetSyncNotifier(_syncCoordinator.OnPendingAdded);

        return UniTask.CompletedTask;
    }

    public void ForceFlushAll()
    {
        _syncCoordinator?.ForceFlush();
    }

    public void Dispose()
    {
        _syncCoordinator?.Dispose();
        _hybridCurrencyRepository?.Dispose();
        _hybridUpgradeRepository?.Dispose();
    }
}
```

**3-2. `HybridCurrencyRepository` 래퍼 (v3)**

```csharp
// Assets/02.Scripts/Outgame/Currency/Repository/HybridCurrencyRepository.cs (신규)
public class HybridCurrencyRepository : ICurrencyRepository, IDisposable
{
    private readonly HybridRepository<CurrencySaveData> _hybrid;

    public HybridCurrencyRepository(
        ICurrencyRepository localRepo,
        ICurrencyRepository firebaseRepo)
    {
        // 초기에는 콜백 없이 생성
        _hybrid = new HybridRepository<CurrencySaveData>(localRepo, firebaseRepo, null);
    }

    /// <summary>
    /// SyncCoordinator 연결 후 콜백 설정
    /// </summary>
    public void SetSyncNotifier(Action onPendingAdded)
    {
        _hybrid.SetOnPendingAdded(onPendingAdded);
    }

    /// <summary>
    /// SyncCoordinator가 pending items에 접근하기 위한 인터페이스
    /// </summary>
    public ISyncable<CurrencySaveData> GetSyncable() => _hybrid;

    public UniTask<List<CurrencySaveData>> Initialize() => _hybrid.Initialize();
    public void Save(CurrencySaveData item) => _hybrid.Save(item);
    public void Dispose() => _hybrid.Dispose();
}
```

> `HybridUpgradeRepository`도 동일한 패턴으로 구현

**3-3. `HybridRepository<T>` 콜백 설정 메서드 추가**

```csharp
// HybridRepository<T>에 추가
private Action _onPendingAdded;

public void SetOnPendingAdded(Action callback)
{
    _onPendingAdded = callback;
}
```

**3-4. `GameBootstrap` 수정**

```csharp
// Assets/02.Scripts/GameBootstrap.cs (수정)
private async UniTask<IRepositoryProvider[]> CreateProviders()
{
    var providers = new List<IRepositoryProvider>();

    try
    {
        var initializer = new FirebaseInitializer();
        await initializer.Initialize();

        providers.Add(new HybridRepositoryProvider(
            initializer.AuthService,
            initializer.StoreService
        ));
    }
    catch (Exception ex)
    {
        Debug.LogWarning($"Firebase 초기화 실패, Local 저장소로 전환: {ex.Message}");
    }

    providers.Add(new LocalRepositoryProvider());
    return providers.ToArray();
}
```

**수정 대상 파일:**
- `Assets/02.Scripts/Outgame/Common/Repository/HybridRepositoryProvider.cs` (신규)
- `Assets/02.Scripts/Outgame/Currency/Repository/HybridCurrencyRepository.cs` (신규)
- `Assets/02.Scripts/Outgame/Upgrade/Repository/HybridUpgradeRepository.cs` (신규)
- `Assets/02.Scripts/GameBootstrap.cs:52-69` (수정)

---

#### Phase 4: 라이프사이클 관리

앱 종료/일시정지 시 강제 flush.

**4-1. `GameBootstrap`에 라이프사이클 콜백 추가**

```csharp
// Assets/02.Scripts/GameBootstrap.cs (수정)
private HybridRepositoryProvider _hybridProvider;

private void OnApplicationPause(bool pauseStatus)
{
    if (!pauseStatus) return;
    _hybridProvider?.ForceFlushAll();
}

private void OnApplicationQuit()
{
    _hybridProvider?.ForceFlushAll();
    _hybridProvider?.Dispose();
}
```

**4-2. RepositoryFactory와 연동**

```csharp
// RepositoryFactory.Initialize() 수정
public async UniTask Initialize(params IRepositoryProvider[] providers)
{
    foreach (var provider in providers)
    {
        try
        {
            await provider.Initialize();

            if (provider is HybridRepositoryProvider hybridProvider)
            {
                CurrentHybridProvider = hybridProvider;
            }
            // ... 기존 로직 ...
        }
        catch { ... }
    }
}

public HybridRepositoryProvider CurrentHybridProvider { get; private set; }
```

**수정 대상 파일:**
- `Assets/02.Scripts/GameBootstrap.cs` (수정 — 라이프사이클 콜백)
- `Assets/02.Scripts/Outgame/Common/Repository/RepositoryFactory.cs` (수정 — HybridProvider 참조)

---

### 데이터 마이그레이션 전략

기존 세이브 데이터에 `LastModified` 필드가 없는 경우:

1. `LastModified == 0` (기본값)으로 로드됨
2. Initialize 시 Firebase 데이터의 `LastModified`도 0일 수 있음
3. **규칙**: 양쪽 모두 0이면 Firebase 데이터 우선 (동점 시 서버 신뢰)
4. 병합된 데이터에 현재 타임스탬프를 부여하고 양쪽에 저장
5. 이후 정상 HybridRepository 흐름으로 전환

---

## Acceptance Criteria

### Functional Requirements

- [ ] `HybridRepository<T>`가 `IRepository<T>` 인터페이스를 구현
- [ ] Save() 호출 시 로컬에 항상 즉시 저장
- [ ] 디바운스(1초) AND 카운트(5회) 조건 충족 시에만 Firebase 저장
- [ ] Initialize() 시 로컬+Firebase 양쪽 로드 후 타임스탬프 비교로 최신 선택
- [ ] 기존 Manager(CurrencyManager, UpgradeManager)가 코드 변경 없이 동작
- [ ] OnApplicationPause/Quit 시 강제 flush
- [ ] 기존 SaveData(타임스탬프 없는)에서 정상 마이그레이션
- [ ] **v3**: Currency + Upgrade가 WriteBatch로 원자적 커밋

### Non-Functional Requirements

- [ ] Save() 호출이 메인 스레드를 블로킹하지 않음
- [ ] 동시 sync 방지 (SyncCoordinator 단일 진입)
- [ ] CancellationTokenSource 정상 해제 (메모리 누수 없음)
- [ ] AccountRepository는 Hybrid 대상에서 제외

### Performance Criteria

- [ ] Save() 100회 호출 시 Firebase 쓰기 최대 20회 이하
- [ ] Save() 호출 후 1ms 이내 반환 (프로파일러 측정)
- [ ] **v3**: WriteBatch 커밋은 단일 네트워크 왕복

### Quality Gates

- [ ] 기존 CurrencyManager, UpgradeManager 코드 변경 없음
- [ ] 기존 Local/Firebase Repository 코드 변경 없음 (SaveData 필드 추가 제외)
- [ ] ServiceLocator 사용 패턴 변경 없음
- [ ] 기존 `IRepository<T>` 인터페이스 변경 없음

---

## Dependencies & Prerequisites

| 의존성 | 상태 | 비고 |
|--------|------|------|
| UniTask (Cysharp) | 이미 사용 중 | UniTask.Delay, CancellationTokenSource |
| Firebase Firestore SDK | 이미 사용 중 | WriteBatch, [FirestoreProperty] |

---

## Risk Analysis & Mitigation

| 위험 | 영향 | 완화 방안 |
|------|------|-----------|
| iOS 앱 서스펜션 시 flush 미완료 | 데이터 미동기화 | 다음 Initialize의 병합 로직이 자동 복구 |
| 디바운스 타이머 메모리 누수 | 앱 장기 실행 시 성능 저하 | Dispose 패턴 + OnDestroy 정리 |
| 기존 SaveData 마이그레이션 실패 | 데이터 손실 | Firebase 우선 정책 + 0 타임스탬프 처리 |
| sync 중 Save() 레이스 컨디션 | 데이터 유실 | TakeAndClear 스냅샷 방식 |
| WriteBatch 500 문서 제한 | 대량 변경 시 실패 | 클리커 게임 특성상 해당 없음 |

---

## File Changes Summary

### 신규 파일 (7개)
| 파일 | 위치 |
|------|------|
| `ITimestamped.cs` | `Assets/02.Scripts/Core/Common/` |
| `ISyncable.cs` | `Assets/02.Scripts/Core/Common/` |
| `HybridRepository.cs` | `Assets/02.Scripts/Outgame/Common/Repository/` |
| `SyncCoordinator.cs` | `Assets/02.Scripts/Outgame/Common/Repository/` |
| `HybridRepositoryProvider.cs` | `Assets/02.Scripts/Outgame/Common/Repository/` |
| `HybridCurrencyRepository.cs` | `Assets/02.Scripts/Outgame/Currency/Repository/` |
| `HybridUpgradeRepository.cs` | `Assets/02.Scripts/Outgame/Upgrade/Repository/` |

### 수정 파일 (5개)
| 파일 | 변경 내용 |
|------|-----------|
| `CurrencySaveData.cs` | `ITimestamped` 구현 + `LastModified` 필드 추가 |
| `UpgradeSaveData.cs` | `ITimestamped` 구현 + `LastModified` 필드 추가 |
| `IFirebaseStoreService.cs` | `CreateWriteBatch()`, `IWriteBatchWrapper` 추가 |
| `FirebaseStoreService.cs` | `IWriteBatchWrapper` 구현 |
| `GameBootstrap.cs` | `CreateProviders()` HybridProvider 추가 + 라이프사이클 콜백 |

### 변경 없는 파일
- `IRepository.cs`, `IIdentifiable.cs`, `ServiceLocator.cs`
- `ICurrencyRepository.cs`, `IUpgradeRepository.cs`
- `LocalCurrencyRepository.cs`, `LocalUpgradeRepository.cs`
- `FirebaseCurrencyRepository.cs`, `FirebaseUpgradeRepository.cs`
- `CurrencyManager.cs`, `UpgradeManager.cs`

---

## Build Sequence

```
Phase 1 (도메인 모델 + WriteBatch API)
    ↓
Phase 2 (HybridRepository)
    ↓
Phase 2.5 (SyncCoordinator) ← v3 신규
    ↓
Phase 3 (Provider 통합)
    ↓
Phase 4 (라이프사이클)
```

각 Phase 완료 후 컴파일 확인. Phase 3까지 완료하면 기본 동작 검증 가능.

---

## v3 변경 사항 (SyncCoordinator + WriteBatch)

### 추가된 기능
| 기능 | 설명 |
|------|------|
| `SyncCoordinator` | 전체 pending 관리 + WriteBatch 커밋 |
| `ISyncable<T>` | HybridRepository ↔ SyncCoordinator 인터페이스 |
| `IWriteBatchWrapper` | Firestore WriteBatch 추상화 |
| 원자적 커밋 | Currency + Upgrade 동시 저장 보장 |

### 아키텍처 변경
| 항목 | v2 | v3 |
|------|----|----|
| Sync 주체 | 각 HybridRepository | SyncCoordinator (중앙) |
| 타이머 | Repository별 | 전체 1개 |
| Firebase 호출 | 개별 Save | WriteBatch.CommitAsync |
| 원자성 | 없음 | Currency + Upgrade 함께 |

### 학습 포인트
- **Coordinator 패턴**: 여러 컴포넌트 간의 조율
- **WriteBatch**: Firestore 원자적 쓰기
- **ISyncable 인터페이스**: 의존성 역전 (DIP)
- **스냅샷 패턴**: 동시성 안전한 데이터 추출

---

## References

### Internal
- Brainstorm: `Docs/brainstorms/2026-02-04-hybrid-repository-brainstorm.md`
- IRepository 계약: `Assets/02.Scripts/Core/Common/IRepository.cs:6-10`
- IRepositoryProvider: `Assets/02.Scripts/Outgame/Common/Repository/IRepositoryProvider.cs:5-11`
- RepositoryFactory: `Assets/02.Scripts/Outgame/Common/Repository/RepositoryFactory.cs:12-31`
- GameBootstrap 부팅: `Assets/02.Scripts/GameBootstrap.cs:52-69`
- IFirebaseStoreService: `Assets/02.Scripts/Core/Firebase/Service/IFirebaseStoreService.cs:6-13`

### External
- [Firestore WriteBatch Documentation](https://firebase.google.com/docs/firestore/manage-data/transactions#batched-writes)
