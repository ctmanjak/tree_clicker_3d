using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    private const string SfxUpgradeBuy = "upgrade_buy";

    [Header("Data")]
    [SerializeField] private UpgradeData _upgradeData;

    [Header("References")]
    [SerializeField] private UpgradeManager _upgradeManager;
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _effectText;

    private GameManager _gameManager;
    private AudioManager _audioManager;
    private UpgradeButtonAnimator _animator;
    private bool _isSubscribed;
    private bool _wasAffordable;

    public void Init(UpgradeData data, UpgradeManager manager)
    {
        _upgradeData = data;
        _upgradeManager = manager;

        if (_gameManager != null)
        {
            UpdateDisplay();
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        ServiceLocator.TryGet(out _audioManager);
        _animator = GetComponent<UpgradeButtonAnimator>();

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
        if (_isSubscribed || GameEvents.Instance == null) return;

        GameEvents.Instance.OnWoodChanged += OnWoodChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || GameEvents.Instance == null) return;

        GameEvents.Instance.OnWoodChanged -= OnWoodChanged;
        _isSubscribed = false;
    }

    private void OnClick()
    {
        if (_upgradeManager.TryPurchase(_upgradeData))
        {
            _audioManager?.PlaySFX(SfxUpgradeBuy);
            _animator?.PlayPurchaseAnimation();
            UpdateDisplay();
        }
        else
        {
            _animator?.PlayDeniedAnimation();
        }
    }

    private void OnWoodChanged(long _)
    {
        UpdateButtonState();
    }

    private void UpdateDisplay()
    {
        int level = _upgradeManager.GetLevel(_upgradeData);
        bool isMaxLevel = _upgradeData.IsMaxLevel(level);

        if (_iconImage != null && _upgradeData.Icon != null)
        {
            _iconImage.sprite = _upgradeData.Icon;
        }

        if (_nameText != null)
        {
            _nameText.text = _upgradeData.UpgradeName;
        }

        if (_costText != null)
        {
            _costText.text = isMaxLevel ? "MAX" : FormatNumber(_upgradeData.GetCost(level));
        }

        if (_levelText != null)
        {
            _levelText.text = isMaxLevel ? "MAX" : $"Lv.{level}";
        }

        if (_effectText != null)
        {
            _effectText.text = GetEffectText();
        }

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (_upgradeManager == null || _gameManager == null) return;

        int level = _upgradeManager.GetLevel(_upgradeData);

        if (_upgradeData.IsMaxLevel(level))
        {
            _button.interactable = false;
            _animator?.StopPulseAnimation();
            return;
        }

        long cost = _upgradeData.GetCost(level);
        bool canAfford = _gameManager.CanAfford(cost);

        _button.interactable = true;

        if (_animator != null)
        {
            if (canAfford && !_wasAffordable)
            {
                _animator.StartPulseAnimation();
            }
            else if (!canAfford && _wasAffordable)
            {
                _animator.StopPulseAnimation();
            }
        }

        _wasAffordable = canAfford;
    }

    private string GetEffectText()
    {
        return _upgradeData.Type switch
        {
            UpgradeType.WoodPerClick => $"+{_upgradeData.EffectAmount}/클릭",
            UpgradeType.SpawnLumberjack => "벌목꾼 +1",
            _ => ""
        };
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
