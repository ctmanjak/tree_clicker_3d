using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class UpgradeManager : MonoBehaviour
{
    public event Action<Upgrade> OnUpgradePurchased;

    [SerializeField] private List<UpgradeSpecData> _upgradeSpecs = new();

    private readonly Dictionary<string, Upgrade> _upgrades = new();
    private readonly Dictionary<UpgradeType, List<Upgrade>> _upgradesByType = new();
    private readonly Dictionary<UpgradeType, IUpgradeEffectHandler> _effectHandlers = new();
    private IUpgradeRepository _repository;
    private CurrencyManager _currencyManager;

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

        RegisterEffectHandlers();
        await InitializeUpgrades();

        foreach (var pair in _effectHandlers)
        {
            pair.Value.OnInitialLoad(GetUpgradesByType(pair.Key));
        }
    }

    private void RegisterEffectHandlers()
    {
        ServiceLocator.TryGet(out LumberjackSpawner spawner);

        var productionHandler = new LumberjackProductionEffectHandler(spawner);

        RegisterHandler(UpgradeType.WoodPerClick, new WoodPerClickEffectHandler());
        RegisterHandler(UpgradeType.LumberjackProduction, productionHandler);
        RegisterHandler(UpgradeType.SpawnLumberjack, new SpawnLumberjackEffectHandler(spawner, productionHandler));
    }

    private void RegisterHandler(UpgradeType type, IUpgradeEffectHandler handler)
    {
        _effectHandlers[type] = handler;
    }

    private async UniTask InitializeUpgrades()
    {
        var savedData = await _repository.Initialize();
        var savedLevels = savedData
            .GroupBy(s => s.Id)
            .ToDictionary(g => g.Key, g => g.Max(s => s.Level));

        foreach (var spec in _upgradeSpecs)
        {
            int level = savedLevels.GetValueOrDefault(spec.Id, 0);
            var upgrade = new Upgrade(spec, level);

            if (_effectHandlers.TryGetValue(spec.Type, out var handler))
                upgrade.SetEffectHandler(handler);

            _upgrades[spec.Id] = upgrade;

            if (!_upgradesByType.TryGetValue(spec.Type, out var list))
            {
                list = new List<Upgrade>();
                _upgradesByType[spec.Type] = list;
            }
            list.Add(upgrade);
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public Upgrade GetUpgrade(string upgradeId)
    {
        return _upgrades.GetValueOrDefault(upgradeId);
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
        _repository.Save(new UpgradeSaveData { Id = upgrade.Id, Level = upgrade.Level });

        upgrade.ApplyEffect();
        OnUpgradePurchased?.Invoke(upgrade);
        return true;
    }

    public T GetEffectHandler<T>() where T : class, IUpgradeEffectHandler
    {
        foreach (var handler in _effectHandlers.Values)
        {
            if (handler is T typed)
                return typed;
        }
        return null;
    }

    public CurrencyValue GetWoodPerClick()
        => GetEffectHandler<WoodPerClickEffectHandler>()?.WoodPerClick ?? CurrencyValue.One;

    public CurrencyValue GetLumberjackProduction()
        => GetEffectHandler<LumberjackProductionEffectHandler>()?.LumberjackProduction ?? CurrencyValue.One;
}
