using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Cysharp.Threading.Tasks;
using Ingame;
using UnityEngine;

namespace Outgame
{
    [DefaultExecutionOrder(-50)]
    public class UpgradeManager : MonoBehaviour
    {
        public event Action<Upgrade> OnUpgradePurchased;
        public event Action<CurrencyType, CurrencyValue> OnCurrencyChanged;

        [SerializeField] private List<UpgradeSpecData> _upgradeSpecs = new();

        private readonly Dictionary<string, Upgrade> _upgrades = new();
        private readonly Dictionary<UpgradeType, List<Upgrade>> _upgradesByType = new();
        private readonly Dictionary<UpgradeType, IUpgradeEffectHandler> _effectHandlers = new();
        private readonly Dictionary<Type, IUpgradeEffectHandler> _handlersByConcreteType = new();
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

            _currencyManager.OnCurrencyChanged += HandleCurrencyChanged;

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
            _handlersByConcreteType[handler.GetType()] = handler;
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
                _effectHandlers.TryGetValue(spec.Type, out var handler);
                var upgrade = new Upgrade(spec, level, handler);

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
            if (_currencyManager != null)
                _currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;

            ServiceLocator.Unregister(this);
        }

        private void HandleCurrencyChanged(CurrencyType type, CurrencyValue amount)
        {
            OnCurrencyChanged?.Invoke(type, amount);
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
            return _handlersByConcreteType.TryGetValue(typeof(T), out var handler) ? handler as T : null;
        }

        public CurrencyValue GetWoodPerClick()
            => GetEffectHandler<WoodPerClickEffectHandler>()?.WoodPerClick ?? CurrencyValue.One;

        public CurrencyValue GetLumberjackProduction()
            => GetEffectHandler<LumberjackProductionEffectHandler>()?.LumberjackProduction ?? CurrencyValue.One;
    }
}
