using System.Collections;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [SerializeField] private float _spawnDuration = 0.5f;
    [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private ParticleSystem _dustParticle;

    private Vector3 _originalScale;
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    public void PlaySpawnAnimation()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(SpawnAnimationRoutine());
    }

    private IEnumerator SpawnAnimationRoutine()
    {
        transform.localScale = Vector3.zero;

        if (_dustParticle != null)
        {
            _dustParticle.Play();
        }

        float elapsed = 0f;
        while (elapsed < _spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _spawnDuration;
            float scaleValue = _scaleCurve.Evaluate(t);
            transform.localScale = _originalScale * scaleValue;
            yield return null;
        }

        transform.localScale = _originalScale;
        _spawnCoroutine = null;
    }
}
