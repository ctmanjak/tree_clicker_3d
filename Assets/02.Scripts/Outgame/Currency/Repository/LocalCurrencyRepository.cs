using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalCurrencyRepository : ICurrencyRepository
{
    private const string SAVE_KEY = "CurrencyData";

    private readonly Dictionary<CurrencyType, Currency> _currencies = new();

    public void Initialize()
    {
        LoadFromPlayerPrefs();
        InitializeDefaultCurrencies();
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
        Save();
    }

    public void Save()
    {
        var saveData = new CurrencySaveDataCollection();
        foreach (var kvp in _currencies)
        {
            saveData.Items.Add(new CurrencySaveData
            {
                Type = kvp.Key.ToString(),
                Amount = kvp.Value.Amount.ToDouble()
            });
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void LoadFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        var saveData = JsonUtility.FromJson<CurrencySaveDataCollection>(json);

        if (saveData?.Items == null) return;

        foreach (var item in saveData.Items)
        {
            if (Enum.TryParse<CurrencyType>(item.Type, out var type))
            {
                var currency = new Currency(type);
                currency.SetAmount(item.Amount);
                _currencies[type] = currency;
            }
        }
    }

    [Serializable]
    private class CurrencySaveDataCollection
    {
        public List<CurrencySaveData> Items = new();
    }

    [Serializable]
    private class CurrencySaveData
    {
        public string Type;
        public double Amount;
    }
}
