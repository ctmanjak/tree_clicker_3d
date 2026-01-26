---
title: "M1 프로토타입 구현"
type: feat
date: 2026-01-26
milestone: M1
duration: 2주 (Week 1-2)
architecture: extensibility-first
---

# M1: 프로토타입 구현 계획

## Overview

벌목왕(Lumber Tycoon) 게임의 핵심 메카닉을 검증하기 위한 프로토타입을 구현합니다. 클릭으로 목재를 수집하고, 기본 업그레이드와 벌목꾼 소환이 동작하는 플레이 가능한 빌드를 목표로 합니다.

### 아키텍처 원칙 (확장성 우선)

- **ServiceLocator 패턴**: 의존성 관리 및 테스트 용이성
- **인터페이스 레이어**: M2+ 확장을 위한 ISaveable, IClickable 인터페이스
- **인스턴스 기반 이벤트**: GameEvents 클래스를 통한 메모리 누수 방지
- **오브젝트 풀링 기초**: FloatingText에 적용, M2+ 확장 대비

## 목표

- 핵심 게임 루프 검증: 클릭 → 수집 → 투자 → 강화
- 기본 타격감(Game Feel) 구현
- 1개 클릭 업그레이드 + 1명 벌목꾼 동작 확인
- M2+ 확장을 위한 아키텍처 기초 마련

---

## Phase 1: 프로젝트 기반 설정 (Week 1, Day 1-2)

### Task 1.1: 프로젝트 구조 설정

**목표:** Unity 프로젝트의 기본 폴더 구조와 설정을 완료합니다.

**작업 내용:**

- [ ] Assets/02.Scripts 하위 폴더 구조 생성
  - `Core/` - 핵심 시스템 (GameManager, ServiceLocator, GameEvents)
  - `Interfaces/` - 인터페이스 정의 (ISaveable, IClickable)
  - `Services/` - 서비스 클래스 (WoodService, UpgradeService)
  - `Tree/` - 나무 관련 스크립트
  - `Player/` - 플레이어 입력 및 도끼 관련
  - `Economy/` - 재화 및 업그레이드 시스템
  - `Lumberjack/` - 벌목꾼 AI
  - `UI/` - UI 관련 스크립트
  - `Effects/` - 파티클, 피드백 효과
  - `Utils/` - 유틸리티 클래스 (ObjectPool)

- [ ] 기본 씬 구성 (GameScene.unity)
  - Main Camera (Orthographic 또는 Perspective 16:9)
  - Directional Light
  - Ground Plane
  - Tree Placeholder

**산출물:**

```
Assets/02.Scripts/
├── Core/
│   ├── GameManager.cs
│   ├── ServiceLocator.cs
│   └── GameEvents.cs
├── Interfaces/
│   ├── ISaveable.cs
│   └── IClickable.cs
├── Services/
├── Tree/
├── Player/
├── Economy/
├── Lumberjack/
├── UI/
├── Effects/
└── Utils/
    └── ObjectPool.cs
```

### Task 1.2: 카메라 및 렌더링 설정

**목표:** 16:9 가로 화면에 최적화된 카메라 설정

**작업 내용:**

- [ ] Main Camera 설정 (Inspector에서 직접 설정)
  - Projection: Perspective (3D 느낌) 또는 Orthographic (2D 느낌)
  - Field of View: 60
  - Clear Flags: Skybox 또는 Solid Color
  - Transform 고정 배치 (별도 스크립트 불필요)

- [ ] URP 설정 확인 (이미 설정된 경우 스킵)

> **Note:** 고정 카메라이므로 별도 CameraController 불필요. M3에서 카메라 효과 필요시 추가.

---

## Phase 2: 핵심 시스템 구현 (Week 1, Day 2-4)

### Task 1.2.1: 핵심 인프라 구축

**목표:** ServiceLocator, GameEvents, 인터페이스 정의

**파일:** `Assets/02.Scripts/Core/ServiceLocator.cs`

