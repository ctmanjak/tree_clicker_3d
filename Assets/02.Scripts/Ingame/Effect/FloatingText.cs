using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _fadeDuration = 0.8f;
    [SerializeField] private float _textMinWidth = 0f;
    [SerializeField] private float _textMaxWidth = 320f;

    [Header("Style Settings")]
    [SerializeField] private FloatingTextStyleProvider _styleProvider;

    private RectTransform _textRectTransform;
    private Vector3 _baseScale;

    private Action<FloatingText> _onComplete;
    private Transform _cameraTransform;
    private FloatingTextStyle _currentStyle = FloatingTextStyle.Normal;

    private void Awake()
    {
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        _textRectTransform = _text.GetComponent<RectTransform>();
        _baseScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (_cameraTransform != null)
        {
            transform.rotation = _cameraTransform.rotation;
        }
    }

    public void Initialize(Action<FloatingText> returnToPool)
    {
        _onComplete = returnToPool;
    }

    public void Show(string content, Vector3 worldPosition)
    {
        Show(content, worldPosition, FloatingTextStyle.Normal);
    }

    public void Show(string content, Vector3 worldPosition, FloatingTextStyle style)
    {
        _currentStyle = style;
        _text.text = content;

        ApplyStyle();

        float preferredWidth = _text.GetPreferredValues(content).x;
        float width = Mathf.Clamp(preferredWidth, _textMinWidth, _textMaxWidth);
        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        transform.position = worldPosition;
        transform.SetAsLastSibling();
        StopAllCoroutines();
        StartCoroutine(AnimateAndHide());
    }

    private void ApplyStyle()
    {
        if (_styleProvider != null)
        {
            _styleProvider.SetStyle(_currentStyle);
            _text.color = _styleProvider.TextColor;
            _text.fontSize = _styleProvider.FontSize;
            transform.localScale = _baseScale * _styleProvider.InitialScale;
        }
        else
        {
            _text.color = _currentStyle switch
            {
                FloatingTextStyle.Critical => new Color(1f, 0.9f, 0f),
                FloatingTextStyle.Bonus => new Color(0.3f, 1f, 0.3f),
                _ => Color.white
            };
            transform.localScale = _baseScale;
        }
    }

    private IEnumerator AnimateAndHide()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Color startColor = _text.color;

        float duration;
        float floatDistance;
        Func<float, float> evaluateMove;
        Func<float, float> evaluateAlpha;
        Func<float, float> evaluateScale;

        if (_styleProvider != null)
        {
            duration = _styleProvider.Duration;
            floatDistance = _styleProvider.FloatDistance;
            evaluateMove = _styleProvider.MoveCurve.Evaluate;
            evaluateAlpha = _styleProvider.AlphaCurve.Evaluate;
            evaluateScale = _styleProvider.ScaleCurve.Evaluate;
        }
        else
        {
            duration = _fadeDuration;
            floatDistance = _floatSpeed;
            evaluateMove = t => t;
            evaluateAlpha = t => 1f - t;
            evaluateScale = _ => 1f;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            transform.position = startPos + Vector3.up * floatDistance * evaluateMove(t);
            transform.localScale = startScale * evaluateScale(t);
            _text.color = new Color(startColor.r, startColor.g, startColor.b, evaluateAlpha(t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _baseScale;
        _onComplete?.Invoke(this);
    }
}
