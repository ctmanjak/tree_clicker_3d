public class UpgradeState
{
    public string UpgradeId { get; }
    public int Level { get; private set; }

    public UpgradeState(string upgradeId, int level = 0)
    {
        UpgradeId = upgradeId;
        Level = level;
    }

    public void IncrementLevel()
    {
        Level++;
    }

    public bool IsMaxLevel(int maxLevel)
    {
        return maxLevel > 0 && Level >= maxLevel;
    }
}
