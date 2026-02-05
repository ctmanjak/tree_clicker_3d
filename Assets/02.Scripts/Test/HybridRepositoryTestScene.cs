using System;
using Core;
using Cysharp.Threading.Tasks;
using Outgame;
using UnityEngine;

public class HybridRepositoryTestScene : MonoBehaviour
{
    private IFirebaseAuthService _authService;
    private IFirebaseStoreService _storeService;
    private HybridRepositoryProvider _provider;
    private ICurrencyRepository _currencyRepository;

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

            await InitializeHybridRepository();
        }
        catch (Exception ex)
        {
            Log($"Firebase 초기화 실패: {ex.Message}");
        }
    }

    private async UniTask InitializeHybridRepository()
    {
        Log("HybridRepositoryProvider 초기화 시작...");

        _provider = new HybridRepositoryProvider(_authService, _storeService);
        await _provider.Initialize();

        _currencyRepository = _provider.CurrencyRepository;

        Log("HybridRepositoryProvider 초기화 완료");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        GUILayout.Label("=== HybridRepository 테스트 ===", GUILayout.Height(30));

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("시간 오프셋 테스트", GUILayout.Height(50)))
        {
            TestTimeOffset().Forget();
        }

        if (GUILayout.Button("Save() 1회", GUILayout.Height(50)))
        {
            TestSingleSave();
        }

        if (GUILayout.Button("Save() 10회 연속", GUILayout.Height(50)))
        {
            TestMultipleSaves();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("ForceFlush()", GUILayout.Height(50)))
        {
            TestForceFlush();
        }

        if (GUILayout.Button("데이터 로드", GUILayout.Height(50)))
        {
            TestLoad().Forget();
        }

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

    private async UniTaskVoid TestTimeOffset()
    {
        if (_storeService == null)
        {
            Log("Firebase가 초기화되지 않음");
            return;
        }

        Log("서버 시간 요청 중...");

        long localTimeBefore = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long serverTime = await _storeService.GetServerTimeAsync();
        long localTimeAfter = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long offset = serverTime - localTimeBefore;

        Log($"로컬 시간 (요청 전): {localTimeBefore}");
        Log($"서버 시간: {serverTime}");
        Log($"로컬 시간 (요청 후): {localTimeAfter}");
        Log($"오프셋: {offset}초 ({offset / 60f:F1}분)");
    }

    private void TestSingleSave()
    {
        if (_currencyRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        _saveCounter++;
        var data = new CurrencySaveData
        {
            Id = "test_gold",
            Type = "gold",
            Amount = _saveCounter * 100
        };

        Log($"Save() 호출 #{_saveCounter}: gold={data.Amount}");
        _currencyRepository.Save(data);
        Log("Save() 완료 (1초 후 로컬 저장 예정)");
    }

    private void TestMultipleSaves()
    {
        if (_currencyRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        Log("Save() 10회 연속 호출 시작...");

        for (int i = 0; i < 10; i++)
        {
            _saveCounter++;
            var data = new CurrencySaveData
            {
                Id = "test_gold",
                Type = "gold",
                Amount = _saveCounter * 100
            };
            _currencyRepository.Save(data);
            Log($"  Save() #{_saveCounter}: gold={data.Amount}");
        }

        Log("10회 호출 완료 (디바운스로 1초 후 1회만 로컬 저장됨)");
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
        Log("ForceFlushAll() 완료");
    }

    private async UniTaskVoid TestLoad()
    {
        if (_currencyRepository == null)
        {
            Log("Repository가 초기화되지 않음");
            return;
        }

        Log("데이터 로드 중...");

        var items = await _currencyRepository.Initialize();

        Log($"로드된 항목 수: {items.Count}");
        foreach (var item in items)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(item.LastModified).LocalDateTime;
            Log($"  - {item.Id}: {item.Amount} (수정: {time:HH:mm:ss})");
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
        Debug.Log($"[HybridTest] {message}");

        _scrollPosition = new Vector2(0, float.MaxValue);
    }
}
