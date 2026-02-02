using System;
using UnityEngine;

public class Upgrade
{
    public event Action<Upgrade> OnLevelChanged;

    private UpgradeSpecData Spec { get; }
    public int Level { get; private set; }

    public string Name => Spec.UpgradeName;
    public string Description => Spec.Description;
    public Sprite Icon => Spec.Icon;
    public UpgradeType Type => Spec.Type;
    public bool IsMaxLevel => Spec.IsMaxLevel(Level);
    public CurrencyValue CurrentCost => Spec.GetCost(Level);
    public CurrencyValue EffectAmount => Spec.EffectAmount;

    public Upgrade(UpgradeSpecData spec, int level)
    {
        if (spec == null)
            throw new ArgumentNullException(nameof(spec));

        ValidateSpec(spec);
        ValidateLevel(spec, level);

        Spec = spec;
        Level = level;
    }

    private static void ValidateSpec(UpgradeSpecData spec)
    {
        if (string.IsNullOrWhiteSpace(spec.UpgradeName))
            throw new ArgumentException("업그레이드 이름은 비어있을 수 없습니다.", nameof(spec));

        if (spec.BaseCost <= CurrencyValue.Zero)
            throw new ArgumentException("기본 비용은 0보다 커야 합니다.", nameof(spec));

        if (spec.CostMultiplier < 1)
            throw new ArgumentException("비용 배율은 1 이상이어야 합니다.", nameof(spec));

        if (spec.EffectAmount <= CurrencyValue.Zero)
            throw new ArgumentException("효과량은 0보다 커야 합니다.", nameof(spec));

        if (spec.MaxLevel < 0)
            throw new ArgumentException("최대 레벨은 0 이상이어야 합니다.", nameof(spec));
    }

    private static void ValidateLevel(UpgradeSpecData spec, int level)
    {
        if (level < 0)
            throw new ArgumentOutOfRangeException(nameof(level), "레벨은 0 이상이어야 합니다.");

        if (spec.MaxLevel > 0 && level > spec.MaxLevel)
            throw new ArgumentOutOfRangeException(nameof(level), $"레벨은 최대 레벨({spec.MaxLevel})을 초과할 수 없습니다.");
    }

    public void SetLevel(int level)
    {
        if (Level == level) return;

        Level = level;
        OnLevelChanged?.Invoke(this);
    }

    public void IncrementLevel()
    {
        SetLevel(Level + 1);
    }
}
