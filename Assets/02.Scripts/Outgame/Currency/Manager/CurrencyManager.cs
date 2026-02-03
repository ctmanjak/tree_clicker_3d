using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class CurrencyManager : MonoBehaviour
{
    public event Action<CurrencyType, CurrencyValue> OnCurrencyChanged;
    public event Action<CurrencyType, CurrencyValue> OnCurrencyAdded;

    private ICurrencyRepository _repository;
    private Dictionary<CurrencyType, Currency> _currencies;

    public CurrencyValue GetAmount(CurrencyType type) => GetCurrency(type).Amount;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private async UniTaskVoid Start()
    {
        await GameBootstrap.Instance.Initialization;
        ServiceLocator.TryGet(out _repository);
        await InitializeCurrencies();
        BroadcastInitialValues();
    }

    private async UniTask InitializeCurrencies()
    {
        _currencies = new Dictionary<CurrencyType, Currency>();

        var savedData = await _repository.Initialize();
        foreach (var data in savedData)
        {
            if (Enum.TryParse<CurrencyType>(data.Type, out var type))
                _currencies[type] = new Currency(type, data.Amount);
        }

        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (!_currencies.ContainsKey(type))
                _currencies[type] = new Currency(type);
        }
    }

    private void BroadcastInitialValues()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            OnCurrencyChanged?.Invoke(type, GetAmount(type));
        }
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public Currency GetCurrency(CurrencyType type)
    {
        return _currencies[type];
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
        var key = type.ToString();
        _repository.Save(new CurrencySaveData
        {
            Id = key,
            Type = key,
            Amount = currency.Amount.ToDouble()
        });

        OnCurrencyAdded?.Invoke(type, amount);
        OnCurrencyChanged?.Invoke(type, currency.Amount);
    }

    public bool Spend(CurrencyType type, CurrencyValue amount)
    {
        if (!amount.IsPositive) return false;

        var currency = GetCurrency(type);
        if (currency.TrySpend(amount))
        {
            var key = type.ToString();
            _repository.Save(new CurrencySaveData
            {
                Id = key,
                Type = key,
                Amount = currency.Amount.ToDouble()
            });
            OnCurrencyChanged?.Invoke(type, currency.Amount);
            return true;
        }
        return false;
    }
}
