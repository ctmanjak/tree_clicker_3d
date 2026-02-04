# Outgame 모듈 API 레퍼런스

> 비즈니스 도메인: 계정 관리, 재화 시스템, 업그레이드 시스템, 데이터 영속성, Firebase 서비스

## 목차

- [Common 인터페이스](#common-인터페이스)
- [Repository 시스템](#repository-시스템)
- [Firebase 서비스](#firebase-서비스)
- [Account 도메인](#account-도메인)
- [Currency 도메인](#currency-도메인)
- [Upgrade 도메인](#upgrade-도메인)

---

## Common 인터페이스

### IIdentifiable

**파일**: `Outgame/Common/IIdentifiable.cs`
**역할**: 고유 식별자를 가진 엔티티의 마커 인터페이스

```csharp
public interface IIdentifiable
{
    string Id { get; }
}
```

### IRepository\<T\>

**파일**: `Outgame/Common/IRepository.cs`
**역할**: 데이터 영속성 추상화를 위한 제네릭 인터페이스

```csharp
public interface IRepository<T>
{
    UniTask<List<T>> Initialize();
    void Save(T item);
}
```

### IRepositoryProvider

**파일**: `Outgame/Common/IRepositoryProvider.cs`
**역할**: 모든 Repository 구현체에 대한 팩토리 인터페이스

```csharp
public interface IRepositoryProvider
{
    IAccountRepository AccountRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IUpgradeRepository UpgradeRepository { get; }
    UniTask Initialize();
}
```

---

## Repository 시스템

### RepositoryFactory

**파일**: `Outgame/Common/RepositoryFactory.cs`
**역할**: Firebase → Local 순서로 Repository Provider 초기화 시도
**패턴**: Factory + Chain of Responsibility

| 프로퍼티 | 반환 타입 | 설명 |
|---|---|---|
| `AccountRepository` | `IAccountRepository` | 계정 저장소 |
| `CurrencyRepository` | `ICurrencyRepository` | 재화 저장소 |
| `UpgradeRepository` | `IUpgradeRepository` | 업그레이드 저장소 |

### LocalRepositoryProvider

**파일**: `Outgame/Common/LocalRepositoryProvider.cs`
**역할**: PlayerPrefs/File I/O 기반 로컬 저장소 제공

| 프로퍼티 | 반환 타입 | 구현체 |
|---|---|---|
| `AccountRepository` | `IAccountRepository` | `LocalAccountRepository` |
| `CurrencyRepository` | `ICurrencyRepository` | `LocalCurrencyRepository` |
| `UpgradeRepository` | `IUpgradeRepository` | `LocalUpgradeRepository` |

### FirebaseRepositoryProvider

**파일**: `Outgame/Common/FirebaseRepositoryProvider.cs`
**역할**: Firebase Firestore 기반 원격 저장소 제공

| 프로퍼티 | 반환 타입 | 구현체 |
|---|---|---|
| `AccountRepository` | `IAccountRepository` | `FirebaseAccountRepository` |
| `CurrencyRepository` | `ICurrencyRepository` | `FirebaseCurrencyRepository` |
| `UpgradeRepository` | `IUpgradeRepository` | `FirebaseUpgradeRepository` |

---

## Firebase 서비스

### FirebaseInitializer

**파일**: `Outgame/Firebase/FirebaseInitializer.cs`
**역할**: Firebase SDK 종속성 확인 및 Auth/Store 서비스 초기화

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `AuthService` | `IFirebaseAuthService` | 인증 서비스 인스턴스 |
| `StoreService` | `IFirebaseStoreService` | 저장소 서비스 인스턴스 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Initialize` | `UniTask Initialize()` | Firebase 초기화 |

### IFirebaseAuthService

**파일**: `Outgame/Firebase/Service/IFirebaseAuthService.cs`
**역할**: Firebase Authentication 서비스 계약

| 멤버 | 타입 | 설명 |
|---|---|---|
| `IsInitialized` | `bool` (property) | 초기화 완료 여부 |
| `CurrentUserId` | `string` (property) | 현재 로그인 사용자 ID |
| `IsLoggedIn` | `bool` (property) | 로그인 상태 |
| `Register` | `UniTask<FirebaseAuthResult>` | 이메일/비밀번호 회원가입 |
| `Login` | `UniTask<FirebaseAuthResult>` | 이메일/비밀번호 로그인 |
| `Logout` | `void` | 로그아웃 |

### IFirebaseStoreService

**파일**: `Outgame/Firebase/Service/IFirebaseStoreService.cs`
**역할**: Firebase Firestore 데이터베이스 서비스 계약

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetDocument<T>` | `UniTask SetDocument<T>(string collection, T data) where T : IIdentifiable` | 문서 저장/업데이트 |
| `GetCollection<T>` | `UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable` | 컬렉션 전체 조회 |
| `SetDocumentAsync<T>` | `void SetDocumentAsync<T>(string collection, T data) where T : IIdentifiable` | 비동기 저장 (fire-and-forget) |

### FirebaseAuthResult

**파일**: `Outgame/Firebase/Domain/FirebaseAuthResult.cs`
**역할**: Firebase 인증 결과 컨테이너 (값 객체)

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Success` | `bool` | 성공 여부 |
| `UserId` | `string` | 사용자 고유 ID |
| `Email` | `string` | 이메일 주소 |
| `ErrorMessage` | `string` | 실패 시 오류 메시지 |

### FirebaseAuthService

**파일**: `Outgame/Firebase/Service/FirebaseAuthService.cs`
**역할**: `IFirebaseAuthService`의 Firebase SDK 구현체
**구현**: `Firebase.Auth.FirebaseAuth` API 래핑

### FirebaseStoreService

**파일**: `Outgame/Firebase/Service/FirebaseStoreService.cs`
**역할**: `IFirebaseStoreService`의 Firestore SDK 구현체
**구현**: `Firebase.Firestore.FirebaseFirestore` API 래핑

---

## Account 도메인

### Account

**파일**: `Outgame/Account/Account.cs`
**역할**: 사용자 계정 도메인 모델. 이메일/비밀번호 유효성 검증 포함.

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Email` | `string` (read-only) | 이메일 주소 |
| `Password` | `string` (read-only) | 비밀번호 |

**유효성 검증 규칙**:
- 이메일: `EmailAccountSpecification` (RFC 5322 호환 정규식)
- 비밀번호: 6~15자, 대문자 1개 이상, 특수문자 1개 이상

### AuthResult

**파일**: `Outgame/Account/AuthResult.cs`
**역할**: 인증 작업 결과 컨테이너

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Success` | `bool` | 성공 여부 |
| `ErrorMessage` | `string` | 실패 시 오류 메시지 |
| `Account` | `Account` | 성공 시 계정 객체 |

### EmailAccountSpecification

**파일**: `Outgame/Account/EmailAccountSpecification.cs`
**역할**: 이메일 형식 검증 (Specification 패턴)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `IsSatisfiedBy` | `bool IsSatisfiedBy(string email)` | 이메일 형식 유효성 검사 |

### IAccountRepository

**파일**: `Outgame/Account/IAccountRepository.cs`
**역할**: 계정 저장소 인터페이스
**상속**: `IRepository<Account>`

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Register` | `UniTask<AuthResult> Register(string email, string password)` | 회원가입 |
| `Login` | `UniTask<AuthResult> Login(string email, string password)` | 로그인 |
| `Logout` | `UniTask Logout()` | 로그아웃 |
| `Initialize` | `UniTask<Account[]> Initialize()` | 저장된 계정 로드 |
| `Save` | `UniTask Save(Account account)` | 계정 저장 |

### LocalAccountRepository

**파일**: `Outgame/Account/LocalAccountRepository.cs`
**역할**: PlayerPrefs 기반 로컬 계정 저장소
**보안**: Salt + SHA256 해시로 비밀번호 저장 (`Crypto` 클래스 사용)

### FirebaseAccountRepository

**파일**: `Outgame/Account/FirebaseAccountRepository.cs`
**역할**: Firebase Authentication 기반 원격 계정 저장소
**위임**: `IFirebaseAuthService`에 인증 로직 위임

### AccountManager

**파일**: `Outgame/Account/AccountManager.cs`
**역할**: 현재 로그인 계정 관리 및 인증 흐름 조율
**패턴**: Singleton MonoBehaviour

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `CurrentAccount` | `Account` (read-only) | 현재 로그인된 계정 |
| `IsLoggedIn` | `bool` (read-only) | 로그인 상태 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `TryLogin` | `UniTask<bool> TryLogin(string email, string password)` | 로그인 시도 |
| `TryRegister` | `UniTask<bool> TryRegister(string email, string password)` | 회원가입 시도 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnLoginSuccess` | `Action` | 로그인 성공 시 발생 |
| `OnLogoutSuccess` | `Action` | 로그아웃 성공 시 발생 |
| `OnAuthError` | `Action<string>` | 인증 오류 시 발생 (오류 메시지 포함) |

---

## Currency 도메인

### CurrencyType

**파일**: `Outgame/Currency/CurrencyType.cs`
**역할**: 게임 재화 타입 열거형

| 값 | 정수값 | 설명 |
|---|---|---|
| `Wood` | `0` | 목재 (기본 재화) |

### CurrencyValue

**파일**: `Outgame/Currency/CurrencyValue.cs`
**역할**: 불변 재화 값 객체. 연산자 오버로딩, 비교, 형식화 출력 지원.
**패턴**: Value Object, IEquatable, IComparable

| 정적 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Zero` | `CurrencyValue` | 0 값 |
| `One` | `CurrencyValue` | 1 값 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `ToFormattedString` | `string ToFormattedString()` | `B`/`M`/`K` 형식 문자열 |
| `Equals` | `bool Equals(CurrencyValue other)` | 값 비교 |
| `CompareTo` | `int CompareTo(CurrencyValue other)` | 크기 비교 |

**연산자**: `+`, `-`, `*`, `/`, `==`, `!=`, `>`, `<`, `>=`, `<=`, `* double`, `double *`, `(double)` 캐스트

**형식화 규칙**:

| 범위 | 포맷 | 예시 |
|---|---|---|
| >= 1,000,000,000 | `{n:F1}B` | `1.5B` |
| >= 1,000,000 | `{n:F1}M` | `2.3M` |
| >= 1,000 | `{n:F1}K` | `4.7K` |
| < 1,000 | 정수 | `999` |

### Currency

**파일**: `Outgame/Currency/Currency.cs`
**역할**: 개별 재화 인스턴스 도메인 모델

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Type` | `CurrencyType` (read-only) | 재화 타입 |
| `Amount` | `CurrencyValue` (read-only) | 현재 보유량 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `CanAfford` | `bool CanAfford(CurrencyValue cost)` | 비용 지불 가능 여부 |
| `TrySpend` | `bool TrySpend(CurrencyValue amount)` | 지불 시도 (성공 시 차감) |
| `Add` | `void Add(CurrencyValue amount)` | 재화 추가 |
| `SetAmount` | `void SetAmount(CurrencyValue amount)` | 직접 설정 |

### CurrencySaveData

**파일**: `Outgame/Currency/CurrencySaveData.cs`
**역할**: 재화 영속성용 직렬화 데이터 모델
**상속**: `IIdentifiable`

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Id` | `string` | 고유 식별자 |
| `Type` | `CurrencyType` | 재화 타입 |
| `Amount` | `double` | 보유량 |

### ICurrencyRepository

**파일**: `Outgame/Currency/ICurrencyRepository.cs`
**상속**: `IRepository<CurrencySaveData>`

### CurrencyManager

**파일**: `Outgame/Currency/CurrencyManager.cs`
**역할**: 모든 게임 재화를 관리하고 영속성을 처리하는 중앙 매니저
**패턴**: Singleton MonoBehaviour

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `GetCurrency` | `Currency GetCurrency(CurrencyType type)` | 재화 인스턴스 조회 |
| `CanAfford` | `bool CanAfford(CurrencyType type, CurrencyValue cost)` | 비용 지불 가능 여부 |
| `Spend` | `bool Spend(CurrencyType type, CurrencyValue amount)` | 재화 지출 |
| `Add` | `void Add(CurrencyType type, CurrencyValue amount)` | 재화 획득 |

| 이벤트 | 타입 | 발생 시점 |
|---|---|---|
| `OnCurrencyChanged` | `Action<CurrencyType, CurrencyValue>` | 재화 변동 시 (지출/획득 모두) |
| `OnCurrencyAdded` | `Action<CurrencyType, CurrencyValue>` | 재화 획득 시에만 |

**이벤트 구독자**:

```
OnCurrencyChanged ──→ WoodCounterUI (텍스트 업데이트)
                 ──→ UpgradeButtonUI (구매 가능 상태 갱신)

OnCurrencyAdded  ──→ FloatingTextSpawner (플로팅 텍스트)
                 ──→ WoodCounterAnimator (펀치 애니메이션)
```

---

## Upgrade 도메인

### UpgradeSpecData

**파일**: `Outgame/Upgrade/Domain/UpgradeSpecData.cs`
**역할**: ScriptableObject 기반 업그레이드 설정 데이터
**패턴**: Configuration/Specification

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Id` | `string` | 고유 식별자 |
| `UpgradeName` | `string` | 표시 이름 |
| `Description` | `string` | 설명 텍스트 |
| `Icon` | `Sprite` | 아이콘 이미지 |
| `Type` | `UpgradeType` | 업그레이드 타입 |
| `BaseCost` | `CurrencyValue` | 기본 비용 |
| `CostMultiplier` | `float` | 비용 증가 배율 |
| `MaxLevel` | `int` | 최대 레벨 |
| `EffectAmount` | `CurrencyValue` | 효과 수치 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `GetCost` | `CurrencyValue GetCost(int level)` | 레벨별 비용 계산 |
| `IsMaxLevel` | `bool IsMaxLevel(int level)` | 최대 레벨 도달 여부 |

**비용 공식**:

```
Cost(level) = BaseCost * (CostMultiplier ^ level) * (1 + 0.3 * sin(level * π / 4))
```

### Upgrade

**파일**: `Outgame/Upgrade/Domain/Upgrade.cs`
**역할**: 업그레이드 도메인 모델. 레벨 진행 및 효과 적용 관리.

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Id` | `string` (read-only) | 고유 식별자 |
| `Name` | `string` (read-only) | 표시 이름 |
| `Description` | `string` (read-only) | 설명 텍스트 |
| `Icon` | `Sprite` (read-only) | 아이콘 |
| `Type` | `UpgradeType` (read-only) | 업그레이드 타입 |
| `Level` | `int` (read-only) | 현재 레벨 |
| `IsMaxLevel` | `bool` (read-only) | 최대 레벨 도달 여부 |
| `CurrentCost` | `CurrencyValue` (read-only) | 다음 레벨 비용 |
| `EffectAmount` | `CurrencyValue` (read-only) | 효과 수치 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetLevel` | `void SetLevel(int level)` | 레벨 설정 (검증 포함) |
| `IncrementLevel` | `void IncrementLevel()` | 레벨 1 증가 |
| `ApplyEffect` | `void ApplyEffect()` | 효과 핸들러 트리거 |
| `GetEffectText` | `string GetEffectText()` | UI용 효과 설명 문자열 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnLevelChanged` | `Action<Upgrade>` | 레벨 변경 시 발생 |

### IUpgradeEffectHandler

**파일**: `Outgame/Upgrade/Effect/IUpgradeEffectHandler.cs`
**역할**: 업그레이드 효과 처리 계약 (Strategy 패턴)

```csharp
public interface IUpgradeEffectHandler
{
    void OnInitialLoad(IEnumerable<Upgrade> upgrades);  // 세이브 로드 시
    void OnEffectApplied(Upgrade upgrade);               // 구매 시
    string GetEffectText(Upgrade upgrade);               // UI 텍스트
}
```

### 효과 핸들러 구현체

#### WoodPerClickEffectHandler

**파일**: `Outgame/Upgrade/Effect/WoodPerClickEffectHandler.cs`
**역할**: 클릭당 목재 획득량 보너스 누적

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `WoodPerClick` | `CurrencyValue` (read-only) | 누적된 클릭당 목재 |

**계산 로직**: 모든 WoodPerClick 타입 업그레이드의 `EffectAmount * Level` 합산

**효과 텍스트**: `"+{EffectAmount}"`

#### LumberjackProductionEffectHandler

**파일**: `Outgame/Upgrade/Effect/LumberjackProductionEffectHandler.cs`
**역할**: 벌목꾼 개당 생산량 보너스 누적

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `LumberjackProduction` | `CurrencyValue` (read-only) | 벌목꾼 개당 생산량 |

**계산 로직**: `1.0 + sum(EffectAmount * Level)` (기본 생산량 1.0 포함)

**효과 텍스트**: `"+{EffectAmount}/벌목꾼"`

**부수 효과**: 구매 시 `LumberjackSpawner.UpdateAllLumberjackStats()` 호출하여 모든 활성 벌목꾼 업데이트

#### SpawnLumberjackEffectHandler

**파일**: `Outgame/Upgrade/Effect/SpawnLumberjackEffectHandler.cs`
**역할**: 업그레이드 구매 시 벌목꾼 스폰

**초기 로드**: 누적된 SpawnLumberjack 레벨 수만큼 벌목꾼 스폰
**구매 시**: 벌목꾼 1체 스폰
**효과 텍스트**: `"벌목꾼 +1"`

### UpgradeSaveData

**파일**: `Outgame/Upgrade/Repository/UpgradeSaveData.cs`
**역할**: 업그레이드 영속성용 직렬화 데이터

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Id` | `string` | 업그레이드 고유 ID |
| `Level` | `int` | 현재 레벨 |

### UpgradeManager

**파일**: `Outgame/Upgrade/Manager/UpgradeManager.cs`
**역할**: 업그레이드 시스템 중앙 매니저. 효과 핸들러 등록, 구매 처리, 조회 기능.
**패턴**: Singleton MonoBehaviour, Strategy Pattern
**실행 순서**: `-50` (다른 MonoBehaviour보다 먼저 초기화)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `GetUpgrade` | `Upgrade GetUpgrade(string upgradeId)` | ID로 업그레이드 조회 |
| `GetUpgradesByType` | `IEnumerable<Upgrade> GetUpgradesByType(UpgradeType type)` | 타입별 업그레이드 목록 |
| `GetAllUpgrades` | `IEnumerable<Upgrade> GetAllUpgrades()` | 전체 업그레이드 목록 |
| `CanPurchase` | `bool CanPurchase(Upgrade upgrade)` | 구매 가능 여부 |
| `TryPurchase` | `bool TryPurchase(Upgrade upgrade)` | 구매 시도 (비용 차감 + 레벨 증가 + 효과 적용) |
| `GetEffectHandler<T>` | `T GetEffectHandler<T>()` | 구체 타입으로 핸들러 조회 |
| `GetWoodPerClick` | `CurrencyValue GetWoodPerClick()` | 현재 클릭당 목재 조회 |
| `GetLumberjackProduction` | `CurrencyValue GetLumberjackProduction()` | 현재 벌목꾼 생산량 조회 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnUpgradePurchased` | `Action<Upgrade>` | 구매 성공 시 발생 |

**핸들러 등록 매핑**:

| UpgradeType | 핸들러 |
|---|---|
| `WoodPerClick` | `WoodPerClickEffectHandler` |
| `LumberjackProduction` | `LumberjackProductionEffectHandler` |
| `SpawnLumberjack` | `SpawnLumberjackEffectHandler` |

**구매 흐름** (`TryPurchase`):

```
1. CanPurchase(upgrade) 확인
2. CurrencyManager.Spend(Wood, upgrade.CurrentCost)
3. upgrade.IncrementLevel()
4. upgrade.ApplyEffect() → IUpgradeEffectHandler.OnEffectApplied()
5. Repository.Save(UpgradeSaveData)
6. OnUpgradePurchased 이벤트 발행
```