```csharp
using System;
using System.Collections.Generic;

/// <summary>
/// 간단한 ServiceLocator 패턴 구현
/// M2+에서 DI 컨테이너로 교체 가능
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out var service))
        {
            return service as T;
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    public static bool TryGet<T>(out T service) where T : class
    {
        if (services.TryGetValue(typeof(T), out var obj))
        {
            service = obj as T;
            return true;
        }
        service = null;
        return false;
    }

    public static void Clear()
    {
        services.Clear();
    }
}
```

**파일:** `Assets/02.Scripts/Core/GameEvents.cs`

```csharp
using System;
using UnityEngine;

/// <summary>
/// 중앙화된 게임 이벤트 시스템
/// 인스턴스 기반으로 메모리 누수 방지
/// </summary>
public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }

    // 재화 이벤트
    public event Action<long> OnWoodChanged;
    public event Action<long> OnWoodAdded;

    // 클릭 이벤트
    public event Action<Vector3> OnClickPerformed;
    public event Action<GameObject> OnTreeClicked;
    public event Action OnTreeHit;

    // 업그레이드 이벤트
    public event Action<string, int> OnUpgradePurchased;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 이벤트 발행 메서드
    public void RaiseWoodChanged(long amount) => OnWoodChanged?.Invoke(amount);
    public void RaiseWoodAdded(long amount) => OnWoodAdded?.Invoke(amount);
    public void RaiseClickPerformed(Vector3 pos) => OnClickPerformed?.Invoke(pos);
    public void RaiseTreeClicked(GameObject tree) => OnTreeClicked?.Invoke(tree);
    public void RaiseTreeHit() => OnTreeHit?.Invoke();
    public void RaiseUpgradePurchased(string id, int level) => OnUpgradePurchased?.Invoke(id, level);
}
```

**파일:** `Assets/02.Scripts/Interfaces/ISaveable.cs`

```csharp
/// <summary>
/// M2 저장 시스템을 위한 인터페이스
/// M1에서는 정의만 하고, M2에서 구현
/// </summary>
public interface ISaveable
{
    string SaveKey { get; }
    object CaptureState();
    void RestoreState(object state);
}
```

**파일:** `Assets/02.Scripts/Interfaces/IClickable.cs`

```csharp
using UnityEngine;

/// <summary>
/// 클릭 가능한 오브젝트 인터페이스
/// </summary>
public interface IClickable
{
    void OnClick(Vector3 hitPoint);
}

### Task 1.3: 입력 시스템 구현

**목표:** 마우스 클릭/터치 입력을 감지하고 이벤트로 전달합니다.

**작업 내용:**

- [ ] New Input System 활용 (이미 프로젝트에 포함됨)
- [ ] 클릭 위치의 오브젝트 감지 (Raycast)
- [ ] GameEvents를 통한 이벤트 발행

**파일:** `Assets/02.Scripts/Player/InputHandler.cs`

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 입력 처리 및 Raycast를 통한 클릭 대상 감지
/// </summary>
public class InputHandler : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayers;

    private Camera mainCamera;
    private GameEvents gameEvents;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! InputHandler requires a camera tagged 'MainCamera'.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        gameEvents = GameEvents.Instance;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(Mouse.current.position.ReadValue());
        }

        // 터치 지원
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayers))
        {
            gameEvents.RaiseClickPerformed(hit.point);

            // IClickable 인터페이스 지원
            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick(hit.point);
            }

            // 기존 태그 기반 호환성 유지
            if (hit.collider.CompareTag("Tree"))
            {
                gameEvents.RaiseTreeClicked(hit.collider.gameObject);
            }
        }
    }
}
```

### Task 1.4: 재화 시스템 (GameManager)

**목표:** 목재 재화를 관리하고, 변경 시 이벤트를 발행합니다.

**작업 내용:**

- [ ] 싱글톤 패턴 GameManager 구현 (올바른 Awake 초기화)
- [ ] 목재 수량 저장 및 GameEvents를 통한 이벤트 발행
- [ ] 클릭당 획득량 관리
- [ ] ServiceLocator에 등록

**파일:** `Assets/02.Scripts/Core/GameManager.cs`

