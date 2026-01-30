using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private ICurrencyRepository _repository;
    private GameEvents _gameEvents;

    public CurrencyValue CurrentWood => GetCurrency(CurrencyType.Wood).Amount;
    public CurrencyValue WoodPerClick => GetCurrency(CurrencyType.Wood).PerClick;

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

    public void AddWood(CurrencyValue amount)
    {
        if (!amount.IsPositive) return;

        var currency = GetCurrency(CurrencyType.Wood);
        currency.Add(amount);
        _repository.SaveCurrency(currency);

        _gameEvents?.RaiseCurrencyAdded(CurrencyType.Wood, amount);
        _gameEvents?.RaiseCurrencyChanged(CurrencyType.Wood, currency.Amount);
    }

    public bool SpendWood(CurrencyValue amount)
    {
        if (!amount.IsPositive) return false;

        var currency = GetCurrency(CurrencyType.Wood);
        if (currency.TrySpend(amount))
        {
            _repository.SaveCurrency(currency);
            _gameEvents?.RaiseCurrencyChanged(CurrencyType.Wood, currency.Amount);
            return true;
        }
        return false;
    }

    public void IncreaseWoodPerClick(CurrencyValue amount)
    {
        if (!amount.IsPositive) return;

        var currency = GetCurrency(CurrencyType.Wood);
        currency.IncreasePerClick(amount);
        _repository.SaveCurrency(currency);

        _gameEvents?.RaisePerClickChanged(CurrencyType.Wood, currency.PerClick);
    }
}
