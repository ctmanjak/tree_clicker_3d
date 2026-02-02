using System;
using UnityEngine;
using DG.Tweening;

public class PanelTransition : MonoBehaviour
{
    public enum TransitionType
    {
        SlideFromRight,
        SlideFromBottom,
        SlideFromLeft,
        SlideFromTop,
        ScalePopup,
        Fade
    }

    [Header("Transition Settings")]
    [SerializeField] private TransitionType _openTransition = TransitionType.ScalePopup;
    [SerializeField] private TransitionType _closeTransition = TransitionType.ScalePopup;
    [SerializeField] private float _duration = 0.3f;
    [SerializeField] private Ease _openEase = Ease.OutBack;
    [SerializeField] private Ease _closeEase = Ease.InBack;

    [Header("Backdrop")]
    [SerializeField] private CanvasGroup _backdrop;
    [SerializeField] private float _backdropAlpha = 0.5f;

    [Header("Audio")]
    [SerializeField] private bool _playSound = true;

    [Header("References")]
    [SerializeField] private RectTransform _panelTransform;
    [SerializeField] private CanvasGroup _panelCanvasGroup;

    public event Action OnOpenComplete;
    public event Action OnCloseComplete;

    private Vector2 _originalPosition;
    private Sequence _currentSequence;
    private AudioManager _audioManager;
    private bool _isOpen;

    private void Awake()
    {
        if (_panelTransform == null)
        {
            _panelTransform = GetComponent<RectTransform>();
        }

        if (_panelCanvasGroup == null)
        {
            _panelCanvasGroup = GetComponent<CanvasGroup>();
        }

        _originalPosition = _panelTransform.anchoredPosition;
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);
    }

    private void OnDestroy()
    {
        _currentSequence?.Kill();
    }

    public void Open()
    {
        if (_isOpen) return;

        _isOpen = true;
        gameObject.SetActive(true);

        if (_playSound)
        {
            _audioManager?.PlaySFX(SFXType.UIOpen);
        }

        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();

        SetupOpenAnimation(_openTransition);

        if (_backdrop != null)
        {
            _backdrop.gameObject.SetActive(true);
            _backdrop.alpha = 0f;
            _currentSequence.Join(_backdrop.DOFade(_backdropAlpha, _duration));
        }

        _currentSequence.OnComplete(() => OnOpenComplete?.Invoke());
    }

    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;

        if (_playSound)
        {
            _audioManager?.PlaySFX(SFXType.UIClose);
        }

        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();

        SetupCloseAnimation(_closeTransition);

        if (_backdrop != null)
        {
            _currentSequence.Join(_backdrop.DOFade(0f, _duration));
        }

        _currentSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            _backdrop?.gameObject.SetActive(false);
            ResetPanel();
            OnCloseComplete?.Invoke();
        });
    }

    public void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public bool IsOpen => _isOpen;

    private void SetupOpenAnimation(TransitionType type)
    {
        switch (type)
        {
            case TransitionType.SlideFromRight:
                _panelTransform.anchoredPosition = _originalPosition + new Vector2(_panelTransform.rect.width, 0);
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition, _duration).SetEase(_openEase));
                break;

            case TransitionType.SlideFromLeft:
                _panelTransform.anchoredPosition = _originalPosition + new Vector2(-_panelTransform.rect.width, 0);
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition, _duration).SetEase(_openEase));
                break;

            case TransitionType.SlideFromBottom:
                _panelTransform.anchoredPosition = _originalPosition + new Vector2(0, -_panelTransform.rect.height);
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition, _duration).SetEase(_openEase));
                break;

            case TransitionType.SlideFromTop:
                _panelTransform.anchoredPosition = _originalPosition + new Vector2(0, _panelTransform.rect.height);
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition, _duration).SetEase(_openEase));
                break;

            case TransitionType.ScalePopup:
                _panelTransform.localScale = Vector3.zero;
                _currentSequence.Append(_panelTransform.DOScale(1f, _duration).SetEase(_openEase));
                break;

            case TransitionType.Fade:
                if (_panelCanvasGroup != null)
                {
                    _panelCanvasGroup.alpha = 0f;
                    _currentSequence.Append(_panelCanvasGroup.DOFade(1f, _duration).SetEase(_openEase));
                }
                break;
        }
    }

    private void SetupCloseAnimation(TransitionType type)
    {
        switch (type)
        {
            case TransitionType.SlideFromRight:
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition + new Vector2(_panelTransform.rect.width, 0), _duration).SetEase(_closeEase));
                break;

            case TransitionType.SlideFromLeft:
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition + new Vector2(-_panelTransform.rect.width, 0), _duration).SetEase(_closeEase));
                break;

            case TransitionType.SlideFromBottom:
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition + new Vector2(0, -_panelTransform.rect.height), _duration).SetEase(_closeEase));
                break;

            case TransitionType.SlideFromTop:
                _currentSequence.Append(_panelTransform.DOAnchorPos(_originalPosition + new Vector2(0, _panelTransform.rect.height), _duration).SetEase(_closeEase));
                break;

            case TransitionType.ScalePopup:
                _currentSequence.Append(_panelTransform.DOScale(0f, _duration).SetEase(_closeEase));
                break;

            case TransitionType.Fade:
                if (_panelCanvasGroup != null)
                {
                    _currentSequence.Append(_panelCanvasGroup.DOFade(0f, _duration).SetEase(_closeEase));
                }
                break;
        }
    }

    private void ResetPanel()
    {
        _panelTransform.anchoredPosition = _originalPosition;
        _panelTransform.localScale = Vector3.one;

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.alpha = 1f;
        }
    }
}
