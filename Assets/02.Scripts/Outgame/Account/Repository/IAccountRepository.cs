using Cysharp.Threading.Tasks;

namespace Outgame
{
    public interface IAccountRepository
    {
        UniTask<AuthResult> Register(string email, string password);
        UniTask<AuthResult> Login(string email, string password);
        void Logout();
    }
}
