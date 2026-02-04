using System;

namespace Outgame
{
    public class Currency
    {
        public CurrencyType Type { get; }
        public CurrencyValue Amount { get; private set; }

        public Currency(CurrencyType type, CurrencyValue amount = default)
        {
            if (amount.IsNegative)
                throw new ArgumentException("재화량은 음수일 수 없습니다.", nameof(amount));

            Type = type;
            Amount = amount;
        }

        public bool CanAfford(CurrencyValue cost)
        {
            return Amount >= cost;
        }

        public bool TrySpend(CurrencyValue cost)
        {
            if (!cost.IsPositive || !CanAfford(cost))
                return false;

            Amount -= cost;
            return true;
        }

        public void Add(CurrencyValue value)
        {
            if (!value.IsPositive) return;
            Amount += value;
        }

        public void SetAmount(CurrencyValue value)
        {
            if (value.IsNegative)
                throw new ArgumentException("재화량은 음수일 수 없습니다.", nameof(value));

            Amount = value;
        }
    }
}
