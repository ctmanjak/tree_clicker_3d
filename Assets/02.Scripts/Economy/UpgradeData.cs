using UnityEngine;

public enum UpgradeType
{
    WoodPerClick,
    SpawnLumberjack
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "LumberTycoon/Upgrade")]
public class UpgradeData : ScriptableObject
{
    private const int UnlimitedLevel = 0;

    [Header("기본 정보")]
    [SerializeField] private string _upgradeName;
    [TextArea] [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private UpgradeType _type;

    [Header("비용")]
    [SerializeField] private long _baseCost = 100;
    [SerializeField] private float _costMultiplier = 1.15f;

    [Header("효과")]
    [SerializeField] private long _effectAmount = 1;

    [Header("레벨 제한")]
    [Tooltip("0 = 무제한")]
    [SerializeField] private int _maxLevel = UnlimitedLevel;

    public string UpgradeName => _upgradeName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public UpgradeType Type => _type;
    public long BaseCost => _baseCost;
    public float CostMultiplier => _costMultiplier;
    public long EffectAmount => _effectAmount;
    public int MaxLevel => _maxLevel;

    private bool HasMaxLevel => _maxLevel > UnlimitedLevel;

    public bool IsMaxLevel(int currentLevel)
    {
        return HasMaxLevel && currentLevel >= _maxLevel;
    }

    public long GetCost(int currentLevel)
    {
        return (long)(_baseCost * Mathf.Pow(_costMultiplier, currentLevel));
    }
}
