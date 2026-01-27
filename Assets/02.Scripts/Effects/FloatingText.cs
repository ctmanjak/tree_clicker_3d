using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _fadeDuration = 0.8f;

    private Action<FloatingText> _onComplete;
    private Transform _cameraTransform;

    private void Awake()
    {
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
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
        _text.text = content;
        _text.color = Color.white;
        transform.position = worldPosition;
        StopAllCoroutines();
        StartCoroutine(AnimateAndHide());
    }

    private IEnumerator AnimateAndHide()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color startColor = _text.color;

        while (elapsed < _fadeDuration)
        {
            float t = elapsed / _fadeDuration;
            transform.position = startPos + Vector3.up * _floatSpeed * t;
            _text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _onComplete?.Invoke(this);
    }
}
