using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private UpgradeButtonAnimator _animator;

    private Upgrade _upgrade;
    private UpgradeManager _upgradeManager;
    private CurrencyManager _currencyManager;
    private AudioManager _audioManager;
    private bool _isSubscribed;
    private bool _wasAffordable;

    public void Init(Upgrade upgrade, UpgradeManager manager)
    {
        _upgrade = upgrade;
        _upgradeManager = manager;
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _currencyManager);
        ServiceLocator.TryGet(out _audioManager);

        if (_upgradeManager == null)
        {
            ServiceLocator.TryGet(out _upgradeManager);
        }

        _button.onClick.AddListener(OnClick);
        Subscribe();
        UpdateDisplay();
    }

    private void OnEnable()
    {
        Subscribe();
        ResetAnimationState();
    }

    private void ResetAnimationState()
    {
        _wasAffordable = false;
        _animator?.StopPulseAnimation();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void Subscribe()
    {
        if (_isSubscribed) return;

        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged += OnCurrencyChanged;
        }

        if (_upgrade != null)
        {
            _upgrade.OnLevelChanged += OnLevelChanged;
        }

        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;

        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
        }

        if (_upgrade != null)
        {
            _upgrade.OnLevelChanged -= OnLevelChanged;
        }

        _isSubscribed = false;
    }

    private void OnClick()
    {
        if (_upgradeManager.TryPurchase(_upgrade))
        {
            _audioManager?.PlaySFX(SFXType.UpgradeBuy);
            _animator?.PlayPurchaseAnimation();
            UpdateDisplay();
        }
        else
        {
            _animator?.PlayDeniedAnimation();
        }
    }

    private void OnCurrencyChanged(CurrencyType type, CurrencyValue _)
    {
        if (type != CurrencyType.Wood) return;
        UpdateButtonState();
    }

    private void OnLevelChanged(Upgrade _)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_upgrade == null) return;

        if (_iconImage != null && _upgrade.Icon != null)
        {
            _iconImage.sprite = _upgrade.Icon;
        }

        if (_nameText != null)
        {
            _nameText.text = _upgrade.Name;
        }

        if (_costText != null)
        {
            _costText.text = _upgrade.IsMaxLevel ? "MAX" : _upgrade.CurrentCost.ToFormattedString();
        }

        if (_levelText != null)
        {
            _levelText.text = _upgrade.IsMaxLevel ? "MAX" : $"Lv.{_upgrade.Level}";
        }

        if (_effectText != null)
        {
            _effectText.text = GetEffectText();
        }

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (_upgrade == null || _upgradeManager == null) return;

        if (_upgrade.IsMaxLevel)
        {
            _button.interactable = false;
            _animator?.StopPulseAnimation();
            return;
        }

        bool canPurchase = _upgradeManager.CanPurchase(_upgrade);

        if (_animator != null)
        {
            if (canPurchase && !_wasAffordable)
            {
                _animator.StartPulseAnimation();
            }
            else if (!canPurchase && _wasAffordable)
            {
                _animator.StopPulseAnimation();
            }
        }

        _wasAffordable = canPurchase;
    }

    private string GetEffectText()
    {
        return _upgrade.GetEffectText();
    }
}
