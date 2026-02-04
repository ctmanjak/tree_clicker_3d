using System.Collections.Generic;
using Ingame;

namespace Outgame
{
    public class LumberjackProductionEffectHandler : IUpgradeEffectHandler
    {
        private readonly LumberjackSpawner _spawner;
        private IEnumerable<Upgrade> _upgrades;
        private CurrencyValue _cachedLumberjackProduction = CurrencyValue.One;

        public CurrencyValue LumberjackProduction => _cachedLumberjackProduction;

        public LumberjackProductionEffectHandler(LumberjackSpawner spawner)
        {
            _spawner = spawner;
        }

        public void OnInitialLoad(IEnumerable<Upgrade> upgrades)
        {
            _upgrades = upgrades;
            Recalculate();
        }

        public void OnEffectApplied(Upgrade upgrade)
        {
            Recalculate();
            _spawner?.UpdateAllLumberjackStats(_cachedLumberjackProduction);
        }

        public string GetEffectText(Upgrade upgrade)
        {
            return $"+{upgrade.EffectAmount}/벌목꾼";
        }

        private void Recalculate()
        {
            CurrencyValue total = CurrencyValue.One;

            foreach (var upgrade in _upgrades)
            {
                total += upgrade.EffectAmount * upgrade.Level;
            }

            _cachedLumberjackProduction = total;
        }
    }
}
