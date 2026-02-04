using Core;
using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class FirebaseAccountRepository : IAccountRepository
    {
        private readonly IFirebaseAuthService _authService;

        public FirebaseAccountRepository(IFirebaseAuthService authService)
        {
            _authService = authService;
        }

        public async UniTask<AuthResult> Register(string email, string password)
        {
            FirebaseAuthResult firebaseResult = await _authService.Register(email, password);
            return ToAuthResult(firebaseResult);
        }

        public async UniTask<AuthResult> Login(string email, string password)
        {
            FirebaseAuthResult firebaseResult = await _authService.Login(email, password);
            return ToAuthResult(firebaseResult);
        }

        public void Logout()
        {
            _authService.Logout();
        }

        private AuthResult ToAuthResult(FirebaseAuthResult firebaseResult)
        {
            return new AuthResult
            {
                Success = firebaseResult.Success,
                ErrorMessage = firebaseResult.ErrorMessage,
            };
        }
    }
}
