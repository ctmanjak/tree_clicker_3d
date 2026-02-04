using Cysharp.Threading.Tasks;

namespace Outgame
{
    public interface IFirebaseAuthService
    {
        bool IsInitialized { get; }
        string CurrentUserId { get; }
        bool IsLoggedIn { get; }
        UniTask Initialize();
        UniTask<FirebaseAuthResult> Register(string email, string password);
        UniTask<FirebaseAuthResult> Login(string email, string password);
        void Logout();
    }
}
