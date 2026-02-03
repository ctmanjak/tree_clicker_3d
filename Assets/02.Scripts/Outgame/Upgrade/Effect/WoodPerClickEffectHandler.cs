using System.Collections.Generic;

public class WoodPerClickEffectHandler : IUpgradeEffectHandler
{
    private IEnumerable<Upgrade> _upgrades;
    private CurrencyValue _cachedWoodPerClick = CurrencyValue.One;

    public CurrencyValue WoodPerClick => _cachedWoodPerClick;

    public void OnInitialLoad(IEnumerable<Upgrade> upgrades)
    {
        _upgrades = upgrades;
        Recalculate();
    }

    public void OnEffectApplied(Upgrade upgrade)
    {
        Recalculate();
    }

    public string GetEffectText(Upgrade upgrade)
    {
        return $"+{upgrade.EffectAmount}/클릭";
    }

    private void Recalculate()
    {
        CurrencyValue total = CurrencyValue.One;

        foreach (var upgrade in _upgrades)
        {
            total += upgrade.EffectAmount * upgrade.Level;
        }

        _cachedWoodPerClick = total;
    }
}
