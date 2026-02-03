using System.Collections.Generic;

public class SpawnLumberjackEffectHandler : IUpgradeEffectHandler
{
    private readonly LumberjackSpawner _spawner;
    private readonly LumberjackProductionEffectHandler _productionHandler;

    public SpawnLumberjackEffectHandler(LumberjackSpawner spawner, LumberjackProductionEffectHandler productionHandler)
    {
        _spawner = spawner;
        _productionHandler = productionHandler;
    }

    public void OnInitialLoad(IEnumerable<Upgrade> upgrades)
    {
        CurrencyValue production = _productionHandler.LumberjackProduction;

        int totalLevel = 0;
        foreach (var upgrade in upgrades)
        {
            totalLevel += upgrade.Level;
        }

        for (int i = 0; i < totalLevel; i++)
        {
            _spawner?.SpawnLumberjack(production);
        }
    }

    public void OnEffectApplied(Upgrade upgrade)
    {
        _spawner?.SpawnLumberjack(_productionHandler.LumberjackProduction);
    }

    public string GetEffectText(Upgrade upgrade)
    {
        return "벌목꾼 +1";
    }
}
