using System;
using UnityEngine;

public enum UpgradeType
{
    WoodPerClick,
    SpawnLumberjack,
    LumberjackProduction
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Lumberman/Upgrade")]
public class UpgradeSpecData : ScriptableObject
{
    private const int UnlimitedLevel = 0;

    [Header("기본 정보")]
    [SerializeField] private string _upgradeName;
    [TextArea] [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private UpgradeType _type;

    [Header("비용")]
    [SerializeField] private CurrencyValue _baseCost = 100;
    [SerializeField] private double _costMultiplier = 1.15;

    [Header("비용 파동")]
    [Tooltip("파동 주기 (레벨 단위). 0 = 파동 비활성화")]
    [SerializeField] private int _cycleLength = 25;
    [Tooltip("파동 진폭 (±%). 0.05 = ±5%")]
    [Range(0f, 0.2f)]
    [SerializeField] private double _waveAmplitude = 0.05;

    [Header("효과")]
    [SerializeField] private CurrencyValue _effectAmount = 1;

    [Header("레벨 제한")]
    [Tooltip("0 = 무제한")]
    [SerializeField] private int _maxLevel = UnlimitedLevel;

    public string UpgradeName => _upgradeName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public UpgradeType Type => _type;
    public CurrencyValue BaseCost => _baseCost;
    public double CostMultiplier => _costMultiplier;
    public CurrencyValue EffectAmount => _effectAmount;
    public int MaxLevel => _maxLevel;

    private bool HasMaxLevel => _maxLevel > UnlimitedLevel;

    public bool IsMaxLevel(int currentLevel)
    {
        return HasMaxLevel && currentLevel >= _maxLevel;
    }

    public CurrencyValue GetCost(int currentLevel)
    {
        CurrencyValue baseCost = _baseCost * CurrencyValue.Pow(_costMultiplier, currentLevel);

        if (_cycleLength <= 0)
            return CurrencyValue.Max(baseCost.Floor(), CurrencyValue.One);

        double phase = 2 * Math.PI * currentLevel / _cycleLength;
        double waveMultiplier = 1 + _waveAmplitude * Math.Sin(phase);

        return CurrencyValue.Max((baseCost * waveMultiplier).Floor(), CurrencyValue.One);
    }
}
