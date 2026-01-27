using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "LumberTycoon/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("기본 정보")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("비용")]
    public long baseCost = 100;
    public float costMultiplier = 1.15f;

    [Header("효과")]
    public long effectAmount = 1;

    public long GetCost(int currentLevel)
    {
        return (long)(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }
}
