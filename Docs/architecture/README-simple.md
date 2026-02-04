# 벌목왕 - 아키텍처 다이어그램 (요약본)

> `README.md`의 다이어그램을 핵심 위주로 정리한 버전입니다.
> 클래스명 옆 괄호는 해당 클래스의 역할 설명입니다.

---

## 1. 시스템 레이어 구조

게임 코드는 3개 레이어로 나뉘고, 각 레이어는 명확한 역할을 갖습니다.

```
┌──────────────────────┐     ┌──────────────────────────┐
│       Ingame         │ ←─→ │        Outgame           │
│                      │     │                          │
│  플레이어가 직접 보고  │     │  화면 뒤에서 돌아가는     │
│  상호작용하는 영역     │     │  비즈니스 로직 영역       │
│  (나무, 벌목꾼, UI,   │     │  (계정, 재화, 업그레이드)  │
│   사운드, 이펙트)     │     │                          │
└──────────┬───────────┘     └────────────┬─────────────┘
           │                              │
           └──────────┬───────────────────┘
                      ▼
         ┌─────────────────────────┐
         │          Core           │
         │                         │
         │  모두가 공유하는 인프라    │
         │  (ServiceLocator, 암호화, │
         │   Firebase, 공통 인터페이스)│
         └─────────────────────────┘
```

**의존성 규칙**:
- Ingame ↔ Outgame: 양방향 가능 (서로 협력)
- Ingame/Outgame → Core: **단방향만 허용** (Core를 가져다 쓸 수만 있음)

---

## 2. 주요 컴포넌트 의존성

```mermaid
graph TB
    subgraph Ingame
        subgraph "UI (화면 표시/입력)"
            UpgradeButtonUI["UpgradeButtonUI\n(업그레이드 버튼)"]
            UpgradePanelUI["UpgradePanelUI\n(업그레이드 패널)"]
            WoodCounterUI["WoodCounterUI\n(재화 표시)"]
            LoginPanel["LoginPanel\n(로그인 화면)"]
            FloatingTextSpawner["FloatingTextSpawner\n(+N 텍스트)"]
        end

        subgraph "게임 오브젝트 (월드)"
            TreeController["TreeController\n(나무)"]
            LumberjackController["LumberjackController\n(벌목꾼 AI)"]
            LumberjackSpawner["LumberjackSpawner\n(벌목꾼 생성)"]
            InputHandler["InputHandler\n(터치/클릭 입력)"]
        end

        subgraph "연출 (이펙트/사운드)"
            AudioManager["AudioManager\n(사운드)"]
            Effects["TreeShake, ScreenShake\nParticle 등"]
        end
    end

    subgraph "Outgame (비즈니스 로직)"
        CurrencyManager["CurrencyManager\n(재화 관리)"]
        UpgradeManager["UpgradeManager\n(업그레이드 관리)"]
        AccountManager["AccountManager\n(계정 관리)"]
        EffectHandlers["IUpgradeEffectHandler\n(업그레이드 효과 처리)"]
    end

    subgraph "Core (인프라)"
        ServiceLocator["ServiceLocator\n(서비스 레지스트리)"]
        FirebaseServices["Firebase 서비스\n(Auth, Store, Initializer)"]
    end

    %% 입력 흐름
    InputHandler -->|"클릭 감지"| TreeController

    %% 게임 오브젝트 → Outgame
    TreeController -->|"재화 추가"| CurrencyManager
    LumberjackController -->|"재화 추가"| CurrencyManager

    %% UI → Outgame
    UpgradeButtonUI -->|"구매 요청"| UpgradeManager
    UpgradeButtonUI -->|"잔액 확인"| CurrencyManager
    WoodCounterUI -->|"이벤트 구독"| CurrencyManager
    FloatingTextSpawner -->|"이벤트 구독"| CurrencyManager
    LoginPanel -->|"로그인 요청"| AccountManager

    %% 게임 오브젝트 → 연출
    TreeController -->|"OnTreeHit"| Effects

    %% Outgame → Ingame (역방향)
    UpgradeManager -->|"효과 위임"| EffectHandlers
    EffectHandlers -->|"벌목꾼 스폰"| LumberjackSpawner

    %% Outgame → Core
    CurrencyManager --> ServiceLocator
    UpgradeManager --> ServiceLocator
    AccountManager --> ServiceLocator
    AccountManager -->|"Firebase 인증"| FirebaseServices
```