```csharp
using UnityEngine;

/// <summary>
/// 게임 상태 관리자
/// 재화, 클릭당 획득량 등 핵심 게임 데이터 관리
/// </summary>
public class GameManager : MonoBehaviour, ISaveable
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private long currentWood = 0;
    [SerializeField] private long woodPerClick = 1;

    private GameEvents gameEvents;

    public long CurrentWood => currentWood;
    public long WoodPerClick => woodPerClick;

    // ISaveable 구현 (M2에서 활용)
    public string SaveKey => "GameManager";

    private void Awake()
    {
        // 싱글톤 초기화 (중복 체크 포함)
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ServiceLocator에 등록
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        gameEvents = GameEvents.Instance;
    }

    public void AddWood(long amount)
    {
        currentWood += amount;
        gameEvents?.RaiseWoodAdded(amount);
        gameEvents?.RaiseWoodChanged(currentWood);
    }

    public bool SpendWood(long amount)
    {
        if (currentWood >= amount)
        {
            currentWood -= amount;
            gameEvents?.RaiseWoodChanged(currentWood);
            return true;
        }
        return false;
    }

    public void IncreaseWoodPerClick(long amount)
    {
        woodPerClick += amount;
    }

    // ISaveable 구현
    public object CaptureState()
    {
        return new SaveData { wood = currentWood, woodPerClick = woodPerClick };
    }

    public void RestoreState(object state)
    {
        if (state is SaveData data)
        {
            currentWood = data.wood;
            woodPerClick = data.woodPerClick;
            gameEvents?.RaiseWoodChanged(currentWood);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public long wood;
        public long woodPerClick;
    }
}
```

### Task 1.5: 나무 오브젝트 및 클릭 로직

**목표:** 클릭 가능한 나무 오브젝트를 구현합니다.

**작업 내용:**

- [ ] 기존 숲 에셋에서 나무 모델 선택 (LowpolyNatureBundle 또는 Supercyan)
- [ ] Tree 태그 설정 + IClickable 인터페이스 구현
- [ ] 클릭 시 GameManager에 목재 추가 요청

**파일:** `Assets/02.Scripts/Tree/TreeController.cs`

```csharp
using UnityEngine;

/// <summary>
/// 나무 오브젝트 컨트롤러
/// IClickable 인터페이스로 클릭 처리
/// </summary>
public class TreeController : MonoBehaviour, IClickable
{
    [SerializeField] private TreeShake treeShake;
    [SerializeField] private HitParticleSpawner particleSpawner;

    private GameManager gameManager;
    private GameEvents gameEvents;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameEvents = GameEvents.Instance;

        // 컴포넌트 자동 탐색 (Inspector 설정 안된 경우)
        if (treeShake == null) treeShake = GetComponent<TreeShake>();
        if (particleSpawner == null) particleSpawner = GetComponent<HitParticleSpawner>();
    }

    /// <summary>
    /// IClickable 구현 - InputHandler에서 직접 호출
    /// </summary>
    public void OnClick(Vector3 hitPoint)
    {
        // 목재 획득
        gameManager.AddWood(gameManager.WoodPerClick);

        // 이벤트 발행 (UI, 이펙트 등에서 구독)
        gameEvents.RaiseTreeHit();

        // 나무 흔들림 (선택적)
        Vector3 hitDirection = (hitPoint - transform.position).normalized;
        treeShake?.Shake(hitDirection);
    }
}
```

### Task 1.6: 기본 UI - 목재 카운터

**목표:** 화면 상단에 현재 목재량을 표시합니다.

**작업 내용:**

- [ ] Canvas 생성 (Screen Space - Overlay)
- [ ] 목재 카운터 UI (아이콘 + 텍스트)
- [ ] 숫자 포맷팅 (1K, 1M 등)

**파일:** `Assets/02.Scripts/UI/WoodCounterUI.cs`

