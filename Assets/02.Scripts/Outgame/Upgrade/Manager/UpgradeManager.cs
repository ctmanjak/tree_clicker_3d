using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public event Action<string, int> OnUpgradePurchased;
    public event Action<CurrencyType, CurrencyValue> OnPerClickChanged;

    [SerializeField] private List<UpgradeData> _upgrades = new();

    private IUpgradeRepository _repository;
    private CurrencyManager _currencyManager;
    private LumberjackSpawner _lumberjackSpawner;

    private CurrencyValue _cachedWoodPerClick = CurrencyValue.One;

    public IReadOnlyList<UpgradeData> Upgrades => _upgrades;

    private void Awake()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet(out _repository);
        ServiceLocator.TryGet(out _currencyManager);
        ServiceLocator.TryGet(out _lumberjackSpawner);
    }

    private void Start()
    {
        RecalculateWoodPerClick();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public bool TryPurchase(UpgradeData upgrade)
    {
        int currentLevel = _repository.GetLevel(upgrade.UpgradeName);

        if (upgrade.IsMaxLevel(currentLevel))
            return false;

        CurrencyValue cost = upgrade.GetCost(currentLevel);

        if (!_currencyManager.Spend(CurrencyType.Wood, cost))
            return false;

        int newLevel = currentLevel + 1;
        _repository.SetLevel(upgrade.UpgradeName, newLevel);

        ApplyEffect(upgrade);
        OnUpgradePurchased?.Invoke(upgrade.UpgradeName, newLevel);
        return true;
    }

    private void ApplyEffect(UpgradeData upgrade)
    {
        switch (upgrade.Type)
        {
            case UpgradeType.WoodPerClick:
                RecalculateWoodPerClick();
                OnPerClickChanged?.Invoke(CurrencyType.Wood, _cachedWoodPerClick);
                break;
            case UpgradeType.SpawnLumberjack:
                _lumberjackSpawner?.SpawnLumberjack();
                break;
        }
    }

    public int GetLevel(UpgradeData upgrade)
    {
        return _repository.GetLevel(upgrade.UpgradeName);
    }

    public IEnumerable<UpgradeData> GetUpgradesByType(UpgradeType type)
    {
        return _upgrades.Where(u => u.Type == type);
    }

    public CurrencyValue GetWoodPerClick() => _cachedWoodPerClick;

    private void RecalculateWoodPerClick()
    {
        CurrencyValue total = CurrencyValue.One;

        foreach (var upgrade in GetUpgradesByType(UpgradeType.WoodPerClick))
        {
            int level = GetLevel(upgrade);
            total += upgrade.EffectAmount * level;
        }

        _cachedWoodPerClick = total;
    }
}
