using System;
using UnityEngine;

namespace Outgame
{
    public class Upgrade
    {
        public event Action<Upgrade> OnLevelChanged;

        private UpgradeSpecData Spec { get; }
        public int Level { get; private set; }

        private readonly IUpgradeEffectHandler _effectHandler;
        private CurrencyValue _cachedCost;

        public string Id => Spec.Id;
        public string Name => Spec.UpgradeName;
        public string Description => Spec.Description;
        public Sprite Icon => Spec.Icon;
        public UpgradeType Type => Spec.Type;
        public bool IsMaxLevel => Spec.IsMaxLevel(Level);
        public CurrencyValue CurrentCost => _cachedCost;
        public CurrencyValue EffectAmount => Spec.EffectAmount;

        public Upgrade(UpgradeSpecData spec, int level, IUpgradeEffectHandler effectHandler = null)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            ValidateSpec(spec);
            ValidateLevel(spec, level);

            Spec = spec;
            Level = level;
            _effectHandler = effectHandler;
            _cachedCost = Spec.GetCost(Level);
        }

        private static void ValidateSpec(UpgradeSpecData spec)
        {
            if (string.IsNullOrWhiteSpace(spec.Id))
                throw new ArgumentException("업그레이드 ID는 비어있을 수 없습니다.", nameof(spec));

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
            if (!IsValidLevel(level)) return;

            Level = level;
            _cachedCost = Spec.GetCost(Level);
            OnLevelChanged?.Invoke(this);
        }

        public void IncrementLevel()
        {
            if (IsMaxLevel) return;
            SetLevel(Level + 1);
        }

        public void ApplyEffect() => _effectHandler?.OnEffectApplied(this);

        public string GetEffectText() => _effectHandler?.GetEffectText(this) ?? "";

        private bool IsValidLevel(int level)
        {
            if (level < 0) return false;
            if (Spec.MaxLevel > 0 && level > Spec.MaxLevel) return false;
            return true;
        }
    }
}