```csharp
using UnityEngine;
using TMPro;

/// <summary>
/// 목재 카운터 UI
/// GameEvents 구독하여 실시간 업데이트
/// </summary>
public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI woodText;

    private GameEvents gameEvents;

    private void Start()
    {
        gameEvents = GameEvents.Instance;
        // 초기값 표시
        UpdateDisplay(GameManager.Instance.CurrentWood);
    }

    private void OnEnable()
    {
        // Start 전에 호출될 수 있으므로 null 체크
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodChanged += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodChanged -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(long amount)
    {
        woodText.text = FormatNumber(amount);
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000_000) return $"{num / 1_000_000_000f:F1}B";
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
```

---

## Phase 3: 게임 필 구현 (Week 2, Day 1-2)

### Task 2.1: 타격 피드백 시스템

**목표:** 클릭 시 시각적 피드백으로 타격감을 제공합니다.

**작업 내용:**

- [ ] 화면 흔들림 (Camera Shake) - 0.05초, 2-5px
- [ ] 나무 흔들림 (Tree Shake) - 15도 기울임, 0.2초 복귀
- [ ] DOTween 또는 자체 구현

**파일:** `Assets/02.Scripts/Effects/ScreenShake.cs`

```csharp
// ScreenShake.cs - 기본 구조
public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.05f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private Vector3 originalPosition;

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
```

**파일:** `Assets/02.Scripts/Effects/TreeShake.cs`

```csharp
// TreeShake.cs - 기본 구조
public class TreeShake : MonoBehaviour
{
    [SerializeField] private float shakeAngle = 15f;
    [SerializeField] private float shakeDuration = 0.2f;

    private Quaternion originalRotation;

    public void Shake(Vector3 hitDirection)
    {
        StartCoroutine(ShakeCoroutine(hitDirection));
    }

    private IEnumerator ShakeCoroutine(Vector3 direction)
    {
        originalRotation = transform.rotation;

        // 반대 방향으로 기울임
        Quaternion targetRotation = Quaternion.Euler(
            originalRotation.eulerAngles + new Vector3(0, 0, shakeAngle * -Mathf.Sign(direction.x))
        );

        // 빠르게 기울임
        float elapsed = 0f;
        float tiltDuration = shakeDuration * 0.3f;
        while (elapsed < tiltDuration)
        {
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsed / tiltDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 천천히 복귀
        elapsed = 0f;
        float returnDuration = shakeDuration * 0.7f;
        while (elapsed < returnDuration)
        {
            transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, elapsed / returnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}
```

### Task 2.2: 플로팅 텍스트 시스템

**목표:** 클릭 위치에서 획득량이 위로 떠오르며 사라집니다.

**작업 내용:**

- [ ] ObjectPool 유틸리티 클래스 구현
- [ ] 플로팅 텍스트 풀링 적용
- [ ] 위로 이동하며 페이드아웃

**파일:** `Assets/02.Scripts/Utils/ObjectPool.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 오브젝트 풀링 시스템
/// M2+에서 파티클, 벌목꾼 등에도 활용
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> pool = new();
    private readonly List<T> activeObjects = new();

    public ObjectPool(T prefab, Transform parent, int initialSize = 10)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }
    }

    private T CreateNew()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        activeObjects.Remove(obj);
        pool.Enqueue(obj);
    }

    public void ReturnAll()
    {
        foreach (var obj in activeObjects.ToArray())
        {
            Return(obj);
        }
    }
}
```

**파일:** `Assets/02.Scripts/Effects/FloatingText.cs`

```csharp
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 풀링 지원 플로팅 텍스트
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 0.8f;

    private System.Action<FloatingText> onComplete;

    public void Initialize(System.Action<FloatingText> returnToPool)
    {
        onComplete = returnToPool;
    }

    public void Show(string content, Vector3 worldPosition)
    {
        text.text = content;
        text.color = Color.white;
        transform.position = worldPosition;
        StopAllCoroutines();
        StartCoroutine(AnimateAndHide());
    }

    private IEnumerator AnimateAndHide()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = text.color;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            transform.position = startPos + Vector3.up * floatSpeed * t;
            text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke(this);
    }
}
```

**파일:** `Assets/02.Scripts/Effects/FloatingTextSpawner.cs`

