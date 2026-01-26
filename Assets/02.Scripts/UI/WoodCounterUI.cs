using UnityEngine;
using TMPro;

public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _woodText;

    private bool _isSubscribed;

    private void Start()
    {
        Subscribe();
        UpdateDisplay(GameManager.Instance.CurrentWood);
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
        if (_isSubscribed || GameEvents.Instance == null) return;

        GameEvents.Instance.OnWoodChanged += UpdateDisplay;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || GameEvents.Instance == null) return;

        GameEvents.Instance.OnWoodChanged -= UpdateDisplay;
        _isSubscribed = false;
    }

    private void UpdateDisplay(long amount)
    {
        if (_woodText != null)
        {
            _woodText.text = FormatNumber(amount);
        }
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000_000) return $"{num / 1_000_000_000f:F1}B";
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
