using UnityEngine;
using TMPro;

public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _woodText;

    private void Start()
    {
        UpdateDisplay(GameManager.Instance.CurrentWood);
    }

    private void OnEnable()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodChanged += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodChanged -= UpdateDisplay;
        }
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