```csharp
using UnityEngine;

/// <summary>
/// 플로팅 텍스트 풀링 및 스폰 관리
/// </summary>
public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private FloatingText prefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 2f, 0);

    private ObjectPool<FloatingText> pool;
    private Transform treeTransform;

    private void Awake()
    {
        pool = new ObjectPool<FloatingText>(prefab, transform, poolSize);
    }

    private void Start()
    {
        treeTransform = GameObject.FindWithTag("Tree")?.transform;
        GameEvents.Instance.OnWoodAdded += SpawnText;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodAdded -= SpawnText;
        }
    }

    private void SpawnText(long amount)
    {
        if (treeTransform == null) return;

        var text = pool.Get();
        text.Initialize(ReturnToPool);

        // 약간의 랜덤 오프셋
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.2f, 0.2f),
            0
        );

        text.Show($"+{amount}", treeTransform.position + spawnOffset + randomOffset);
    }

    private void ReturnToPool(FloatingText text)
    {
        pool.Return(text);
    }
}
```

### Task 2.3: 파티클 시스템 기초

**목표:** 나무 타격 시 나무 조각과 잎사귀가 튀어나갑니다.

**작업 내용:**

- [ ] 나무 조각 파티클 (3-5개)
- [ ] 잎사귀 파티클 (2-3개)
- [ ] GameEvents.OnTreeHit 구독

**파일:** `Assets/02.Scripts/Effects/HitParticleSpawner.cs`

```csharp
using UnityEngine;

/// <summary>
/// 타격 파티클 스폰
/// GameEvents 구독하여 이벤트 기반 동작
/// </summary>
public class HitParticleSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem woodChipParticle;
    [SerializeField] private ParticleSystem leafParticle;

    private void Start()
    {
        GameEvents.Instance.OnTreeHit += SpawnParticles;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit -= SpawnParticles;
        }
    }

    private void SpawnParticles()
    {
        woodChipParticle?.Play();
        leafParticle?.Play();
    }
}
```

---

## Phase 4: 업그레이드 및 벌목꾼 (Week 2, Day 3-4)

### Task 2.4: 클릭 업그레이드 구현 (1개)

**목표:** 목재를 소비하여 클릭당 획득량을 증가시킵니다.

**작업 내용:**

- [ ] 업그레이드 데이터 구조 (ScriptableObject)
- [ ] 업그레이드 UI 버튼
- [ ] 구매 로직 및 비용 계산

**파일:** `Assets/02.Scripts/Economy/UpgradeData.cs`

```csharp
// UpgradeData.cs - ScriptableObject
[CreateAssetMenu(fileName = "Upgrade", menuName = "LumberTycoon/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public long baseCost;
    public long effectAmount;
    public float costMultiplier = 1.15f;
    public Sprite icon;

    public long GetCost(int currentLevel)
    {
        return (long)(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }
}
```

**파일:** `Assets/02.Scripts/Economy/UpgradeManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업그레이드 관리자
/// ISaveable 구현으로 M2 저장 시스템 대비
/// </summary>
public class UpgradeManager : MonoBehaviour, ISaveable
{
    [SerializeField] private UpgradeData sharpAxeUpgrade;

    private Dictionary<string, int> upgradeLevels = new(); // string ID로 저장 용이
    private GameManager gameManager;
    private GameEvents gameEvents;

    public string SaveKey => "UpgradeManager";

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameEvents = GameEvents.Instance;
    }

    public bool TryPurchase(UpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade);
        long cost = upgrade.GetCost(currentLevel);

        if (gameManager.SpendWood(cost))
        {
            upgradeLevels[upgrade.upgradeName] = currentLevel + 1;
            ApplyEffect(upgrade);
            gameEvents?.RaiseUpgradePurchased(upgrade.upgradeName, currentLevel + 1);
            return true;
        }
        return false;
    }

    private void ApplyEffect(UpgradeData upgrade)
    {
        gameManager.IncreaseWoodPerClick(upgrade.effectAmount);
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return upgradeLevels.TryGetValue(upgrade.upgradeName, out int level) ? level : 0;
    }

    // ISaveable 구현
    public object CaptureState()
    {
        return new Dictionary<string, int>(upgradeLevels);
    }

    public void RestoreState(object state)
    {
        if (state is Dictionary<string, int> savedLevels)
        {
            upgradeLevels = new Dictionary<string, int>(savedLevels);
        }
    }
}
```

