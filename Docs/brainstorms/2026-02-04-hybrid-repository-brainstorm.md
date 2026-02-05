# HybridRepository Brainstorm

**Date**: 2026-02-04
**Status**: Complete
**Next Step**: `/workflows:plan`

## What We're Building

Firebase 호출 횟수를 줄여 비용을 절감하기 위한 HybridRepository 시스템.
로컬 저장소를 기본으로 사용하되, 조건에 따라 Firebase에 동기화하는 방식.

### 적용 범위
- **대상**: Currency, Upgrade Repository (IRepository<T> 구현체)
- **제외**: Account Repository (Auth 전용, 항상 Firebase 직접 호출)

## Why This Approach

현재 아키텍처는 Firebase 또는 Local 중 하나만 사용하는 fallback 패턴.
매 Save 호출마다 Firebase에 쓰기 때문에 호출 횟수가 과다.
HybridRepository는 두 저장소를 조합하여 Firebase 쓰기를 최적화한다.

### 선택한 방식: Generic HybridRepository<T> + HybridRepositoryProvider

- `HybridRepository<T> : IRepository<T>` 제네릭 클래스
- `HybridRepositoryProvider : IRepositoryProvider`로 기존 Provider 패턴 통합
- 기존 RepositoryFactory, ServiceLocator와 호환

**다른 방식을 선택하지 않은 이유:**
- 도메인별 개별 구현(B): 디바운스/카운트 로직 중복 발생
- Middleware 체인(C): 현재 프로젝트 규모 대비 과도한 추상화

## Key Decisions

### 1. 저장 전략

| 항목 | 결정 |
|------|------|
| 로컬 저장 | 항상 즉시 저장 |
| Firebase 저장 조건 | 디바운스 타이머 만료 **AND** 저장 횟수 >= N회 |
| 디바운스 딜레이 | 1초 (설정 가능) |
| 횟수 임계값 | 5회 (설정 가능) |

**동작 흐름:**
1. Save() 호출 → 로컬에 즉시 저장
2. 저장 카운트 증가
3. 디바운스 타이머 리셋 (1초)
4. 타이머 만료 시:
   - 카운트 >= N → Firebase에 저장, 카운트 리셋
   - 카운트 < N → Firebase 저장 건너뜀

### 2. 로드 전략 (Initialize)

| 항목 | 결정 |
|------|------|
| 최신 판단 방식 | 타임스탬프 비교 |
| 로컬 타임스탬프 | DateTime.UtcNow (서버 시간 오프셋 보정) |
| Firebase 타임스탬프 | Firestore 서버 타임스탬프 |

**동작 흐름:**
1. 로컬 데이터 로드 (즉시)
2. Firebase 데이터 로드 (1회 호출)
3. 항목별 타임스탬프 비교 → 더 최신인 쪽 사용
4. 서버 시간과 로컬 시간의 오프셋 계산 → 이후 로컬 저장 시 보정에 사용

### 3. 도메인 모델 변경

SaveData에 타임스탬프 필드 추가 필요:
- `IIdentifiable` 확장 또는 새 인터페이스 `ITimestamped` 도입
- `CurrencySaveData`, `UpgradeSaveData`에 `LastModified` 필드 추가

### 4. 보안

| 항목 | 결정 |
|------|------|
| 로컬 세이브 보안 | HMAC 서명 (기존 Crypto 클래스 활용) |
| Firebase 보안 | 추후 별도 작업 (Security Rules) |

**로컬 HMAC 동작:**
- 저장 시: 데이터를 JSON 직렬화 → HMAC 서명 생성 → 데이터 + 서명 저장
- 로드 시: 데이터 로드 → HMAC 검증 → 실패 시 데이터 무효화 (Firebase fallback)

### 5. 앱 종료 처리

| 항목 | 결정 |
|------|------|
| 전략 | OnPause/Quit 강제 flush + 미동기화 플래그 병행 |

**동작 흐름:**
- OnApplicationPause(true) / OnApplicationQuit → 디바운스 무시하고 즉시 Firebase 저장 시도
- 저장 실패 시 "미동기화" 플래그를 로컬에 기록
- 다음 Initialize 시 플래그 확인 → 있으면 로컬 데이터를 Firebase에 즉시 동기화

### 6. 오프라인 모드

| 항목 | 결정 |
|------|------|
| 전략 | 기본적인 지원 |

**동작 흐름:**
- Firebase 연결 불가 감지 시 → 로컬만 사용 (Firebase 저장 시도 건너뜀)
- 연결 복구 시 → 로컬 데이터를 Firebase에 동기화
- 별도의 큐잉 시스템 없이, 연결 복구 시점에 현재 로컬 데이터 전체를 한 번 flush

### 7. HMAC 키 관리

| 항목 | 결정 |
|------|------|
| 키 소스 | Firebase Auth UID 기반 |

- 로그인한 계정의 UID를 HMAC 키 생성에 활용
- 계정별 고유 키 → 다른 계정의 세이브 데이터 복사 방지 효과
- 기존 Crypto 클래스의 해싱 기능 재활용

### 8. 타임스탬프 정밀도

| 항목 | 결정 |
|------|------|
| 정밀도 | 초 단위 (Unix timestamp) |

- 클리커 게임 특성상 1초 미만 차이는 무의미
- long 타입으로 저장 (Unix epoch seconds)

## Resolved Questions Summary

| 질문 | 결정 |
|------|------|
| 앱 종료 시 미동기화 | OnPause/Quit flush + 플래그 fallback |
| 오프라인 모드 | 기본 지원 (로컬 전환 → 복구 시 동기화) |
| HMAC 키 관리 | Firebase Auth UID 기반 |
| 타임스탬프 정밀도 | 초 단위 (Unix timestamp) |

## Architecture Sketch

```
┌─────────────────────────────────┐
│     Manager (Currency/Upgrade)  │
│     ServiceLocator.Get<IRepo>() │
└──────────────┬──────────────────┘
               │
┌──────────────▼──────────────────┐
│    HybridRepository<T>          │
│    - Debounce Timer (1s)        │
│    - Save Count Threshold (5)   │
│    - Timestamp Comparison       │
│    - HMAC Sign/Verify           │
├─────────────┬───────────────────┤
│  Save()     │  Initialize()     │
│  1. Local즉시│  1. Local 로드    │
│  2. Count++ │  2. Firebase 로드  │
│  3. Debounce│  3. 타임스탬프비교  │
│  4. 조건충족→│  4. 최신 데이터선택│
│     Firebase│                   │
└──────┬──────┴────────┬──────────┘
       │               │
┌──────▼─────┐  ┌──────▼──────┐
│ Local Repo │  │ Firebase    │
│ (즉시저장) │  │ Repo(조건부)│
└────────────┘  └─────────────┘
```

## Config Summary

```
debounceDelay: 1초
saveCountThreshold: 5
timestampSource: ServerTimeOffset
localSecurity: HMAC
```
