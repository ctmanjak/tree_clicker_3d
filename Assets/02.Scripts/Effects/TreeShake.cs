using System.Collections;
using UnityEngine;

public class TreeShake : MonoBehaviour
{
    [SerializeField] private float shakeAngle = 15f;
    [SerializeField] private float shakeDuration = 0.2f;

    private Quaternion _originalRotation;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _originalRotation = transform.rotation;
    }

    public void Shake()
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            transform.rotation = _originalRotation;
        }
        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        // 카메라 기준 왼쪽으로 기울임 (오른쪽에서 도끼가 찍으니까)
        float tiltDirection = -1f;

        Quaternion targetRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y,
            _originalRotation.eulerAngles.z + shakeAngle * tiltDirection
        );

        float elapsed = 0f;
        float tiltDuration = shakeDuration * 0.3f;
        while (elapsed < tiltDuration)
        {
            float t = elapsed / tiltDuration;
            t = 1 - (1 - t) * (1 - t);
            transform.rotation = Quaternion.Lerp(_originalRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        float returnDuration = shakeDuration * 0.7f;
        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            t = t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
            transform.rotation = Quaternion.Lerp(targetRotation, _originalRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = _originalRotation;
        _shakeCoroutine = null;
    }
}
