using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ToastMessage : MonoBehaviour
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    [System.Serializable]
    private class ToastColors
    {
        public Color InfoColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        public Color SuccessColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);
        public Color WarningColor = new Color(0.8f, 0.6f, 0.1f, 0.9f);
        public Color ErrorColor = new Color(0.7f, 0.2f, 0.2f, 0.9f);
    }

    [Header("Animation Settings")]
    [SerializeField] private float _slideDistance = 100f;
    [SerializeField] private float _slideInDuration = 0.2f;
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeOutDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private ToastColors _colors;

    [Header("References")]
    [SerializeField] private RectTransform _toastContainer;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Icons")]
    [SerializeField] private Sprite _infoIcon;
    [SerializeField] private Sprite _successIcon;
    [SerializeField] private Sprite _warningIcon;
    [SerializeField] private Sprite _errorIcon;

    [Header("Audio")]
    [SerializeField] private bool _playSound = true;

    private static ToastMessage _instance;
    private Queue<ToastData> _messageQueue = new Queue<ToastData>();
    private AudioManager _audioManager;
    private bool _isShowing;
    private Sequence _currentSequence;

    private struct ToastData
    {
        public string Message;
        public ToastType Type;
        public Sprite CustomIcon;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_toastContainer == null)
        {
            _toastContainer = GetComponent<RectTransform>();
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        HideImmediate();
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);
    }

    private void OnDestroy()
    {
        _currentSequence?.Kill();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void Show(string message, ToastType type = ToastType.Info)
    {
        if (_instance == null)
        {
            Debug.LogWarning("ToastMessage instance not found");
            return;
        }

        _instance.EnqueueMessage(message, type, null);
    }

    public static void Show(string message, Sprite icon)
    {
        if (_instance == null)
        {
            Debug.LogWarning("ToastMessage instance not found");
            return;
        }

        _instance.EnqueueMessage(message, ToastType.Info, icon);
    }

    private void EnqueueMessage(string message, ToastType type, Sprite customIcon)
    {
        _messageQueue.Enqueue(new ToastData
        {
            Message = message,
            Type = type,
            CustomIcon = customIcon
        });

        if (!_isShowing)
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (_messageQueue.Count == 0)
        {
            _isShowing = false;
            return;
        }

        _isShowing = true;
        var data = _messageQueue.Dequeue();

        SetupToast(data);
        PlayShowAnimation();
    }

    private void SetupToast(ToastData data)
    {
        _messageText.text = data.Message;

        Color bgColor = data.Type switch
        {
            ToastType.Success => _colors.SuccessColor,
            ToastType.Warning => _colors.WarningColor,
            ToastType.Error => _colors.ErrorColor,
            _ => _colors.InfoColor
        };
        _backgroundImage.color = bgColor;

        Sprite icon = data.CustomIcon ?? data.Type switch
        {
            ToastType.Success => _successIcon,
            ToastType.Warning => _warningIcon,
            ToastType.Error => _errorIcon,
            _ => _infoIcon
        };

        _iconImage.gameObject.SetActive(icon != null);
        if (icon != null)
        {
            _iconImage.sprite = icon;
        }
    }

    private void PlayShowAnimation()
    {
        _currentSequence?.Kill();

        if (_playSound)
        {
            _audioManager?.PlaySFX(SFXType.Notification);
        }

        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _toastContainer.anchoredPosition = new Vector2(0, -_slideDistance);

        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(_toastContainer.DOAnchorPosY(0, _slideInDuration).SetEase(Ease.OutQuad));
        _currentSequence.AppendInterval(_displayDuration);
        _currentSequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InQuad));

        _currentSequence.OnComplete(() =>
        {
            HideImmediate();
            ShowNextMessage();
        });
    }

    private void HideImmediate()
    {
        gameObject.SetActive(false);
        _canvasGroup.alpha = 0f;
    }
}
