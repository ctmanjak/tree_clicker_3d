using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private ICurrencyRepository _repository;
    private GameEvents _gameEvents;

    public CurrencyValue GetAmount(CurrencyType type) => GetCurrency(type).Amount;
    public CurrencyValue GetPerClick(CurrencyType type) => GetCurrency(type).PerClick;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _repository);
        ServiceLocator.TryGet(out _gameEvents);
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

        _gameEvents?.RaiseCurrencyAdded(type, amount);
        _gameEvents?.RaiseCurrencyChanged(type, currency.Amount);
    }

    public bool Spend(CurrencyType type, CurrencyValue amount)
    {
        if (!amount.IsPositive) return false;

        var currency = GetCurrency(type);
        if (currency.TrySpend(amount))
        {
            _repository.SaveCurrency(currency);
            _gameEvents?.RaiseCurrencyChanged(type, currency.Amount);
            return true;
        }
        return false;
    }

    public void IncreasePerClick(CurrencyType type, CurrencyValue amount)
    {
        if (!amount.IsPositive) return;

        var currency = GetCurrency(type);
        currency.IncreasePerClick(amount);
        _repository.SaveCurrency(currency);

        _gameEvents?.RaisePerClickChanged(type, currency.PerClick);
    }
}
