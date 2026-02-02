public interface IUpgradeRepository
{
    void Initialize();
    void Save();
    int GetLevel(string upgradeId);
    void SetLevel(string upgradeId, int level);
}
