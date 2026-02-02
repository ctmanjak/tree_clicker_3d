using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    private LocalAccountRepository _accountRepository;
    private LocalCurrencyRepository _currencyRepository;
    private LocalUpgradeRepository _upgradeRepository;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRepositories();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            CleanupRepositories();
            Instance = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAll();
        }
    }

    private void OnApplicationQuit()
    {
        SaveAll();
    }

    private void InitializeRepositories()
    {
        _accountRepository = new LocalAccountRepository();
        _currencyRepository = new LocalCurrencyRepository();
        _upgradeRepository = new LocalUpgradeRepository();

        ServiceLocator.Register<IAccountRepository>(_accountRepository);
        ServiceLocator.Register<ICurrencyRepository>(_currencyRepository);
        ServiceLocator.Register<IUpgradeRepository>(_upgradeRepository);
    }

    private void CleanupRepositories()
    {
        ServiceLocator.Unregister<IAccountRepository>(_accountRepository);
        ServiceLocator.Unregister<ICurrencyRepository>(_currencyRepository);
        ServiceLocator.Unregister<IUpgradeRepository>(_upgradeRepository);
    }

    private void SaveAll()
    {
        _currencyRepository?.Save();
        _upgradeRepository?.Save();
    }
}
