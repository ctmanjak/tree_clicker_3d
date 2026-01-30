using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeData> _upgrades = new();

    private IUpgradeRepository _repository;
    private CurrencyManager _currencyManager;
    private GameEvents _gameEvents;
    private LumberjackSpawner _lumberjackSpawner;

    public IReadOnlyList<UpgradeData> Upgrades => _upgrades;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _repository);
        ServiceLocator.TryGet(out _currencyManager);
        ServiceLocator.TryGet(out _gameEvents);
        ServiceLocator.TryGet(out _lumberjackSpawner);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public bool TryPurchase(UpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade);

        if (upgrade.IsMaxLevel(currentLevel))
            return false;

        CurrencyValue cost = upgrade.GetCost(currentLevel);

        if (_currencyManager.SpendWood(cost))
        {
            var state = _repository.GetState(upgrade.UpgradeName);
            state.IncrementLevel();
            _repository.SaveState(state);

            ApplyEffect(upgrade);
            _gameEvents?.RaiseUpgradePurchased(upgrade.UpgradeName, state.Level);
            return true;
        }
        return false;
    }

    private void ApplyEffect(UpgradeData upgrade)
    {
        switch (upgrade.Type)
        {
            case UpgradeType.WoodPerClick:
                _currencyManager.IncreaseWoodPerClick(upgrade.EffectAmount);
                break;
            case UpgradeType.SpawnLumberjack:
                _lumberjackSpawner?.SpawnLumberjack();
                break;
        }
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return _repository.GetState(upgrade.UpgradeName).Level;
    }

    public IEnumerable<UpgradeData> GetUpgradesByType(UpgradeType type)
    {
        return _upgrades.Where(u => u.Type == type);
    }
}
