using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UpgradeButtonAnimator : MonoBehaviour
{
    [Header("Pulse Animation")]
    [SerializeField] private float _pulseScale = 1.05f;
    [SerializeField] private float _pulseDuration = 0.5f;

    [Header("Purchase Animation")]
    [SerializeField] private float _purchaseScale = 1.2f;
    [SerializeField] private float _purchaseDuration = 0.2f;
    [SerializeField] private Color _purchaseFlashColor = new Color(0.3f, 1f, 0.3f);

    [Header("Denied Animation")]
    [SerializeField] private float _shakeStrength = 10f;
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private Color _deniedFlashColor = new Color(1f, 0.3f, 0.3f);

    [Header("References")]
    [SerializeField] private RectTransform _targetTransform;
    [SerializeField] private Image[] _backgroundImages;

    private Tween _pulseTween;
    private Sequence _currentSequence;
    private Color[] _originalColors;
    private AudioManager _audioManager;
    private bool _isPulsing;

    private void Awake()
    {
        if (_targetTransform == null)
        {
            _targetTransform = GetComponent<RectTransform>();
        }

        if (_backgroundImages == null || _backgroundImages.Length == 0)
        {
            var image = GetComponent<Image>();
            if (image != null)
            {
                _backgroundImages = new[] { image };
            }
        }

        if (_backgroundImages != null && _backgroundImages.Length > 0)
        {
            _originalColors = new Color[_backgroundImages.Length];
            for (int i = 0; i < _backgroundImages.Length; i++)
            {
                if (_backgroundImages[i] != null)
                {
                    _originalColors[i] = _backgroundImages[i].color;
                }
            }
        }
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
        _currentSequence?.Kill();
    }

    public void StartPulseAnimation()
    {
        if (_isPulsing || _targetTransform == null) return;

        _isPulsing = true;
        _pulseTween?.Kill();
        _targetTransform.localScale = Vector3.one;

        _pulseTween = _targetTransform
            .DOScale(_pulseScale, _pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopPulseAnimation()
    {
        if (!_isPulsing) return;

        _isPulsing = false;
        _pulseTween?.Kill();
        _targetTransform.DOScale(1f, 0.1f);
    }

    public void PlayPurchaseAnimation()
    {
        if (_targetTransform == null) return;

        bool wasPulsing = _isPulsing;
        StopPulseAnimation();
        _currentSequence?.Kill();
        _targetTransform.localScale = Vector3.one;

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(_targetTransform.DOPunchScale(Vector3.one * (_purchaseScale - 1f), _purchaseDuration, 1, 0.5f));
        AddColorFlashToSequence(_currentSequence, _purchaseFlashColor, _purchaseDuration);

        if (wasPulsing)
        {
            _currentSequence.OnComplete(StartPulseAnimation);
        }
    }

    public void PlayDeniedAnimation()
    {
        if (_targetTransform == null) return;

        _audioManager?.PlaySFX(SFXType.UpgradeDenied);

        bool wasPulsing = _isPulsing;
        StopPulseAnimation();
        _currentSequence?.Kill();
        _targetTransform.localScale = Vector3.one;

        _currentSequence = DOTween.Sequence();
        _currentSequence.Append(_targetTransform.DOShakeAnchorPos(_shakeDuration, new Vector2(_shakeStrength, 0), 10, 90, false, true));
        AddColorFlashToSequence(_currentSequence, _deniedFlashColor, _shakeDuration);

        if (wasPulsing)
        {
            _currentSequence.OnComplete(StartPulseAnimation);
        }
    }

    private void AddColorFlashToSequence(Sequence sequence, Color flashColor, float duration)
    {
        if (_backgroundImages == null || _originalColors == null) return;

        for (int i = 0; i < _backgroundImages.Length; i++)
        {
            if (_backgroundImages[i] != null)
            {
                sequence.Join(_backgroundImages[i].DOColor(flashColor, duration * 0.3f));
            }
        }

        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < _backgroundImages.Length; i++)
            {
                if (_backgroundImages[i] != null)
                {
                    _backgroundImages[i].DOColor(_originalColors[i], duration * 0.7f);
                }
            }
        });
    }
}