**파일:** `Assets/02.Scripts/UI/UpgradeButtonUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 업그레이드 버튼 UI
/// Inspector 참조 사용 (FindObjectOfType 제거)
/// </summary>
public class UpgradeButtonUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UpgradeData upgradeData;

    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager; // Inspector에서 설정
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI levelText;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        // UpgradeManager가 Inspector에서 설정 안된 경우 ServiceLocator 사용
        if (upgradeManager == null)
        {
            ServiceLocator.TryGet(out upgradeManager);
        }

        button.onClick.AddListener(OnClick);
        GameEvents.Instance.OnWoodChanged += OnWoodChanged;
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodChanged -= OnWoodChanged;
        }
    }

    private void OnClick()
    {
        if (upgradeManager.TryPurchase(upgradeData))
        {
            UpdateDisplay();
            // TODO: 구매 성공 효과음/이펙트
        }
    }

    private void OnWoodChanged(long _)
    {
        UpdateButtonState();
    }

    private void UpdateDisplay()
    {
        int level = upgradeManager.GetLevel(upgradeData);
        long cost = upgradeData.GetCost(level);

        nameText.text = upgradeData.upgradeName;
        costText.text = $"Cost: {FormatNumber(cost)}";
        levelText.text = $"Lv.{level}";

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        int level = upgradeManager.GetLevel(upgradeData);
        long cost = upgradeData.GetCost(level);
        button.interactable = gameManager.CurrentWood >= cost;
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
```

### Task 2.5: 벌목꾼 소환 구현 (1명)

**목표:** 자동으로 나무를 베는 벌목꾼을 소환합니다.

**작업 내용:**

- [ ] 벌목꾼 프리팹 생성
- [ ] 기본 AI (나무 주변 이동 → 타격 → 반복)
- [ ] 자동 목재 수집 (1/초)
- [ ] 소환 업그레이드와 연동

**파일:** `Assets/02.Scripts/Lumberjack/LumberjackController.cs`

```csharp
// LumberjackController.cs - 기본 구조
public class LumberjackController : MonoBehaviour
{
    [SerializeField] private float woodPerSecond = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private Transform treeTarget;

    private enum State { Moving, Attacking, Idle }
    private State currentState = State.Idle;

    private float attackTimer;
    private float woodAccumulator;

    private void Update()
    {
        switch (currentState)
        {
            case State.Moving:
                MoveToTree();
                break;
            case State.Attacking:
                Attack();
                break;
            case State.Idle:
                FindTree();
                break;
        }
    }

    private void FindTree()
    {
        if (treeTarget == null)
            treeTarget = GameObject.FindWithTag("Tree")?.transform;

        if (treeTarget != null)
            currentState = State.Moving;
    }

    private void MoveToTree()
    {
        Vector3 targetPos = treeTarget.position + GetRandomOffset();
        Vector3 direction = (targetPos - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, treeTarget.position) < 2f)
        {
            currentState = State.Attacking;
        }
    }

    private void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0;
            woodAccumulator += woodPerSecond * attackInterval;

            if (woodAccumulator >= 1f)
            {
                long woodToAdd = (long)woodAccumulator;
                GameManager.Instance.AddWood(woodToAdd);
                woodAccumulator -= woodToAdd;
            }

            // 타격 애니메이션 트리거
        }
    }

    private Vector3 GetRandomOffset()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 1.5f;
    }
}
```

**파일:** `Assets/02.Scripts/Lumberjack/LumberjackSpawner.cs`

