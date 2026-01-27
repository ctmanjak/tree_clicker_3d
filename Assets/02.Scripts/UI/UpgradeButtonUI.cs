using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UpgradeData upgradeData;

    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI effectText;

    private GameManager _gameManager;
    private bool _isSubscribed;
    private bool _isInitialized;

    public void Init(UpgradeData data, UpgradeManager manager)
    {
        upgradeData = data;
        upgradeManager = manager;
        _isInitialized = true;

        if (_gameManager != null)
        {
            UpdateDisplay();
        }
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;

        if (upgradeManager == null)
        {
            ServiceLocator.TryGet(out upgradeManager);
        }

        button.onClick.AddListener(OnClick);
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
        button.onClick.RemoveListener(OnClick);
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
        if (upgradeManager.TryPurchase(upgradeData))
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
        int level = upgradeManager.GetLevel(upgradeData);
        long cost = upgradeData.GetCost(level);

        if (iconImage != null && upgradeData.icon != null)
        {
            iconImage.sprite = upgradeData.icon;
        }

        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }

        if (costText != null)
        {
            costText.text = $"{FormatNumber(cost)}";
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }

        if (effectText != null)
        {
            effectText.text = $"+{upgradeData.effectAmount}/클릭";
        }

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (upgradeManager == null || _gameManager == null) return;

        int level = upgradeManager.GetLevel(upgradeData);
        long cost = upgradeData.GetCost(level);
        button.interactable = _gameManager.CurrentWood >= cost;
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
