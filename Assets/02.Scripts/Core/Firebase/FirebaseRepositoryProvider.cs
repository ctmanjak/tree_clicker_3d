using Cysharp.Threading.Tasks;

public class FirebaseRepositoryProvider : IRepositoryProvider
{
    public IAccountRepository AccountRepository { get; private set; }
    public ICurrencyRepository CurrencyRepository { get; private set; }
    public IUpgradeRepository UpgradeRepository { get; private set; }

    public async UniTask Initialize()
    {
        var initializer = new FirebaseInitializer();
        await initializer.Initialize();

        AccountRepository = new FirebaseAccountRepository(initializer.AuthService);
        CurrencyRepository = new FirebaseCurrencyRepository(initializer.StoreService);
        UpgradeRepository = new FirebaseUpgradeRepository(initializer.StoreService);
    }
}
