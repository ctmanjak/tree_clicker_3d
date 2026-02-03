using System.Collections.Generic;

public interface IUpgradeEffectHandler
{
    void OnInitialLoad(IEnumerable<Upgrade> upgrades);
    void OnEffectApplied(Upgrade upgrade);
    string GetEffectText(Upgrade upgrade);
}
