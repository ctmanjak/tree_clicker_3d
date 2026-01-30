public class Currency
{
    public CurrencyType Type { get; }
    public CurrencyValue Amount { get; private set; }
    public CurrencyValue PerClick { get; private set; }

    public Currency(CurrencyType type, CurrencyValue amount = default, CurrencyValue perClick = default)
    {
        Type = type;
        Amount = amount;
        PerClick = perClick.IsZero ? CurrencyValue.One : perClick;
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

    public void IncreasePerClick(CurrencyValue value)
    {
        if (!value.IsPositive) return;
        PerClick += value;
    }

    public void SetAmount(CurrencyValue value)
    {
        Amount = value;
    }

    public void SetPerClick(CurrencyValue value)
    {
        PerClick = value;
    }
}
