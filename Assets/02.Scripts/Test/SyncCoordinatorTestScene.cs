using System;
using Core;
using Cysharp.Threading.Tasks;
using Outgame;
using UnityEngine;

public class SyncCoordinatorTestScene : MonoBehaviour
{
    private IFirebaseAuthService _authService;
    private IFirebaseStoreService _storeService;
    private SyncCoordinatorProvider _provider;
    private ICurrencyRepository _currencyRepository;
    private IUpgradeRepository _upgradeRepository;

    private string _logText = "";
    private Vector2 _scrollPosition;
    private int _saveCounter;

    private async void Start()
    {
        await InitializeFirebase();
    }

    private async UniTask InitializeFirebase()
    {
        Log("Firebase 초기화 시작...");

        try
        {
            var initializer = new FirebaseInitializer();
            await initializer.Initialize();

            _authService = initializer.AuthService;
            _storeService = initializer.StoreService;

            Log($"Firebase 초기화 완료");
            Log($"UserId: {_authService.CurrentUserId}");

            await InitializeSyncCoordinator();
        }
        catch (Exception ex)
        {
            Log($"Firebase 초기화 실패: {ex.Message}");
        }
    }

    private async UniTask InitializeSyncCoordinator()
    {
        Log("SyncCoordinatorProvider 초기화 시작...");

        _provider = new SyncCoordinatorProvider(_authService, _storeService);
        await _provider.Initialize();

        _currencyRepository = _provider.CurrencyRepository;
        _upgradeRepository = _provider.UpgradeRepository;

        Log("SyncCoordinatorProvider 초기화 완료");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        GUILayout.Label("=== SyncCoordinator 테스트 ===", GUILayout.Height(30));

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Currency Save", GUILayout.Height(50)))
        {
            TestCurrencySave();
        }

        if (GUILayout.Button("Upgrade Save", GUILayout.Height(50)))
        {
            TestUpgradeSave();
        }

        if (GUILayout.Button("Both Save", GUILayout.Height(50)))
        {
            TestBothSave();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("5회 연속 Save\n(Firebase 커밋 트리거)", GUILayout.Height(50)))
        {
            TestMultipleSavesToTriggerFirebase();
        }

        if (GUILayout.Button("ForceFlush()", GUILayout.Height(50)))
        {
            TestForceFlush();
        }

        if (GUILayout.Button("데이터 로드", GUILayout.Height(50)))
        {
            TestLoad().Forget();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("로그 초기화", GUILayout.Height(50)))
        {
            _logText = "";
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("=== 로그 ===");

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
        GUILayout.Label(_logText);
        GUILayout.EndScrollView();

        GUILayout.EndArea();
    }

    private void TestCurrencySave()
    {
        if (_currencyRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        _saveCounter++;
        var data = new CurrencySaveData
        {
            Id = "Wood",
            Type = "Wood",
            Amount = _saveCounter * 100
        };

        Log($"Currency Save() 호출 #{_saveCounter}: Wood={data.Amount}");
        _currencyRepository.Save(data);
        Log("→ pending 등록됨 (1초 후 로컬 저장 예정)");
    }

    private void TestUpgradeSave()
    {
        if (_upgradeRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        _saveCounter++;
        var data = new UpgradeSaveData
        {
            Id = "ClickPower",
            Level = _saveCounter
        };

        Log($"Upgrade Save() 호출 #{_saveCounter}: ClickPower Lv.{data.Level}");
        _upgradeRepository.Save(data);
        Log("→ pending 등록됨 (1초 후 로컬 저장 예정)");
    }

    private void TestBothSave()
    {
        if (_currencyRepository == null || _upgradeRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        _saveCounter++;

        var currencyData = new CurrencySaveData
        {
            Id = "Gold",
            Type = "Gold",
            Amount = _saveCounter * 50
        };

        var upgradeData = new UpgradeSaveData
        {
            Id = "AutoClick",
            Level = _saveCounter
        };

        Log($"Both Save() 호출 #{_saveCounter}:");
        Log($"  - Currency: Gold={currencyData.Amount}");
        Log($"  - Upgrade: AutoClick Lv.{upgradeData.Level}");

        _currencyRepository.Save(currencyData);
        _upgradeRepository.Save(upgradeData);

        Log("→ 두 컬렉션 모두 pending 등록됨 (WriteBatch로 원자적 커밋)");
    }

    private void TestMultipleSavesToTriggerFirebase()
    {
        if (_currencyRepository == null || _upgradeRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        Log("=== 5회 연속 Save 테스트 (Firebase 커밋 트리거) ===");
        Log("1초 간격으로 5번 저장 → 5번째에서 WriteBatch 커밋");

        TestMultipleSavesAsync().Forget();
    }

    private async UniTaskVoid TestMultipleSavesAsync()
    {
        for (int i = 1; i <= 5; i++)
        {
            _saveCounter++;

            var currencyData = new CurrencySaveData
            {
                Id = "Wood",
                Type = "Wood",
                Amount = _saveCounter * 100
            };

            var upgradeData = new UpgradeSaveData
            {
                Id = "ClickPower",
                Level = _saveCounter
            };

            _currencyRepository.Save(currencyData);
            _upgradeRepository.Save(upgradeData);

            Log($"[{i}/5] Save 완료: Wood={currencyData.Amount}, ClickPower Lv.{upgradeData.Level}");

            if (i < 5)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            }
        }

        Log("=== 5회 완료 → Firebase WriteBatch 커밋 로그 확인 ===");
    }

    private void TestForceFlush()
    {
        if (_provider == null)
        {
            Log("Provider가 초기화되지 않음");
            return;
        }

        Log("ForceFlushAll() 호출...");
        _provider.ForceFlushAll();
        Log("→ 즉시 로컬 저장 + Firebase WriteBatch 커밋");
    }

    private async UniTaskVoid TestLoad()
    {
        if (_currencyRepository == null || _upgradeRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        Log("=== 데이터 로드 ===");

        var currencies = await _currencyRepository.Initialize();
        var upgrades = await _upgradeRepository.Initialize();

        Log($"[Currency] 로드된 항목: {currencies.Count}개");
        foreach (var item in currencies)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(item.LastModified).LocalDateTime;
            Log($"  - {item.Id}: {item.Amount} ({time:HH:mm:ss})");
        }

        Log($"[Upgrade] 로드된 항목: {upgrades.Count}개");
        foreach (var item in upgrades)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(item.LastModified).LocalDateTime;
            Log($"  - {item.Id}: Lv.{item.Level} ({time:HH:mm:ss})");
        }
    }

    private void OnDestroy()
    {
        _provider?.Dispose();
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _logText += $"[{timestamp}] {message}\n";
        Debug.Log($"[SyncTest] {message}");

        _scrollPosition = new Vector2(0, float.MaxValue);
    }
}
