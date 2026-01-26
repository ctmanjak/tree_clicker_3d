# 벌목왕 (Lumber Tycoon)

## 프로젝트 개요

- **장르**: 3D 클리커 / 방치형 (Idle/Incremental)
- **컨셉**: 나무를 베어 목재를 수집하고, 업그레이드로 벌목꾼으로 성장하는 게임
- **플랫폼**: 모바일 (iOS, Android), 웹 브라우저 (Unity 3D)

## 참고 문서

- `Docs/gdd.md` - 게임 디자인 문서 (GDD)
- `Docs/development_schedule.md` - 개발 일정 및 마일스톤
- `Docs/plans/` - 구현 계획서

## 스크립트 구조

```
Assets/02.Scripts/
├── Core/           # 핵심 시스템 (ServiceLocator, GameEvents)
├── Interfaces/     # 인터페이스 (IClickable, ISaveable)
├── Utils/          # 유틸리티 (ObjectPool)
├── Tree/           # 나무 관련 로직
├── Lumberjack/     # 벌목꾼 시스템
├── Player/         # 플레이어 입력/상호작용
├── Economy/        # 재화 및 경제 시스템
├── Effects/        # 파티클, 피드백 효과
├── UI/             # UI 컴포넌트
└── Services/       # 서비스 클래스
```

## 코딩 컨벤션

### 네이밍
- `PascalCase`: 클래스, 메서드, 프로퍼티, public 필드
- `camelCase`: 지역변수, 매개변수
- `_camelCase`: private 필드
- `UPPER_SNAKE_CASE`: 상수
- `I` 접두어: 인터페이스

### 코드 품질
- 클래스는 하나의 역할만 (단일 책임)
- 깊은 체이닝 피하기 (`a.b.c.d` 금지)
- 구체 클래스보다 인터페이스 의존 (필요 시)
- 과도한 추상화보다 작동하는 코드 우선

### 주석
- 코드로 의도가 명확하면 주석 작성 금지
- "무엇"이 아닌 "왜"를 설명할 때만 주석 사용
- 주석이 필요하면 먼저 코드 개선을 고려

### 메서드
- 한 메서드는 한 가지 일만 수행
- 메서드 길이 30줄 이내 권장
- 매개변수 3개 이하 권장 (많으면 객체로 묶기)
- bool 매개변수 지양 (별도 메서드로 분리)

### 가독성
- Early Return 활용 (중첩 깊이 최소화)
- 부정 조건문 지양 (`!isNotValid` → `isValid`)
- 중첩 깊이 3단계 이내
- 축약어 지양 (`btn` → `button`, `mgr` → `manager`)

### Unity 규칙
- `Awake`: 자기 자신 초기화
- `Start`: 다른 객체 참조, 초기 로직
- `Update`: 최소한으로, 무거운 로직 금지
- `[SerializeField]` private 필드 노출 권장 (public 필드 지양)

## 금지 사항

- `GameObject.Find()`, `FindObjectOfType()` 런타임 남용
- 문자열 경로 하드코딩 (Resources.Load 등)
- Update에서 매 프레임 GetComponent
- 매직넘버 직접 사용
- 주석으로 코드 설명하기 (이름으로 의도 표현)
- 주석 처리된 코드 커밋 (삭제 후 Git 히스토리 활용)
- 빈 catch 블록 (최소한 로깅 필수)

## 작업 원칙

1. **기획서 우선**: 구현 전 `Docs/gdd.md` 확인
2. **마일스톤 준수**: `Docs/development_schedule.md` 범위 내 작업
3. **점진적 구현**: 작동하는 최소 버전 먼저, 이후 개선
4. **확장성 고려**: 하드코딩 피하고 데이터 기반 설계

## AI 개발 원칙

### No Guessing Rule
- 코드 제안 전 기존 코드베이스 분석 필수
- 관련 클래스, 인터페이스가 없으면 먼저 확인 요청
- 기존 시스템(ServiceLocator, GameEvents 등)과의 상호작용 파악
- 프로젝트에 유사 패턴 존재 시 사용자에게 알림

### Ask Before Coding
- 모호한 요청은 질문으로 명확히 한 후 작업 시작
- 체크 항목: 오브젝트 생명주기, 퍼포먼스 민감도, 서드파티 에셋 사용 여부

### Architecture First
- 기능 구현 전 설계 단계 거침
- 적합한 디자인 패턴 2~3가지 제안 (장단점 포함)
- 사용자 선택 후 구현 시작