```csharp
// LumberjackSpawner.cs - 기본 구조
public class LumberjackSpawner : MonoBehaviour
{
    [SerializeField] private GameObject lumberjackPrefab;
    [SerializeField] private Transform spawnPoint;

    public void SpawnLumberjack()
    {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Instantiate(lumberjackPrefab, spawnPos, Quaternion.identity);
    }
}
```

### Task 2.6: 프로토타입 플레이 테스트

**목표:** 전체 시스템이 연동되어 동작하는지 확인합니다.

**작업 내용:**

- [ ] 전체 시스템 통합 테스트
- [ ] 클릭 → 목재 증가 → 업그레이드 구매 → 클릭당 획득량 증가 검증
- [ ] 벌목꾼 소환 → 자동 목재 수집 검증
- [ ] 기본 밸런싱 조정

**체크리스트:**

| 기능 | 테스트 항목 | 통과 |
|------|-------------|------|
| 클릭 | 나무 클릭 시 목재 +1 | ⬜ |
| UI | 목재 카운터 실시간 업데이트 | ⬜ |
| 피드백 | 화면/나무 흔들림 동작 | ⬜ |
| 피드백 | 플로팅 텍스트 표시 | ⬜ |
| 피드백 | 파티클 발생 | ⬜ |
| 업그레이드 | 비용 차감 및 효과 적용 | ⬜ |
| 업그레이드 | 비용 증가 (1.15배) | ⬜ |
| 벌목꾼 | 나무 주변 이동 | ⬜ |
| 벌목꾼 | 자동 목재 수집 (1/초) | ⬜ |

---

## 파일 구조 요약

```
Assets/
├── 01.Scenes/
│   └── GameScene.unity
├── 02.Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # ISaveable 구현, ServiceLocator 등록
│   │   ├── ServiceLocator.cs       # 의존성 관리
│   │   └── GameEvents.cs           # 중앙화된 이벤트 시스템
│   ├── Interfaces/
│   │   ├── ISaveable.cs            # M2 저장 시스템 대비
│   │   └── IClickable.cs           # 클릭 가능 오브젝트 인터페이스
│   ├── Tree/
│   │   └── TreeController.cs       # IClickable 구현
│   ├── Player/
│   │   └── InputHandler.cs         # 터치/마우스 통합 처리
│   ├── Economy/
│   │   ├── UpgradeData.cs          # ScriptableObject
│   │   └── UpgradeManager.cs       # ISaveable 구현
│   ├── Lumberjack/
│   │   ├── LumberjackController.cs
│   │   └── LumberjackSpawner.cs
│   ├── UI/
│   │   ├── WoodCounterUI.cs
│   │   └── UpgradeButtonUI.cs      # Inspector 참조 사용
│   ├── Effects/
│   │   ├── ScreenShake.cs
│   │   ├── TreeShake.cs
│   │   ├── FloatingText.cs         # 풀링 지원
│   │   ├── FloatingTextSpawner.cs  # 풀 관리
│   │   └── HitParticleSpawner.cs
│   └── Utils/
│       └── ObjectPool.cs           # 범용 오브젝트 풀
├── 03.Prefabs/
│   ├── Lumberjack.prefab
│   ├── FloatingText.prefab
│   └── UI/
│       └── UpgradeButton.prefab
└── Resources/
    └── Upgrades/
        └── SharpAxe.asset
```

### 스크립트 수: 17개
- Core: 3개 (GameManager, ServiceLocator, GameEvents)
- Interfaces: 2개 (ISaveable, IClickable)
- Tree: 1개
- Player: 1개
- Economy: 2개
- Lumberjack: 2개
- UI: 2개
- Effects: 5개
- Utils: 1개

---

## Acceptance Criteria

### 기능 요구사항

- [ ] 화면 중앙의 나무를 클릭하면 목재 +1 획득
- [ ] 화면 상단에 현재 목재량 표시 (K, M 포맷팅)
- [ ] 클릭 시 화면 흔들림 + 나무 흔들림 피드백
- [ ] 클릭 시 획득량 플로팅 텍스트 표시
- [ ] 클릭 시 나무 조각/잎사귀 파티클 발생
- [ ] "날카로운 도끼" 업그레이드 구매 가능 (비용: 100 목재, 효과: +1/클릭)
- [ ] "견습 벌목꾼" 소환 가능 (비용: 200 목재, 효과: +1/초)
- [ ] 벌목꾼이 나무 주변을 이동하며 자동으로 목재 수집

