using Core;
using Cysharp.Threading.Tasks;
using Outgame;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }
    public UniTask Initialization => _initializationSource.Task;

    private readonly UniTaskCompletionSource _initializationSource = new();
    private RepositoryFactory _repositoryFactory;
    private IFlushable _flushableProvider;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAsync().Forget();
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
            _flushableProvider?.ForceFlushAll();
        }
    }

    private void OnApplicationQuit()
    {
        _flushableProvider?.ForceFlushAll();
    }

    private async UniTaskVoid InitializeAsync()
    {
        _repositoryFactory = new RepositoryFactory();

        var providers = await CreateProviders();
        await _repositoryFactory.Initialize(providers);

        ServiceLocator.Register(_repositoryFactory.AccountRepository);
        ServiceLocator.Register(_repositoryFactory.CurrencyRepository);
        ServiceLocator.Register(_repositoryFactory.UpgradeRepository);

        _initializationSource.TrySetResult();
        Debug.Log("Repository 초기화 완료");
    }

    private async UniTask<IRepositoryProvider[]> CreateProviders()
    {
        var providers = new System.Collections.Generic.List<IRepositoryProvider>();

        try
        {
            var initializer = new FirebaseInitializer();
            await initializer.Initialize();

            try
            {
                var syncProvider = new SyncCoordinatorProvider(
                    initializer.AuthService,
                    initializer.StoreService
                );
                await syncProvider.Initialize();
                _flushableProvider = syncProvider;
                providers.Add(syncProvider);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"SyncCoordinator 초기화 실패, HybridRepository로 전환: {ex.Message}");

                var hybridProvider = new HybridRepositoryProvider(
                    initializer.AuthService,
                    initializer.StoreService
                );
                await hybridProvider.Initialize();
                _flushableProvider = hybridProvider;
                providers.Add(hybridProvider);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Firebase 초기화 실패, Local 저장소로 전환: {ex.Message}");
        }

        providers.Add(new LocalRepositoryProvider());
        return providers.ToArray();
    }

    private void CleanupRepositories()
    {
        _flushableProvider?.Dispose();
        _flushableProvider = null;

        if (_repositoryFactory == null) return;
        ServiceLocator.Unregister(_repositoryFactory.AccountRepository);
        ServiceLocator.Unregister(_repositoryFactory.CurrencyRepository);
        ServiceLocator.Unregister(_repositoryFactory.UpgradeRepository);
    }
}
