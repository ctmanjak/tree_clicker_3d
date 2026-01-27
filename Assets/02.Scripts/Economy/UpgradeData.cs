using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "LumberTycoon/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("기본 정보")]
    public string UpgradeName;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("비용")]
    public long BaseCost = 100;
    public float CostMultiplier = 1.15f;

    [Header("효과")]
    public long EffectAmount = 1;

    public long GetCost(int currentLevel)
    {
        return (long)(BaseCost * Mathf.Pow(CostMultiplier, currentLevel));
    }
}
