using Cysharp.Threading.Tasks;
using UnityEngine;

public class RepositoryFactory
{
    public IAccountRepository AccountRepository { get; private set; }
    public ICurrencyRepository CurrencyRepository { get; private set; }
    public IUpgradeRepository UpgradeRepository { get; private set; }

    public async UniTask Initialize(IRepositoryProvider[] providers)
    {
        foreach (var provider in providers)
        {
            try
            {
                await provider.Initialize();
                AccountRepository = provider.AccountRepository;
                CurrencyRepository = provider.CurrencyRepository;
                UpgradeRepository = provider.UpgradeRepository;

                await CurrencyRepository.Initialize();
                await UpgradeRepository.Initialize();
                return;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"{provider.GetType().Name} 초기화 실패: {ex.Message}");
            }
        }

        Debug.LogError("모든 RepositoryProvider 초기화 실패");
    }
}
