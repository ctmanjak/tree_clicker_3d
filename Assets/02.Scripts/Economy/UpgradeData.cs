using UnityEngine;

public enum UpgradeType
{
    WoodPerClick,
    SpawnLumberjack
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "LumberTycoon/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("기본 정보")]
    public string UpgradeName;
    [TextArea] public string Description;
    public Sprite Icon;
    public UpgradeType Type;

    [Header("비용")]
    public long BaseCost = 100;
    public float CostMultiplier = 1.15f;

    [Header("효과")]
    public long EffectAmount = 1;

    [Header("레벨 제한")]
    public int MaxLevel = 0;

    public bool IsMaxLevel(int currentLevel)
    {
        return MaxLevel > 0 && currentLevel >= MaxLevel;
    }

    public long GetCost(int currentLevel)
    {
        return (long)(BaseCost * Mathf.Pow(CostMultiplier, currentLevel));
    }
}
