using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<UpgradeData> _upgrades = new();

    private Dictionary<string, int> _upgradeLevels = new();
    private GameManager _gameManager;
    private GameEvents _gameEvents;
    private LumberjackSpawner _lumberjackSpawner;

    public string SaveKey => "UpgradeManager";
    public IReadOnlyList<UpgradeData> Upgrades => _upgrades;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _gameEvents = GameEvents.Instance;
        _lumberjackSpawner = ServiceLocator.Get<LumberjackSpawner>();
    }

    public bool TryPurchase(UpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade);
        long cost = upgrade.GetCost(currentLevel);

        if (_gameManager.SpendWood(cost))
        {
            _upgradeLevels[upgrade.UpgradeName] = currentLevel + 1;
            ApplyEffect(upgrade);
            _gameEvents?.RaiseUpgradePurchased(upgrade.UpgradeName, currentLevel + 1);
            return true;
        }
        return false;
    }

    private void ApplyEffect(UpgradeData upgrade)
    {
        switch (upgrade.Type)
        {
            case UpgradeType.WoodPerClick:
                _gameManager.IncreaseWoodPerClick(upgrade.EffectAmount);
                break;
            case UpgradeType.SpawnLumberjack:
                _lumberjackSpawner?.SpawnLumberjack();
                break;
        }
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return _upgradeLevels.TryGetValue(upgrade.UpgradeName, out int level) ? level : 0;
    }

    public IEnumerable<UpgradeData> GetUpgradesByType(UpgradeType type)
    {
        return _upgrades.Where(u => u.Type == type);
    }

    public object CaptureState()
    {
        return new Dictionary<string, int>(_upgradeLevels);
    }

    public void RestoreState(object state)
    {
        if (state is Dictionary<string, int> savedLevels)
        {
            _upgradeLevels = new Dictionary<string, int>(savedLevels);
        }
    }
}
