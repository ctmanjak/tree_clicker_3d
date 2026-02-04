# Core 모듈 API 레퍼런스

> 인프라 레이어: 의존성 주입, 암호화 유틸리티, 공통 인터페이스, Firebase 서비스

## 목차

- [Common 인터페이스](#common-인터페이스)
- [ServiceLocator](#servicelocator)
- [Crypto](#crypto)
- [Firebase 서비스](#firebase-서비스)

---

## Common 인터페이스

### IIdentifiable

**파일**: `Core/Common/IIdentifiable.cs`
**역할**: 고유 식별자를 가진 엔티티의 마커 인터페이스

```csharp
public interface IIdentifiable
{
    string Id { get; }
}
```

### IRepository\<T\>

**파일**: `Core/Common/IRepository.cs`
**역할**: 데이터 영속성 추상화를 위한 제네릭 인터페이스

```csharp
public interface IRepository<T>
{
    UniTask<List<T>> Initialize();
    void Save(T item);
}
```

---

## ServiceLocator

**파일**: `Core/Common/ServiceLocator.cs`
**역할**: 정적 서비스 레지스트리. 인터페이스 타입을 키로 사용하여 서비스 인스턴스를 등록/조회합니다.

### API

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Register<T>` | `static void Register<T>(object instance)` | 서비스 인스턴스 등록 |
| `Unregister<T>` | `static void Unregister<T>()` | 등록된 서비스 제거 |
| `Get<T>` | `static T Get<T>()` | 서비스 조회 (미등록 시 예외) |
| `TryGet<T>` | `static bool TryGet<T>(out T instance)` | 안전한 서비스 조회 |

### 사용 예시

```csharp
// 등록 (GameBootstrap에서)
ServiceLocator.Register<ICurrencyRepository>(currencyRepo);

// 조회 (Manager에서)
var repo = ServiceLocator.Get<ICurrencyRepository>();

// 안전한 조회
if (ServiceLocator.TryGet<CurrencyManager>(out var currencyManager))
{
    // 사용 가능
}
```

### 등록되는 서비스 목록

| 타입 | 등록 시점 | 등록 주체 |
|---|---|---|
| `IAccountRepository` | 부팅 시 | `GameBootstrap` |
| `ICurrencyRepository` | 부팅 시 | `GameBootstrap` |
| `IUpgradeRepository` | 부팅 시 | `GameBootstrap` |

---

## Crypto

**파일**: `Core/Common/Crypto.cs`
**역할**: 비밀번호 해싱 및 검증을 위한 정적 유틸리티
**알고리즘**: PBKDF2 (SHA256, 10,000 iterations)

### API

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `GenerateSalt` | `static string GenerateSalt(int length)` | 랜덤 솔트 생성 |
| `HashPassword` | `static string HashPassword(string password, string salt)` | 비밀번호 해싱 |
| `VerifyPassword` | `static bool VerifyPassword(string password, string salt, string hash)` | 비밀번호 검증 |

### 상수

| 이름 | 값 | 설명 |
|---|---|---|
| `ITERATION_COUNT` | `10000` | PBKDF2 반복 횟수 |

---

## Firebase 서비스

### FirebaseInitializer

**파일**: `Core/Firebase/FirebaseInitializer.cs`
**역할**: Firebase SDK 종속성 확인 및 Auth/Store 서비스 초기화

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `AuthService` | `IFirebaseAuthService` | 인증 서비스 인스턴스 |
| `StoreService` | `IFirebaseStoreService` | 저장소 서비스 인스턴스 |

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `Initialize` | `UniTask Initialize()` | Firebase 초기화 |

### IFirebaseAuthService

**파일**: `Core/Firebase/Service/IFirebaseAuthService.cs`
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

**파일**: `Core/Firebase/Service/IFirebaseStoreService.cs`
**역할**: Firebase Firestore 데이터베이스 서비스 계약

| 메서드 | 시그니처 | 설명 |
|---|---|---|
| `SetDocument<T>` | `UniTask SetDocument<T>(string collection, T data) where T : IIdentifiable` | 문서 저장/업데이트 |
| `GetCollection<T>` | `UniTask<List<T>> GetCollection<T>(string collection) where T : IIdentifiable` | 컬렉션 전체 조회 |
| `SetDocumentAsync<T>` | `void SetDocumentAsync<T>(string collection, T data) where T : IIdentifiable` | 비동기 저장 (fire-and-forget) |

### FirebaseAuthResult

**파일**: `Core/Firebase/Domain/FirebaseAuthResult.cs`
**역할**: Firebase 인증 결과 컨테이너 (값 객체)

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `Success` | `bool` | 성공 여부 |
| `UserId` | `string` | 사용자 고유 ID |
| `Email` | `string` | 이메일 주소 |
| `ErrorMessage` | `string` | 실패 시 오류 메시지 |

### FirebaseAuthService

**파일**: `Core/Firebase/Service/FirebaseAuthService.cs`
**역할**: `IFirebaseAuthService`의 Firebase SDK 구현체
**구현**: `Firebase.Auth.FirebaseAuth` API 래핑

### FirebaseStoreService

**파일**: `Core/Firebase/Service/FirebaseStoreService.cs`
**역할**: `IFirebaseStoreService`의 Firestore SDK 구현체
**구현**: `Firebase.Firestore.FirebaseFirestore` API 래핑
