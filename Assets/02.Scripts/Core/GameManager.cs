using UnityEngine;

public class GameManager : MonoBehaviour, ISaveable
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private long _currentWood = 0;
    [SerializeField] private long _woodPerClick = 1;

    private GameEvents _gameEvents;

    public long CurrentWood => _currentWood;
    public long WoodPerClick => _woodPerClick;

    public string SaveKey => "GameManager";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ServiceLocator.Register(this);
        _gameEvents = GameEvents.Instance;
    }

    public void AddWood(long amount)
    {
        if (amount <= 0) return;

        _currentWood += amount;
        _gameEvents?.RaiseWoodAdded(amount);
        _gameEvents?.RaiseWoodChanged(_currentWood);
    }

    public bool SpendWood(long amount)
    {
        if (amount <= 0) return false;

        if (_currentWood >= amount)
        {
            _currentWood -= amount;
            _gameEvents?.RaiseWoodChanged(_currentWood);
            return true;
        }
        return false;
    }

    public void IncreaseWoodPerClick(long amount)
    {
        if (amount <= 0) return;

        _woodPerClick += amount;
        _gameEvents?.RaiseWoodPerClickChanged(_woodPerClick);
    }

    public object CaptureState()
    {
        return new SaveData { Wood = _currentWood, WoodPerClick = _woodPerClick };
    }

    public void RestoreState(object state)
    {
        if (state is SaveData data)
        {
            _currentWood = data.Wood;
            _woodPerClick = data.WoodPerClick;
            _gameEvents?.RaiseWoodChanged(_currentWood);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public long Wood;
        public long WoodPerClick;
    }
}
