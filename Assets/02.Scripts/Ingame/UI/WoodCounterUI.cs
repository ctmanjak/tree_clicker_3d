using UnityEngine;
using TMPro;

public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _woodText;

    private CurrencyManager _currencyManager;
    private bool _isSubscribed;

    private void OnEnable()
    {
        ServiceLocator.TryGet(out _currencyManager);
        Subscribe();

        if (_currencyManager != null)
        {
            UpdateDisplay(_currencyManager.GetAmount(CurrencyType.Wood));
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed || _currencyManager == null) return;

        _currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || _currencyManager == null) return;

        _currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
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
