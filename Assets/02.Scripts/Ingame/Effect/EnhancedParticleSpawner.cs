using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Ingame
{
    public class EnhancedParticleSpawner : MonoBehaviour
    {
        [Header("Wood Chips")]
        [SerializeField] private ParticleSystem _woodChipsPrefab;
        [SerializeField] private int _woodChipsPoolSize = 10;

        [Header("Leaves")]
        [SerializeField] private ParticleSystem _leavesPrefab;
        [SerializeField] private int _leavesPoolSize = 10;

        [Header("Critical Effect")]
        [SerializeField] private ParticleSystem _criticalPrefab;
        [SerializeField] private int _criticalPoolSize = 5;

        private ObjectPool<ParticleSystem> _woodChipsPool;
        private ObjectPool<ParticleSystem> _leavesPool;
        private ObjectPool<ParticleSystem> _criticalPool;
        private Dictionary<float, WaitForSeconds> _waitCache = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
            InitializePools();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
            _woodChipsPool?.Clear();
            _leavesPool?.Clear();
            _criticalPool?.Clear();
            _waitCache.Clear();
        }

        private void InitializePools()
        {
            if (_woodChipsPrefab != null)
            {
                _woodChipsPool = new ObjectPool<ParticleSystem>(_woodChipsPrefab, transform, _woodChipsPoolSize);
            }

            if (_leavesPrefab != null)
            {
                _leavesPool = new ObjectPool<ParticleSystem>(_leavesPrefab, transform, _leavesPoolSize);
            }

            if (_criticalPrefab != null)
            {
                _criticalPool = new ObjectPool<ParticleSystem>(_criticalPrefab, transform, _criticalPoolSize);
            }
        }

        public void SpawnWoodChips(Vector3 position, Vector3 direction, int count)
        {
            if (_woodChipsPool == null) return;

            var particle = _woodChipsPool.Get();
            particle.transform.position = position;
            particle.transform.rotation = Quaternion.LookRotation(direction);

            var emission = particle.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, count));

            particle.Play();
            ScheduleReturn(particle, _woodChipsPool);
        }

        public void SpawnLeaves(Vector3 position, int count)
        {
            if (_leavesPool == null) return;

            var particle = _leavesPool.Get();
            particle.transform.position = position;

            var emission = particle.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, count));

            particle.Play();
            ScheduleReturn(particle, _leavesPool);
        }

        public void SpawnCriticalEffect(Vector3 position)
        {
            if (_criticalPool == null) return;

            var particle = _criticalPool.Get();
            particle.transform.position = position;
            particle.Play();
            ScheduleReturn(particle, _criticalPool);
        }

        private void ScheduleReturn(ParticleSystem particle, ObjectPool<ParticleSystem> pool)
        {
            float duration = particle.main.duration + particle.main.startLifetime.constantMax;
            StartCoroutine(ReturnAfterDelay(particle, pool, duration));
        }

        private System.Collections.IEnumerator ReturnAfterDelay(ParticleSystem particle, ObjectPool<ParticleSystem> pool, float delay)
        {
            yield return GetCachedWait(delay);
            if (particle != null && particle.gameObject.activeSelf)
            {
                pool.Return(particle);
            }
        }

        private WaitForSeconds GetCachedWait(float seconds)
        {
            float key = Mathf.Round(seconds * 10f) / 10f;
            if (!_waitCache.TryGetValue(key, out var wait))
            {
                wait = new WaitForSeconds(key);
                _waitCache[key] = wait;
            }
            return wait;
        }
    }
}