### 아키텍처 요구사항

- [ ] ServiceLocator에 GameManager, UpgradeManager 등록
- [ ] GameEvents를 통한 모든 이벤트 발행 (static event 사용 금지)
- [ ] TreeController가 IClickable 인터페이스 구현
- [ ] GameManager, UpgradeManager가 ISaveable 인터페이스 구현
- [ ] FloatingText에 ObjectPool 적용
- [ ] FindObjectOfType 사용 금지 (Inspector 참조 또는 ServiceLocator 사용)

### 비기능 요구사항

- [ ] 60 FPS 유지
- [ ] 모바일 터치 입력 지원
- [ ] 코드 네이밍 컨벤션 준수 (PascalCase for public, camelCase for private)
- [ ] 씬 전환 시 메모리 누수 없음 (이벤트 구독 해제 확인)

---

## 의존성 및 리스크

### 기술 의존성

| 패키지 | 용도 | 상태 |
|--------|------|------|
| Input System | 터치/클릭 입력 | ✅ 설치됨 |
| TextMeshPro | UI 텍스트 | ✅ 기본 포함 |
| DOTween (선택) | 애니메이션 보간 | 📋 필요시 설치 |

### 에셋 의존성

| 에셋 | 용도 | 상태 |
|------|------|------|
| LowpolyNatureBundle | 나무 모델 | ✅ 포함됨 |
| Supercyan Free Forest | 숲 환경 | ✅ 포함됨 |

### 리스크

1. **나무 모델 선택**
   - 기존 에셋에서 적합한 나무 찾기
   - 대응: 양쪽 에셋 모두 확인하여 최적 선택

2. **벌목꾼 애니메이션**
   - 기본 캐릭터 에셋 필요
   - 대응: M1에서는 Capsule로 대체, M3에서 아트 적용

---

## 아키텍처 결정 기록 (ADR)

### ADR-1: ServiceLocator 패턴 도입

**상태:** 채택됨

**맥락:** 프로토타입에서 의존성 관리가 필요하며, M2+ 확장 시 DI 컨테이너로 교체 가능해야 함.

**결정:** 간단한 ServiceLocator 구현. FindObjectOfType 대신 사용.

**장점:**
- 테스트 용이성 (Mock 주입 가능)
- 명시적 의존성
- M2에서 Zenject 등으로 마이그레이션 용이

### ADR-2: 인스턴스 기반 GameEvents

**상태:** 채택됨

**맥락:** static event는 씬 전환 시 메모리 누수 위험이 있음.

**결정:** GameEvents 싱글톤을 통해 모든 이벤트 중앙 관리.

**장점:**
- 씬 전환 시 자동 정리
- 이벤트 디버깅 용이
- 이벤트 발행/구독 추적 가능

### ADR-3: ISaveable 인터페이스

**상태:** 정의됨 (M2에서 구현)

**맥락:** M2에서 저장 시스템 구현 시 리팩토링 최소화 필요.

**결정:** M1에서 인터페이스 정의, GameManager/UpgradeManager에 미리 구현.

**장점:**
- M2에서 즉시 저장 시스템 연동 가능
- 저장 가능 컴포넌트 자동 탐색 가능

### ADR-4: ObjectPool 유틸리티

**상태:** 채택됨

**맥락:** FloatingText가 빈번하게 생성/파괴되어 GC 스파이크 발생 가능.

**결정:** 범용 ObjectPool<T> 구현, FloatingText에 적용.

**장점:**
- GC 압박 감소
- M2+에서 파티클, 벌목꾼 등에 재사용 가능

---

## 참고 자료

- GDD: `Docs/gdd.md`
- 개발 일정: `Docs/development_schedule.md`
- Unity Input System 문서: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest

---

*— 계획 문서 끝 —*
