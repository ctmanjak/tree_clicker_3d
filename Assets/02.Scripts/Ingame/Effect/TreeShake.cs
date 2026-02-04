using System.Collections;
using UnityEngine;

namespace Ingame
{
    public class TreeShake : MonoBehaviour
    {
        [SerializeField] private float _shakeAngle = 15f;
        [SerializeField] private float _shakeDuration = 0.2f;

        private Quaternion _originalRotation;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _originalRotation = transform.rotation;
        }

        public void Shake()
        {
            Shake(null);
        }

        public void Shake(Vector3? attackerPosition)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                transform.rotation = _originalRotation;
            }

            float tiltDirection = CalculateTiltDirection(attackerPosition);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(tiltDirection));
        }

        private float CalculateTiltDirection(Vector3? attackerPosition)
        {
            if (attackerPosition == null)
            {
                return -1f;
            }

            Vector3 toAttacker = attackerPosition.Value - transform.position;
            Vector3 right = Vector3.Cross(Vector3.up, toAttacker).normalized;
            float dot = Vector3.Dot(right, Vector3.right);

            return dot >= 0 ? -1f : 1f;
        }

        private IEnumerator ShakeCoroutine(float tiltDirection)
        {

            Quaternion targetRotation = Quaternion.Euler(
                _originalRotation.eulerAngles.x,
                _originalRotation.eulerAngles.y,
                _originalRotation.eulerAngles.z + _shakeAngle * tiltDirection
            );

            float elapsed = 0f;
            float tiltDuration = _shakeDuration * 0.3f;
            while (elapsed < tiltDuration)
            {
                float t = elapsed / tiltDuration;
                t = 1 - (1 - t) * (1 - t);
                transform.rotation = Quaternion.Lerp(_originalRotation, targetRotation, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            float returnDuration = _shakeDuration * 0.7f;
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
}
