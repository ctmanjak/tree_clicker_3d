using Cysharp.Threading.Tasks;

namespace Outgame
{
    public class LocalRepositoryProvider : IRepositoryProvider
    {
        public IAccountRepository AccountRepository { get; private set; }
        public ICurrencyRepository CurrencyRepository { get; private set; }
        public IUpgradeRepository UpgradeRepository { get; private set; }

        public UniTask Initialize()
        {
            AccountRepository = new LocalAccountRepository();
            CurrencyRepository = new LocalCurrencyRepository();
            UpgradeRepository = new LocalUpgradeRepository();
            return UniTask.CompletedTask;
        }
    }
}
