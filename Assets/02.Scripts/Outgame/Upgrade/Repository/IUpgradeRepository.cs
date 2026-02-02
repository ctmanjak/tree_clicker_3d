public interface IUpgradeRepository
{
    int GetLevel(string upgradeId);
    void SetLevel(string upgradeId, int level);
}
