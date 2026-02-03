using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirebaseCurrencyRepository : ICurrencyRepository
{
    private const string CollectionName = "currencies";

    private readonly IFirebaseStoreService _storeService;
    private readonly Dictionary<CurrencyType, Currency> _currencies = new();

    public FirebaseCurrencyRepository(IFirebaseStoreService storeService)
    {
        _storeService = storeService;
    }

    public async UniTask Initialize()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (!_currencies.ContainsKey(type))
            {
                _currencies[type] = new Currency(type);
            }
        }

        var documents = await _storeService.GetCollection(CollectionName);

        foreach (var doc in documents)
        {
            if (!doc.TryGetValue("_documentId", out var docIdObj))
                continue;

            string docId = docIdObj.ToString();
            if (!Enum.TryParse<CurrencyType>(docId, out var type))
                continue;

            if (!doc.TryGetValue("amount", out var amountObj))
                continue;

            double amount = Convert.ToDouble(amountObj);
            if (_currencies.TryGetValue(type, out var currency))
            {
                currency.SetAmount(amount);
            }
            else
            {
                var newCurrency = new Currency(type);
                newCurrency.SetAmount(amount);
                _currencies[type] = newCurrency;
            }
        }

        Debug.Log($"Firestore에서 재화 데이터 로드 완료 ({documents.Count}건)");
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

        var data = new Dictionary<string, object>
        {
            { "amount", currency.Amount.ToDouble() }
        };
        _storeService.SetDocumentAsync(CollectionName, currency.Type.ToString(), data);
    }

    public void Save()
    {
        foreach (var kvp in _currencies)
        {
            var data = new Dictionary<string, object>
            {
                { "amount", kvp.Value.Amount.ToDouble() }
            };
            _storeService.SetDocumentAsync(CollectionName, kvp.Key.ToString(), data);
        }
    }
}
