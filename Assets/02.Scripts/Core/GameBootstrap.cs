using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }
    public UniTask Initialization => _initializationSource.Task;

    private readonly UniTaskCompletionSource _initializationSource = new();
    private RepositoryFactory _repositoryFactory;

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

    private async UniTaskVoid InitializeAsync()
    {
        _repositoryFactory = new RepositoryFactory();
        await _repositoryFactory.Initialize(new IRepositoryProvider[]
        {
            new FirebaseRepositoryProvider(),
            new LocalRepositoryProvider()
        });

        ServiceLocator.Register(_repositoryFactory.AccountRepository);
        ServiceLocator.Register(_repositoryFactory.CurrencyRepository);
        ServiceLocator.Register(_repositoryFactory.UpgradeRepository);

        _initializationSource.TrySetResult();
        Debug.Log("Repository 초기화 완료");
    }

    private void CleanupRepositories()
    {
        if (_repositoryFactory == null) return;
        ServiceLocator.Unregister(_repositoryFactory.AccountRepository);
        ServiceLocator.Unregister(_repositoryFactory.CurrencyRepository);
        ServiceLocator.Unregister(_repositoryFactory.UpgradeRepository);
    }
}
