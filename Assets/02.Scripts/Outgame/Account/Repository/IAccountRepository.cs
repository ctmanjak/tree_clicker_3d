public interface IAccountRepository
{
    bool IsEmailAvailable(string email);
    AuthResult Register(string email, string password);
    AuthResult Login(string email, string password);
    void Logout();
}