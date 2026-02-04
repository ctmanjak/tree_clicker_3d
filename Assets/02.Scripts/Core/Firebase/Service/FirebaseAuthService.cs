using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;

namespace Core
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private FirebaseAuth _auth;

        public bool IsInitialized => _auth != null;
        public string CurrentUserId => _auth?.CurrentUser?.UserId ?? string.Empty;
        public bool IsLoggedIn => _auth?.CurrentUser != null;

        public UniTask Initialize()
        {
            _auth = FirebaseAuth.DefaultInstance;
            return UniTask.CompletedTask;
        }

        public UniTask<FirebaseAuthResult> Register(string email, string password)
        {
            return ExecuteAuthAsync(() => _auth.CreateUserWithEmailAndPasswordAsync(email, password));
        }

        public UniTask<FirebaseAuthResult> Login(string email, string password)
        {
            return ExecuteAuthAsync(() => _auth.SignInWithEmailAndPasswordAsync(email, password));
        }

        private async UniTask<FirebaseAuthResult> ExecuteAuthAsync(
            Func<System.Threading.Tasks.Task<Firebase.Auth.AuthResult>> authOperation)
        {
            try
            {
                var result = await authOperation();
                return new FirebaseAuthResult
                {
                    Success = true,
                    UserId = result.User.UserId,
                    Email = result.User.Email,
                };
            }
            catch (AggregateException ex)
            {
                return FailureResult(ParseFirebaseError(ex));
            }
            catch (FirebaseException ex)
            {
                return FailureResult(ParseFirebaseError(ex));
            }
            catch (Exception ex)
            {
                return FailureResult(ex.Message);
            }
        }

        private static FirebaseAuthResult FailureResult(string errorMessage)
        {
            return new FirebaseAuthResult
            {
                Success = false,
                ErrorMessage = errorMessage,
            };
        }

        public void Logout()
        {
            _auth.SignOut();
        }

        private string ParseFirebaseError(AggregateException ex)
        {
            foreach (var inner in ex.Flatten().InnerExceptions)
            {
                if (inner is FirebaseException firebaseEx)
                {
                    return ParseFirebaseError(firebaseEx);
                }
            }
            return ex.Message;
        }

        private string ParseFirebaseError(FirebaseException ex)
        {
            var errorCode = (AuthError)ex.ErrorCode;
            return errorCode switch
            {
                AuthError.EmailAlreadyInUse => "이미 사용 중인 이메일입니다.",
                AuthError.InvalidEmail => "올바르지 않은 이메일 형식입니다.",
                AuthError.WeakPassword => "비밀번호가 너무 약합니다.",
                AuthError.WrongPassword => "아이디와 비밀번호를 확인해주세요.",
                AuthError.UserNotFound => "아이디와 비밀번호를 확인해주세요.",
                AuthError.TooManyRequests => "요청이 너무 많습니다. 잠시 후 다시 시도해주세요.",
                AuthError.NetworkRequestFailed => "네트워크 연결을 확인해주세요.",
                _ => $"인증 오류가 발생했습니다. ({errorCode})",
            };
        }
    }
}
