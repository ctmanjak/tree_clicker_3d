using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.05f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private Vector3 _originalPosition;
    private Coroutine _shakeCoroutine;

    private void Start()
    {
        _originalPosition = transform.localPosition;

        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit += Shake;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit -= Shake;
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

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = _originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPosition;
        _shakeCoroutine = null;
    }
}
