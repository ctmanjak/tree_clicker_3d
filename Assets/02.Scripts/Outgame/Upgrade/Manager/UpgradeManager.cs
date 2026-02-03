using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class UpgradeManager : MonoBehaviour
{
    public event Action<Upgrade> OnUpgradePurchased;
    public event Action<CurrencyType, CurrencyValue> OnPerClickChanged;

    [SerializeField] private List<UpgradeSpecData> _upgradeSpecs = new();

    private readonly Dictionary<string, Upgrade> _upgrades = new();
    private readonly Dictionary<UpgradeType, List<Upgrade>> _upgradesByType = new();
    private IUpgradeRepository _repository;
    private CurrencyManager _currencyManager;
    private LumberjackSpawner _lumberjackSpawner;

    private CurrencyValue _cachedWoodPerClick = CurrencyValue.One;
    private CurrencyValue _cachedLumberjackProduction = CurrencyValue.One;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private async UniTaskVoid Start()
    {
        await GameBootstrap.Instance.Initialization;

        if (!ServiceLocator.TryGet(out _repository))
            throw new InvalidOperationException($"{nameof(IUpgradeRepository)} is not registered in ServiceLocator");

        if (!ServiceLocator.TryGet(out _currencyManager))
            throw new InvalidOperationException($"{nameof(CurrencyManager)} is not registered in ServiceLocator");

        ServiceLocator.TryGet(out _lumberjackSpawner);

        InitializeUpgrades();
        RecalculateWoodPerClick();
        RecalculateLumberjackProduction();
        SpawnSavedLumberjacks();
    }

    private void InitializeUpgrades()
    {
        foreach (var spec in _upgradeSpecs)
        {
            int level = _repository.GetLevel(spec.UpgradeName);
            var upgrade = new Upgrade(spec, level);
            _upgrades[spec.UpgradeName] = upgrade;

            if (!_upgradesByType.TryGetValue(spec.Type, out var list))
            {
                list = new List<Upgrade>();
                _upgradesByType[spec.Type] = list;
            }
            list.Add(upgrade);
        }
    }

    private void SpawnSavedLumberjacks()
    {
        int totalLevel = 0;
        foreach (var upgrade in GetUpgradesByType(UpgradeType.SpawnLumberjack))
        {
            totalLevel += upgrade.Level;
        }

        for (int i = 0; i < totalLevel; i++)
        {
            _lumberjackSpawner?.SpawnLumberjack(_cachedLumberjackProduction);
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public Upgrade GetUpgrade(string upgradeName)
    {
        return _upgrades.TryGetValue(upgradeName, out var upgrade) ? upgrade : null;
    }

    public IEnumerable<Upgrade> GetUpgradesByType(UpgradeType type)
    {
        return _upgradesByType.TryGetValue(type, out var list) ? list : Array.Empty<Upgrade>();
    }

    public IEnumerable<Upgrade> GetAllUpgrades()
    {
        return _upgrades.Values;
    }

    public bool CanPurchase(Upgrade upgrade)
    {
        if (upgrade == null) return false;
        if (upgrade.IsMaxLevel) return false;

        return _currencyManager.CanAfford(CurrencyType.Wood, upgrade.CurrentCost);
    }

    public bool TryPurchase(Upgrade upgrade)
    {
        if (upgrade == null) return false;
        if (upgrade.IsMaxLevel) return false;

        if (!_currencyManager.Spend(CurrencyType.Wood, upgrade.CurrentCost))
            return false;

        upgrade.IncrementLevel();
        _repository.SetLevel(upgrade.Name, upgrade.Level);

        ApplyEffect(upgrade);
        OnUpgradePurchased?.Invoke(upgrade);
        return true;
    }

    private void ApplyEffect(Upgrade upgrade)
    {
        switch (upgrade.Type)
        {
            case UpgradeType.WoodPerClick:
                RecalculateWoodPerClick();
                OnPerClickChanged?.Invoke(CurrencyType.Wood, _cachedWoodPerClick);
                break;
            case UpgradeType.SpawnLumberjack:
                _lumberjackSpawner?.SpawnLumberjack(_cachedLumberjackProduction);
                break;
            case UpgradeType.LumberjackProduction:
                RecalculateLumberjackProduction();
                _lumberjackSpawner?.UpdateAllLumberjackStats(_cachedLumberjackProduction);
                break;
        }
    }

    public CurrencyValue GetWoodPerClick() => _cachedWoodPerClick;
    public CurrencyValue GetLumberjackProduction() => _cachedLumberjackProduction;

    private void RecalculateWoodPerClick()
    {
        _cachedWoodPerClick = CalculateTotalEffect(UpgradeType.WoodPerClick);
    }

    private void RecalculateLumberjackProduction()
    {
        _cachedLumberjackProduction = CalculateTotalEffect(UpgradeType.LumberjackProduction);
    }

    private CurrencyValue CalculateTotalEffect(UpgradeType type)
    {
        CurrencyValue total = CurrencyValue.One;

        foreach (var upgrade in GetUpgradesByType(type))
        {
            total += upgrade.EffectAmount * upgrade.Level;
        }

        return total;
    }
}
