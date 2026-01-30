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
    [SerializeField] private FloatingTextAnimator _styleAnimator;

    private RectTransform _textRectTransform;

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
        if (_styleAnimator != null)
        {
            _styleAnimator.SetStyle(_currentStyle);
            _text.color = _styleAnimator.TextColor;
            _text.fontSize = _styleAnimator.FontSize;
            transform.localScale = Vector3.one * _styleAnimator.InitialScale;
        }
        else
        {
            _text.color = _currentStyle switch
            {
                FloatingTextStyle.Critical => new Color(1f, 0.9f, 0f),
                FloatingTextStyle.Bonus => new Color(0.3f, 1f, 0.3f),
                _ => Color.white
            };
        }
    }

    private IEnumerator AnimateAndHide()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Color startColor = _text.color;

        float duration = _styleAnimator != null ? _styleAnimator.Duration : _fadeDuration;
        float floatDistance = _styleAnimator != null ? _styleAnimator.FloatDistance : _floatSpeed;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float moveT = _styleAnimator != null ? _styleAnimator.MoveCurve.Evaluate(t) : t;
            float alphaT = _styleAnimator != null ? _styleAnimator.AlphaCurve.Evaluate(t) : (1 - t);
            float scaleT = _styleAnimator != null ? _styleAnimator.ScaleCurve.Evaluate(t) : 1f;

            transform.position = startPos + Vector3.up * floatDistance * moveT;
            transform.localScale = startScale * scaleT;
            _text.color = new Color(startColor.r, startColor.g, startColor.b, alphaT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
        _onComplete?.Invoke(this);
    }
}
