using UnityEngine;
using TMPro;

public class WoodCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _woodText;

    private WoodCounterAnimator _animator;
    private GameManager _gameManager;
    private GameEvents _gameEvents;
    private bool _isSubscribed;
    private long _previousAmount;
    private long _lastMilestone;

    private void Awake()
    {
        _animator = GetComponent<WoodCounterAnimator>();
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _gameManager);
        ServiceLocator.TryGet(out _gameEvents);
        Subscribe();

        _previousAmount = _gameManager?.CurrentWood ?? 0;
        _lastMilestone = GetCurrentMilestone(_previousAmount);
        UpdateDisplay(_previousAmount);
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

        _gameEvents.OnWoodChanged += UpdateDisplay;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed || _gameEvents == null) return;

        _gameEvents.OnWoodChanged -= UpdateDisplay;
        _isSubscribed = false;
    }

    private void UpdateDisplay(long amount)
    {
        if (_woodText != null)
        {
            _woodText.text = FormatNumber(amount);
        }

        TryPlayGainAnimation(amount);
        _previousAmount = amount;
    }

    private void TryPlayGainAnimation(long amount)
    {
        if (_animator == null || amount <= _previousAmount) return;

        long currentMilestone = GetCurrentMilestone(amount);
        if (currentMilestone > _lastMilestone)
        {
            _animator.PlayMilestoneAnimation();
            _lastMilestone = currentMilestone;
        }
        else
        {
            _animator.PlayGainAnimation(amount - _previousAmount);
        }
    }

    private long GetCurrentMilestone(long amount)
    {
        if (amount >= 1_000_000_000) return amount / 1_000_000_000 * 1_000_000_000;
        if (amount >= 1_000_000) return amount / 1_000_000 * 1_000_000;
        if (amount >= 1_000) return amount / 1_000 * 1_000;
        return 0;
    }

    private string FormatNumber(long num)
    {
        if (num >= 1_000_000_000) return $"{num / 1_000_000_000f:F1}B";
        if (num >= 1_000_000) return $"{num / 1_000_000f:F1}M";
        if (num >= 1_000) return $"{num / 1_000f:F1}K";
        return num.ToString();
    }
}
