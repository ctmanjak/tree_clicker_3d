using Cysharp.Threading.Tasks;

public interface IUpgradeRepository
{
    UniTask Initialize();
    void Save();
    int GetLevel(string upgradeId);
    void SetLevel(string upgradeId, int level);
}
