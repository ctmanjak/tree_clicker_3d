# Ingame 모듈 API 레퍼런스

> 게임플레이: 입력, 나무, 벌목꾼 AI, 오디오, 이펙트, UI, 유틸리티

## 목차

- [Common](#common)
- [Player](#player)
- [Tree](#tree)
- [Lumberjack](#lumberjack)
- [Audio](#audio)
- [Effect](#effect)
- [UI](#ui)
- [Util](#util)

---

## Common

### IClickable

**파일**: `Ingame/Common/Interface/IClickable.cs`
**역할**: 클릭/터치 가능한 오브젝트의 계약

```csharp
public interface IClickable
{
    void OnClick(Vector3 hitPoint, Vector3 hitNormal);
}
```

**구현체**: `TreeController`

---

## Player

### InputHandler

**파일**: `Ingame/Player/InputHandler.cs`
**역할**: Unity Input System 입력을 감지하고 Raycast로 클릭 대상을 식별
**패턴**: Event-driven, Adapter

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnClickPerformed` | `Action<Vector3>` | 클릭 발생 시 (클릭 위치) |
| `OnTreeClicked` | `Action<Vector3, Vector3>` | 나무 클릭 시 (hitPoint, hitNormal) |

**동작 흐름**:
1. Input System에서 클릭 콜백 수신
2. `Camera.main.ScreenPointToRay()` 로 레이 생성
3. `Physics.Raycast()` 로 충돌 감지
4. 충돌 오브젝트에 `IClickable` 인터페이스가 있으면 `OnClick()` 호출
5. 이벤트 발행

---

## Tree

### TreeController

**파일**: `Ingame/Tree/TreeController.cs`
**역할**: 나무 오브젝트 관리. 클릭 시 재화 추가 및 이펙트 트리거.
**상속**: MonoBehaviour, IClickable

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `OnClick` | `void OnClick(Vector3 hitPoint, Vector3 hitNormal)` | IClickable 구현 - 나무 타격 처리 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnTreeHit` | `Action<Vector3, Vector3>` | 나무 타격 시 (hitPoint, hitNormal) |

**타격 처리 흐름**:
1. `UpgradeManager.GetWoodPerClick()` 으로 획득량 계산
2. `CurrencyManager.Add(Wood, amount)` 로 재화 추가
3. `OnTreeHit` 이벤트 발행 → 이펙트 시스템 트리거

---

## Lumberjack

### LumberjackController

**파일**: `Ingame/Lumberjack/LumberjackController.cs`
**역할**: 벌목꾼 AI 상태 머신 (Idle → Moving → Attacking)
**패턴**: State Machine, Agent AI

**상태**:

| 상태 | 설명 | 전이 조건 |
|---|---|---|
| `Idle` | 대기/나무 탐색 | 나무 발견 시 → Moving |
| `Moving` | NavMeshAgent로 나무까지 이동 | 도착 시 → Attacking |
| `Attacking` | 나무 벌목 (쿨다운 기반) | 공격 세트 완료 시 → Idle |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetProduction` | `void SetProduction(CurrencyValue production)` | 스윙당 생산량 설정 |
| `FindAndAttackTree` | `UniTask FindAndAttackTree()` | 메인 AI 루프 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnTreeFound` | `Action<TreeController>` | 나무 대상 발견 |
| `OnReachedTree` | `Action` | 나무 도착 |
| `OnStartAttack` | `Action` | 공격 시작 |
| `OnStopAttack` | `Action` | 공격 종료 |

### LumberjackSpawner

**파일**: `Ingame/Lumberjack/LumberjackSpawner.cs`
**역할**: 벌목꾼 인스턴스 생성 및 관리
**패턴**: Factory, Object Pool

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SpawnLumberjack` | `void SpawnLumberjack(CurrencyValue production)` | 벌목꾼 생성 |
| `UpdateAllLumberjackStats` | `void UpdateAllLumberjackStats(CurrencyValue production)` | 전체 벌목꾼 생산량 갱신 |
| `ReturnAllLumberjacks` | `void ReturnAllLumberjacks()` | 모든 벌목꾼 풀 반환 |

### LumberjackAnimator

**파일**: `Ingame/Lumberjack/LumberjackAnimator.cs`
**역할**: 벌목꾼 애니메이션 상태 관리

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetBlendValue` | `void SetBlendValue(float value)` | 이동 블렌드 (0=Idle, 1=Run) |
| `SetAttackParameter` | `void SetAttackParameter(bool attacking)` | 공격 애니메이션 토글 |
| `SetSpeed` | `void SetSpeed(float speed)` | 애니메이션 속도 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnAttackHit` | `Action` | 공격 애니메이션 "히트" 이벤트 |

**Animator 파라미터**:

| 파라미터 | 타입 | 설명 |
|---|---|---|
| `Blend` | `float` | Idle/Run 블렌드 |
| `Attack` | `bool` | 공격 상태 |
| `Speed` | `float` | 재생 속도 |

### LumberjackRootMotion

**파일**: `Ingame/Lumberjack/LumberjackRootMotion.cs`
**역할**: Animator 루트 모션을 캐릭터에 적용하고 중력 추가

### LumberjackAnimationReceiver

**파일**: `Ingame/Lumberjack/LumberjackAnimationReceiver.cs`
**역할**: Animation Event를 수신하여 핸들러로 라우팅

---

## Audio

### SFXType

**파일**: `Ingame/Audio/SFXType.cs`
**역할**: 효과음 타입 열거형

| 값 | 설명 |
|---|---|
| `HitWood` | 나무 타격음 |
| `Swing` | 스윙 소리 |
| `Hit` | 타격 효과 |
| `UpgradeBuy` | 업그레이드 구매 |
| `UpgradeDenied` | 업그레이드 거부 |
| `Milestone` | 마일스톤 달성 |
| `UIOpen` | UI 열기 |
| `UIClose` | UI 닫기 |
| `Notification` | 알림 |

### SFXConfig

**파일**: `Ingame/Audio/SFXConfig.cs`
**역할**: ScriptableObject 기반 효과음 설정 (AudioClip, 볼륨, 피치 범위)

### AudioManager

**파일**: `Ingame/Audio/AudioManager.cs`
**역할**: 효과음 재생 관리 (10개 AudioSource 오브젝트 풀)
**패턴**: Singleton, Object Pool

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `MasterVolume` | `float` (get/set) | 마스터 볼륨 |
| `VolumePercent` | `float` (get/set) | 볼륨 퍼센트 (0~100) |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `PlaySFX` | `void PlaySFX(SFXType type)` | 원점에서 SFX 재생 |
| `PlaySFXAtPosition` | `void PlaySFXAtPosition(SFXType type, Vector3 position)` | 월드 위치에서 SFX 재생 |

### BGMController

**파일**: `Ingame/Audio/BGMController.cs`
**역할**: 배경음악 관리. 두 AudioSource로 크로스페이드 전환 지원.

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Play` | `void Play(AudioClip clip)` | BGM 재생 (페이드인) |
| `Stop` | `void Stop()` | BGM 정지 (페이드아웃) |
| `Pause` | `void Pause()` | 일시 정지 |
| `Resume` | `void Resume()` | 재개 |

### AmbientSoundController

**파일**: `Ingame/Audio/AmbientSoundController.cs`
**역할**: 환경음 루프 재생 (페이드 인/아웃)

---

## Effect

### SpawnEffect

**파일**: `Ingame/Effect/SpawnEffect.cs`
**역할**: 오브젝트 스폰 시 스케일 애니메이션 (0 → 1)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `PlaySpawnEffect` | `void PlaySpawnEffect()` | 스폰 애니메이션 재생 |

### TreeShake

**파일**: `Ingame/Effect/TreeShake.cs`
**역할**: 나무 타격 시 방향 기반 회전 흔들림 효과

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `ShakeFromDirection` | `void ShakeFromDirection(Vector3 direction)` | 방향 기반 흔들림 |

### ScreenShake

**파일**: `Ingame/Effect/ScreenShake.cs`
**역할**: 카메라 흔들림 효과 (충격 시)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `PlayScreenShake` | `void PlayScreenShake(float intensity, float duration)` | 카메라 흔들림 재생 |

### EnhancedParticleSpawner

**파일**: `Ingame/Effect/EnhancedParticleSpawner.cs`
**역할**: 3종 파티클 이펙트 풀 관리 (나무 조각, 나뭇잎, 크리티컬)
**패턴**: Object Pool

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SpawnEffect` | `void SpawnEffect(EffectType type, Vector3 position)` | 파티클 이펙트 스폰 |

### FirewoodSpawner

**파일**: `Ingame/Effect/FirewoodSpawner.cs`
**역할**: 나무 타격 시 장작 조각 3~5개 스폰 (물리 기반)
**패턴**: Object Pool, Factory

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SpawnFirewood` | `void SpawnFirewood(Vector3 position, Vector3 normal)` | 장작 스폰 |

### Firewood

**파일**: `Ingame/Effect/Firewood.cs`
**역할**: 개별 장작 조각. 물리 적용 후 일정 시간 뒤 풀 반환.

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Reset` | `void Reset()` | 위치/속도 초기화 (풀 재사용) |
| `ApplyForce` | `void ApplyForce(Vector3 force, Vector3 position, Vector3 torque)` | 물리력 적용 |

---

## UI

### FloatingText 시스템

#### FloatingTextStyle

**파일**: `Ingame/UI/FloatingText/FloatingTextStyle.cs`

| 값 | 설명 |
|---|---|
| `Normal` | 기본 텍스트 |
| `Critical` | 크리티컬 텍스트 (강조) |
| `Bonus` | 보너스 텍스트 |

#### FloatingTextStyleConfig

**파일**: `Ingame/UI/FloatingText/FloatingTextStyleConfig.cs`
**역할**: ScriptableObject 기반 스타일별 설정 (폰트 크기, 색상, 이동 거리, 지속 시간)

#### FloatingTextStyleProvider

**파일**: `Ingame/UI/FloatingText/FloatingTextStyleProvider.cs`
**역할**: 스타일 설정 제공자 (Singleton)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `GetStyle` | `FloatingTextStyleConfig GetStyle(FloatingTextStyle style)` | 스타일 설정 조회 |
| `GetAnimationCurve` | `AnimationCurve GetAnimationCurve(FloatingTextStyle style)` | 애니메이션 커브 |

#### FloatingText

**파일**: `Ingame/UI/FloatingText/FloatingText.cs`
**역할**: 떠오르며 사라지는 텍스트 컴포넌트 (카메라 방향 빌보딩)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetText` | `void SetText(string text, FloatingTextStyle style)` | 텍스트 설정 및 애니메이션 시작 |

#### FloatingTextSpawner

**파일**: `Ingame/UI/FloatingText/FloatingTextSpawner.cs`
**역할**: 재화 획득 이벤트에 반응하여 플로팅 텍스트 스폰
**패턴**: Object Pool, Observer

**이벤트 연결**: `CurrencyManager.OnCurrencyAdded` 구독

---

### WoodCounter UI

#### WoodCounterUI

**파일**: `Ingame/UI/WoodCounter/WoodCounterUI.cs`
**역할**: 현재 목재 보유량 실시간 표시

**이벤트 연결**: `CurrencyManager.OnCurrencyChanged` 구독

#### WoodCounterAnimator

**파일**: `Ingame/UI/WoodCounter/WoodCounterAnimator.cs`
**역할**: 재화 획득 시 DOTween 기반 펀치 스케일/색상 플래시 애니메이션

**이벤트 연결**: `CurrencyManager.OnCurrencyAdded` 구독

---

### Upgrade UI

#### UpgradeButtonUI

**파일**: `Ingame/UI/Upgrade/UpgradeButtonUI.cs`
**역할**: 개별 업그레이드 버튼 UI 컴포넌트

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Init` | `void Init(Upgrade upgrade, UpgradeManager manager)` | 업그레이드 데이터로 초기화 |

**표시 정보**: 아이콘, 이름, 비용/"MAX", 레벨/"MAX", 효과 설명

**상호작용**:
- 구매 가능 시: 펄스 애니메이션 + 구매 버튼 활성
- 구매 성공: 스케일 펀치 애니메이션 + SFX
- 구매 거부: 셰이크 애니메이션 + SFX

**이벤트 연결**: `CurrencyManager.OnCurrencyChanged`, `Upgrade.OnLevelChanged`

#### UpgradeButtonAnimator

**파일**: `Ingame/UI/Upgrade/UpgradeButtonAnimator.cs`
**역할**: DOTween 기반 업그레이드 버튼 애니메이션

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `StartPulseAnimation` | `void StartPulseAnimation()` | 연속 펄스 시작 (0.95 ↔ 1.05) |
| `StopPulseAnimation` | `void StopPulseAnimation()` | 펄스 정지 |
| `PlayPurchaseAnimation` | `void PlayPurchaseAnimation()` | 구매 성공 스케일 펀치 |
| `PlayDeniedAnimation` | `void PlayDeniedAnimation()` | 구매 거부 셰이크 |

#### UpgradePanelUI

**파일**: `Ingame/UI/Upgrade/UpgradePanelUI.cs`
**역할**: 업그레이드 상점 패널. UpgradeType별 필터링하여 버튼 동적 생성.

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Open` | `void Open(UpgradeType type)` | 타입별 패널 열기 |
| `Close` | `void Close()` | 패널 닫기 |

---

### Panel 시스템

#### UpgradePanelToggle

**파일**: `Ingame/UI/Panel/UpgradePanelToggle.cs`
**역할**: 패널 토글 및 상호 배제 (한 번에 하나의 패널만 열림)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `OnPanelToggleClicked` | `void OnPanelToggleClicked()` | 패널 토글 |
| `CloseAllPanels` | `static void CloseAllPanels()` | 모든 패널 닫기 |

#### CloseAllPanelsButton

**파일**: `Ingame/UI/Panel/CloseAllPanelsButton.cs`
**역할**: 모든 UI 패널 닫기 버튼

#### PanelTransition

**파일**: `Ingame/UI/Panel/PanelTransition.cs`
**역할**: 설정 가능한 패널 진입/퇴장 애니메이션
**전환 타입**: Slide, Scale, Fade

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `OpenPanel` | `void OpenPanel(Action onComplete)` | 애니메이션과 함께 열기 |
| `ClosePanel` | `void ClosePanel(Action onComplete)` | 애니메이션과 함께 닫기 |

| 이벤트 | 타입 | 설명 |
|---|---|---|
| `OnOpenComplete` | `Action` | 열기 완료 |
| `OnCloseComplete` | `Action` | 닫기 완료 |

**설정 가능 항목**: 전환 타입, 지속 시간, Ease 커브, 배경 오버레이 사용 여부

---

### Auth UI

#### LoginPanel

**파일**: `Ingame/UI/Auth/LoginPanel.cs`
**역할**: 로그인/회원가입 UI 패널 (모드 전환 지원)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `OnLoginButtonClicked` | `void OnLoginButtonClicked()` | 로그인 제출 |
| `OnRegisterButtonClicked` | `void OnRegisterButtonClicked()` | 회원가입 제출 |
| `OnModeToggleClicked` | `void OnModeToggleClicked()` | 로그인/회원가입 모드 전환 |

**유효성 검증**: 이메일 형식, 비밀번호 요구사항 (6~15자, 대문자, 특수문자), 비밀번호 확인 일치

---

### Toast

#### ToastMessage

**파일**: `Ingame/UI/Toast/ToastMessage.cs`
**역할**: 토스트 알림 시스템 (큐 기반, 최대 3개 동시 표시)
**패턴**: Singleton, Queue

**토스트 타입**: Info, Success, Warning, Error (각각 고유 아이콘/배경색)

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Show` | `static void Show(string message, ToastType type)` | 토스트 메시지 표시 |

**표시 동작**: 하단에서 슬라이드 인 → 설정 시간 유지 → 페이드 아웃

---

## Util

### ObjectPool\<T\>

**파일**: `Ingame/Util/ObjectPool.cs`
**역할**: 제네릭 오브젝트 풀링 시스템
**제약 조건**: `T : MonoBehaviour`

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Get` | `T Get()` | 풀에서 인스턴스 가져오기 (없으면 생성) |
| `Return` | `void Return(T instance)` | 풀에 인스턴스 반환 |
| `ReturnAll` | `void ReturnAll()` | 모든 활성 인스턴스 반환 |
| `Clear` | `void Clear()` | 풀 비우기 (오브젝트 파괴) |

**사용처**:

| 풀링 대상 | 사용 위치 | 용도 |
|---|---|---|
| `FloatingText` | `FloatingTextSpawner` | 떠오르는 텍스트 |
| `Firewood` | `FirewoodSpawner` | 장작 조각 |
| `ParticleSystem` | `EnhancedParticleSpawner` | 파티클 이펙트 |
| `AudioSource` | `AudioManager` | 효과음 재생 |

### AspectRatioManager

**파일**: `Ingame/Util/AspectRatioManager.cs`
**역할**: 9:16 세로 화면 비율 강제 적용 (필러박스 처리)

**대상 비율**: 9:16 (0.5625)
**처리 방식**: 화면 비율이 대상과 다를 경우 카메라 Viewport Rect를 조정하여 좌우 검은 바(필러박스) 생성
