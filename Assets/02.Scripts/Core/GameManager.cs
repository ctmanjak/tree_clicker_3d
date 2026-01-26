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
    }

    private void Start()
    {
        _gameEvents = GameEvents.Instance;
    }

    public void AddWood(long amount)
    {
        _currentWood += amount;
        _gameEvents?.RaiseWoodAdded(amount);
        _gameEvents?.RaiseWoodChanged(_currentWood);
    }

    public bool SpendWood(long amount)
    {
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
        _woodPerClick += amount;
    }

    public object CaptureState()
    {
        return new SaveData { wood = _currentWood, woodPerClick = _woodPerClick };
    }

    public void RestoreState(object state)
    {
        if (state is SaveData data)
        {
            _currentWood = data.wood;
            _woodPerClick = data.woodPerClick;
            _gameEvents?.RaiseWoodChanged(_currentWood);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public long wood;
        public long woodPerClick;
    }
}
