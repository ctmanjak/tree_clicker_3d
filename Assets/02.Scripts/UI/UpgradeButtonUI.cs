using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
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
    private bool _isSubscribed;
    private bool _isInitialized;

    public void Init(UpgradeData data, UpgradeManager manager)
    {
        _upgradeData = data;
        _upgradeManager = manager;
        _isInitialized = true;

        if (_gameManager != null)
        {
            UpdateDisplay();
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

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
            UpdateDisplay();
        }
    }

    private void OnWoodChanged(long _)
    {
        UpdateButtonState();
    }

    private void UpdateDisplay()
    {
        int level = _upgradeManager.GetLevel(_upgradeData);
        long cost = _upgradeData.GetCost(level);

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
            _costText.text = $"{FormatNumber(cost)}";
        }

        if (_levelText != null)
        {
            _levelText.text = $"Lv.{level}";
        }

        if (_effectText != null)
        {
            _effectText.text = $"+{_upgradeData.EffectAmount}/클릭";
        }

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (_upgradeManager == null || _gameManager == null) return;

        int level = _upgradeManager.GetLevel(_upgradeData);
        long cost = _upgradeData.GetCost(level);
        _button.interactable = _gameManager.CanAfford(cost);
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