**화살표 = 의존 방향**: A → B는 "A가 B를 사용한다"는 의미.
실선(─) = 직접 참조, 점선(- -) = 인터페이스를 통한 간접 참조.

---

## 3. Repository 패턴 — 저장소 추상화

데이터 저장 방식(Local / Firebase)을 인터페이스로 추상화해서, 어떤 저장소든 같은 방식으로 사용할 수 있게 합니다.

```mermaid
graph TB
    subgraph "인터페이스 (계약)"
        IAccountRepo["IAccountRepository"]
        ICurrencyRepo["ICurrencyRepository"]
        IUpgradeRepo["IUpgradeRepository"]
    end

    subgraph "Local 구현 (오프라인)"
        LocalAccount["LocalAccountRepository\n(PlayerPrefs)"]
        LocalCurrency["LocalCurrencyRepository\n(File I/O)"]
        LocalUpgrade["LocalUpgradeRepository\n(File I/O)"]
    end

    subgraph "Firebase 구현 (온라인)"
        FBAccount["FirebaseAccountRepository\n(Auth API)"]
        FBCurrency["FirebaseCurrencyRepository\n(Firestore)"]
        FBUpgrade["FirebaseUpgradeRepository\n(Firestore)"]
    end

    LocalAccount -.->|"구현"| IAccountRepo
    LocalCurrency -.->|"구현"| ICurrencyRepo
    LocalUpgrade -.->|"구현"| IUpgradeRepo

    FBAccount -.->|"구현"| IAccountRepo
    FBCurrency -.->|"구현"| ICurrencyRepo
    FBUpgrade -.->|"구현"| IUpgradeRepo

    subgraph "Core (인프라)"
        CoreFirebase["Firebase 서비스\n(IFirebaseAuthService,\nIFirebaseStoreService)"]
    end

    FBAccount -->|"사용"| CoreFirebase
    FBCurrency -->|"사용"| CoreFirebase
    FBUpgrade -->|"사용"| CoreFirebase
```

Manager들은 인터페이스(I~Repository)만 알고, 실제 구현이 Local인지 Firebase인지 모릅니다.
덕분에 저장소 교체 시 Manager 코드를 건드릴 필요가 없습니다.

---

## 4. 나무 클릭 → 재화 획득 흐름

```mermaid
sequenceDiagram
    participant Player as 플레이어
    participant Input as InputHandler
    participant Tree as TreeController
    participant UM as UpgradeManager
    participant CM as CurrencyManager
    participant Repo as Repository
    participant UI as WoodCounterUI
    participant FT as FloatingTextSpawner
    participant FX as Effects

    Player->>Input: 화면 터치
    Input->>Input: 터치 지점에 나무가 있는지 확인
    Input->>Tree: 나무 클릭 전달
    Tree->>UM: 클릭당 획득량 조회
    UM-->>Tree: 획득량 반환

    Tree->>CM: 목재 추가 요청
    CM->>Repo: 변경된 재화 저장

    par 이벤트 병렬 처리
        CM-->>UI: 재화 변경 알림 → 숫자 갱신
    and
        CM-->>FT: 재화 추가 알림 → +N 텍스트 표시
    and
        Tree-->>FX: 나무 피격 알림 → 흔들림/파티클
    end
```

핵심 포인트:
- **이벤트 기반 통신**: CurrencyManager가 직접 UI를 호출하지 않고, 이벤트를 발행(점선 화살표)하면 구독자들이 각자 반응
- **par 블록**: 이벤트 구독자들이 동시에 독립적으로 반응하는 구간

