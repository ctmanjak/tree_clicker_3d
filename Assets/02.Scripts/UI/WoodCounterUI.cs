using UnityEngine;
using TMPro;

public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _woodText;

    private CurrencyManager _currencyManager;
    private GameEvents _gameEvents;
    private bool _isSubscribed;

    private void Start()
    {
        ServiceLocator.TryGet(out _currencyManager);
        ServiceLocator.TryGet(out _gameEvents);
        Subscribe();
        UpdateDisplay(_currencyManager?.GetAmount(CurrencyType.Wood) ?? CurrencyValue.Zero);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed || _gameEvents == null) return;

        _gameEvents.OnCurrencyChanged += OnCurrencyChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || _gameEvents == null) return;

        _gameEvents.OnCurrencyChanged -= OnCurrencyChanged;
        _isSubscribed = false;
    }

    private void OnCurrencyChanged(CurrencyType type, CurrencyValue amount)
    {
        if (type != CurrencyType.Wood) return;
        UpdateDisplay(amount);
    }

    private void UpdateDisplay(CurrencyValue amount)
    {
        if (_woodText != null)
        {
            _woodText.text = amount.ToFormattedString();
        }
    }
}
