# Core 모듈 API 레퍼런스

> 순수 인프라: 의존성 주입, 암호화 유틸리티

## 목차

- [ServiceLocator](#servicelocator)
- [Crypto](#crypto)

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
