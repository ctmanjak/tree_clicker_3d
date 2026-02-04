using System.Collections.Generic;

namespace Outgame
{
    public interface IUpgradeEffectHandler
    {
        void OnInitialLoad(IEnumerable<Upgrade> upgrades);
        void OnEffectApplied(Upgrade upgrade);
        string GetEffectText(Upgrade upgrade);
    }
}
