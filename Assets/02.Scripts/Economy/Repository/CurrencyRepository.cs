using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyRepository : MonoBehaviour, ICurrencyRepository, ISaveable
{
    private readonly Dictionary<CurrencyType, Currency> _currencies = new();

    public string SaveKey => "CurrencyRepository";

    private void Awake()
    {
        InitializeDefaultCurrencies();
        ServiceLocator.Register<ICurrencyRepository>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ICurrencyRepository>(this);
    }

    private void InitializeDefaultCurrencies()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (!_currencies.ContainsKey(type))
            {
                _currencies[type] = new Currency(type);
            }
        }
    }

    public Currency GetCurrency(CurrencyType type)
    {
        if (!_currencies.TryGetValue(type, out var currency))
        {
            currency = new Currency(type);
            _currencies[type] = currency;
        }
        return currency;
    }

    public void SaveCurrency(Currency currency)
    {
        _currencies[currency.Type] = currency;
    }

    public object CaptureState()
    {
        var saveData = new Dictionary<string, CurrencySaveData>();
        foreach (var kvp in _currencies)
        {
            saveData[kvp.Key.ToString()] = new CurrencySaveData
            {
                Amount = kvp.Value.Amount.ToDouble(),
                PerClick = kvp.Value.PerClick.ToDouble()
            };
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        if (state is Dictionary<string, CurrencySaveData> saveData)
        {
            foreach (var kvp in saveData)
            {
                if (Enum.TryParse<CurrencyType>(kvp.Key, out var type))
                {
                    var currency = GetCurrency(type);
                    currency.SetAmount(kvp.Value.Amount);
                    currency.SetPerClick(kvp.Value.PerClick);
                }
            }
        }
    }

    [Serializable]
    private class CurrencySaveData
    {
        public double Amount;
        public double PerClick;
    }
}
