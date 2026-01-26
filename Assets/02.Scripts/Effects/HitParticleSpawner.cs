using UnityEngine;

public class HitParticleSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem _woodChipParticle;
    [SerializeField] private ParticleSystem _leafParticle;

    private void Start()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit += SpawnParticles;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit -= SpawnParticles;
        }
    }

    private void SpawnParticles()
    {
        _woodChipParticle?.Play();
        _leafParticle?.Play();
    }
}
