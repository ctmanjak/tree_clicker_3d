using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float _shakeDuration = 0.05f;
    [SerializeField] private float _shakeMagnitude = 0.1f;

    private Vector3 _originalPosition;
    private Coroutine _shakeCoroutine;
    private GameEvents _gameEvents;

    private void Start()
    {
        _originalPosition = transform.localPosition;

        if (ServiceLocator.TryGet(out _gameEvents))
        {
            _gameEvents.OnTreeHit += Shake;
        }
    }

    private void OnDestroy()
    {
        if (_gameEvents != null)
        {
            _gameEvents.OnTreeHit -= Shake;
        }
    }

    public void Shake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            transform.localPosition = _originalPosition;
        }
        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * _shakeMagnitude;
            float y = Random.Range(-1f, 1f) * _shakeMagnitude;
            transform.localPosition = _originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPosition;
        _shakeCoroutine = null;
    }
}
