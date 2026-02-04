using Cysharp.Threading.Tasks;

namespace Outgame
{
    public interface IRepositoryProvider
    {
        IAccountRepository AccountRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        IUpgradeRepository UpgradeRepository { get; }
        UniTask Initialize();
    }
}
