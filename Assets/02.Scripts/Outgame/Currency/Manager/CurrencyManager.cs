using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public event Action<CurrencyType, CurrencyValue> OnCurrencyChanged;
    public event Action<CurrencyType, CurrencyValue> OnCurrencyAdded;

    private ICurrencyRepository _repository;

    public CurrencyValue GetAmount(CurrencyType type) => GetCurrency(type).Amount;

    private void Awake()
    {
        ServiceLocator.Register(this);
        ServiceLocator.TryGet(out _repository);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public Currency GetCurrency(CurrencyType type)
    {
        return _repository.GetCurrency(type);
    }

    public bool CanAfford(CurrencyType type, CurrencyValue amount)
    {
        return GetCurrency(type).CanAfford(amount);
    }

    public void Add(CurrencyType type, CurrencyValue amount)
    {
        if (!amount.IsPositive) return;

        var currency = GetCurrency(type);
        currency.Add(amount);
        _repository.SaveCurrency(currency);

        OnCurrencyAdded?.Invoke(type, amount);
        OnCurrencyChanged?.Invoke(type, currency.Amount);
    }

    public bool Spend(CurrencyType type, CurrencyValue amount)
    {
        if (!amount.IsPositive) return false;

        var currency = GetCurrency(type);
        if (currency.TrySpend(amount))
        {
            _repository.SaveCurrency(currency);
            OnCurrencyChanged?.Invoke(type, currency.Amount);
            return true;
        }
        return false;
    }

}
