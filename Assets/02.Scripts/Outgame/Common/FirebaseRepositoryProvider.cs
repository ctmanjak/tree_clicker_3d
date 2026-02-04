using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class FirebaseRepositoryProvider : IRepositoryProvider
    {
        private readonly IFirebaseAuthService _authService;
        private readonly IFirebaseStoreService _storeService;

        public IAccountRepository AccountRepository { get; private set; }
        public ICurrencyRepository CurrencyRepository { get; private set; }
        public IUpgradeRepository UpgradeRepository { get; private set; }

        public FirebaseRepositoryProvider(IFirebaseAuthService authService, IFirebaseStoreService storeService)
        {
            _authService = authService;
            _storeService = storeService;
        }

        public UniTask Initialize()
        {
            AccountRepository = new FirebaseAccountRepository(_authService);
            CurrencyRepository = new FirebaseCurrencyRepository(_storeService);
            UpgradeRepository = new FirebaseUpgradeRepository(_storeService);
            return UniTask.CompletedTask;
        }
    }
}
