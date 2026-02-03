using Cysharp.Threading.Tasks;

public interface IRepositoryProvider
{
    IAccountRepository AccountRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IUpgradeRepository UpgradeRepository { get; }
    UniTask Initialize();
}
