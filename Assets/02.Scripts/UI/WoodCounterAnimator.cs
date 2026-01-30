using UnityEngine;
using TMPro;
using DG.Tweening;

public class WoodCounterAnimator : MonoBehaviour
{
    [Header("Punch Settings")]
    [SerializeField] private float _punchScale = 0.2f;
    [SerializeField] private float _punchDuration = 0.15f;
    [SerializeField] private int _punchVibrato = 1;

    [Header("Color Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _gainColor = Color.yellow;
    [SerializeField] private float _colorFadeDuration = 0.2f;

    [Header("Milestone Settings")]
    [SerializeField] private float _milestonePunchScale = 0.4f;
    [SerializeField] private float _milestoneDuration = 0.3f;
    [SerializeField] private Color _milestoneColor = new Color(1f, 0.8f, 0f);

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private RectTransform _targetTransform;

    private Sequence _currentSequence;
    private AudioManager _audioManager;

    private void Awake()
    {
        if (_targetTransform == null)
        {
            _targetTransform = GetComponent<RectTransform>();
        }

        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);
    }

    private void OnDestroy()
    {
        _currentSequence?.Kill();
    }

    public void PlayGainAnimation(CurrencyValue amount)
    {
        if (_targetTransform == null) return;

        _currentSequence?.Kill();
        _targetTransform.localScale = Vector3.one;

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(_targetTransform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, _punchVibrato, 0.5f));

        if (_text != null)
        {
            _text.color = _gainColor;
            _currentSequence.Join(_text.DOColor(_normalColor, _colorFadeDuration).SetDelay(_punchDuration * 0.5f));
        }
    }

    public void PlayMilestoneAnimation()
    {
        if (_targetTransform == null) return;

        _audioManager?.PlaySFX(SFXType.Milestone);

        _currentSequence?.Kill();
        _targetTransform.localScale = Vector3.one;

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(_targetTransform.DOPunchScale(Vector3.one * _milestonePunchScale, _milestoneDuration, 2, 0.3f));

        if (_text != null)
        {
            _text.color = _milestoneColor;
            _currentSequence.Join(_text.DOColor(_normalColor, _milestoneDuration * 1.5f));
        }
    }
}
