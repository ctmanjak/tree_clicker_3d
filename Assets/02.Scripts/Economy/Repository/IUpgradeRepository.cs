using System.Collections.Generic;

public interface IUpgradeRepository
{
    UpgradeState GetState(string upgradeId);
    void SaveState(UpgradeState state);
    IEnumerable<UpgradeState> GetAllStates();
}