---

## 5. 업그레이드 구매 흐름

```mermaid
sequenceDiagram
    participant Player as 플레이어
    participant BtnUI as UpgradeButtonUI
    participant UM as UpgradeManager
    participant CM as CurrencyManager
    participant Upgrade as Upgrade
    participant Handler as IUpgradeEffectHandler
    participant Repo as Repository

    Player->>BtnUI: 업그레이드 버튼 클릭
    BtnUI->>UM: 구매 시도 요청
    UM->>CM: 비용을 지불할 수 있는지 확인
    CM-->>UM: 가능 / 불가능

    alt 구매 가능
        UM->>CM: 비용 차감
        UM->>Upgrade: 레벨 1 증가
        Upgrade-->>BtnUI: 레벨 변경 알림
        UM->>Upgrade: 효과 적용
        Upgrade->>Handler: 업그레이드 종류에 맞는 효과 실행
        UM->>Repo: 변경된 업그레이드 저장
        UM-->>BtnUI: 구매 성공 알림
        BtnUI->>BtnUI: 성공 애니메이션
    else 구매 불가
        UM-->>BtnUI: 실패 반환
        BtnUI->>BtnUI: 거부 애니메이션
    end
```

**Strategy 패턴**: 업그레이드 종류마다 다른 효과를 IUpgradeEffectHandler 구현체로 분리

| 핸들러 | 하는 일 |
|---|---|
| WoodPerClickEffectHandler | 클릭당 목재 보너스 증가 |
| LumberjackProductionEffectHandler | 벌목꾼 1명당 생산량 증가 |
| SpawnLumberjackEffectHandler | 새 벌목꾼 스폰 |

---

## 6. 벌목꾼 AI 상태 머신

```mermaid
stateDiagram-v2
    [*] --> Idle: 스폰 완료

    Idle --> Moving: FindAndAttackTree()
    Moving --> Attacking: 나무 도달
    Attacking --> Idle: 공격 세트 완료

    state Attacking {
        [*] --> SwingAxe
        SwingAxe --> HitTree: 애니메이션 이벤트
        HitTree --> AddCurrency: CurrencyManager.Add()
        AddCurrency --> Cooldown
        Cooldown --> SwingAxe: 쿨다운 종료
        Cooldown --> [*]: 공격 횟수 소진
    }
```

벌목꾼은 **Idle → Moving → Attacking** 3개 상태를 순환합니다.
Attacking 내부에서는 "휘두르기 → 적중 → 재화 추가 → 쿨다운"을 반복합니다.

---

## 7. 앱 부팅 시퀀스

```mermaid
graph TD
    A["GameBootstrap.Awake()\nSingleton 초기화"] --> B["RepositoryFactory.CreateProvider()"]
    B --> C{"Firebase SDK\n사용 가능?"}

    C -->|"가능"| D["Core::FirebaseInitializer.Initialize()"]
    D --> E{"초기화 성공?"}
    E -->|"성공"| F["FirebaseRepositoryProvider 생성"]
    E -->|"실패"| G["LocalRepositoryProvider 생성"]
    C -->|"불가"| G

    F --> H["ServiceLocator에 Repository 등록\n(IAccountRepo, ICurrencyRepo, IUpgradeRepo)"]
    G --> H

    H --> I["각 Manager의 Start()에서\nServiceLocator.Get()으로 Repository 획득"]
    I --> J["초기 데이터 로드 및 게임 시작"]
```

부팅 순서 요약:
1. `GameBootstrap.Awake()` — 앱 진입점, Singleton 보장
2. `RepositoryFactory` — Firebase 가능 여부에 따라 저장소 구현체 결정
3. `ServiceLocator` — 결정된 Repository를 전역 레지스트리에 등록
4. 각 Manager — `Start()`에서 ServiceLocator를 통해 Repository를 가져와 초기화
