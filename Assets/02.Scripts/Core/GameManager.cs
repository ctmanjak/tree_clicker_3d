using UnityEngine;

public class GameManager : MonoBehaviour, ISaveable
{
    [SerializeField] private long _currentWood = 0;
    [SerializeField] private long _woodPerClick = 1;

    private GameEvents _gameEvents;

    public long CurrentWood => _currentWood;
    public long WoodPerClick => _woodPerClick;

    public bool CanAfford(long amount) => _currentWood >= amount;

    public string SaveKey => "GameManager";

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _gameEvents);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public void AddWood(long amount)
    {
        if (amount <= 0) return;

        _currentWood += amount;
        GameEvents?.RaiseWoodAdded(amount);
        GameEvents?.RaiseWoodChanged(_currentWood);
    }

    public bool SpendWood(long amount)
    {
        if (amount <= 0) return false;

        if (_currentWood >= amount)
        {
            _currentWood -= amount;
            GameEvents?.RaiseWoodChanged(_currentWood);
            return true;
        }
        return false;
    }

    public void IncreaseWoodPerClick(long amount)
    {
        if (amount <= 0) return;

        _woodPerClick += amount;
        GameEvents?.RaiseWoodPerClickChanged(_woodPerClick);
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
            GameEvents?.RaiseWoodChanged(_currentWood);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public long Wood;
        public long WoodPerClick;
    }
}
