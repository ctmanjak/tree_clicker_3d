using System.Collections.Generic;
using Core;
using Outgame;
using UnityEngine;

namespace Ingame
{
    public class LumberjackSpawner : MonoBehaviour
    {
        private const float RandomSpawnDistance = 10f;

        [SerializeField] private GameObject _lumberjackPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private int _maxLumberjacks = 20;

        private HashSet<LumberjackController> _activeLumberjacks = new();

        public int ActiveCount => _activeLumberjacks.Count;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        public bool CanSpawn => _activeLumberjacks.Count < _maxLumberjacks;

        public LumberjackController SpawnLumberjack(CurrencyValue woodPerSecond)
        {
            if (!CanSpawn)
            {
                Debug.LogWarning("Maximum lumberjacks reached!");
                return null;
            }

            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : GetRandomSpawnPosition();
            GameObject obj = Instantiate(_lumberjackPrefab, spawnPos, Quaternion.identity);

            if (obj.TryGetComponent(out LumberjackController controller))
            {
                _activeLumberjacks.Add(controller);
                controller.SetStats(woodPerSecond, 1f);
            }

            if (obj.TryGetComponent(out SpawnEffect spawnEffect))
            {
                spawnEffect.PlaySpawnAnimation();
            }

            return controller;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = RandomSpawnDistance;
            return new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
        }

        public void RemoveLumberjack(LumberjackController lumberjack)
        {
            if (_activeLumberjacks.Remove(lumberjack))
            {
                Destroy(lumberjack.gameObject);
            }
        }

        public void ClearAll()
        {
            foreach (var lumberjack in _activeLumberjacks)
            {
                if (lumberjack != null)
                {
                    Destroy(lumberjack.gameObject);
                }
            }
            _activeLumberjacks.Clear();
        }

        public void UpdateAllLumberjackStats(CurrencyValue woodPerSecond)
        {
            foreach (var lumberjack in _activeLumberjacks)
            {
                if (lumberjack != null)
                {
                    lumberjack.SetStats(woodPerSecond, 1f);
                }
            }
        }
    }
}
