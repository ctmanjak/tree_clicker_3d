using Core;
using Outgame;
using UnityEngine;

namespace Ingame
{
    public class FloatingTextSpawner : MonoBehaviour
    {
        [SerializeField] private FloatingText _prefab;
        [SerializeField] private Transform _treeTransform;
        [SerializeField] private int _poolSize = 20;
        [SerializeField] private Vector3 _spawnOffset = new Vector3(0, 2f, 0);
        [SerializeField] private Vector2 _randomRangeX = new Vector2(-0.5f, 0.5f);
        [SerializeField] private Vector2 _randomRangeY = new Vector2(-0.2f, 0.2f);
        [SerializeField] private float _cameraOffsetDistance = 1f;

        private ObjectPool<FloatingText> _pool;
        private CurrencyManager _currencyManager;

        private void Awake()
        {
            _pool = new ObjectPool<FloatingText>(_prefab, transform, _poolSize);
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out _currencyManager))
            {
                _currencyManager.OnCurrencyAdded += OnCurrencyAdded;
            }
        }

        private void OnDestroy()
        {
            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyAdded -= OnCurrencyAdded;
            }
        }

        private void OnCurrencyAdded(CurrencyType type, CurrencyValue amount)
        {
            if (type != CurrencyType.Wood) return;
            SpawnText(amount);
        }

        private void SpawnText(CurrencyValue amount)
        {
            if (_treeTransform == null) return;

            var text = _pool.Get();
            text.Initialize(ReturnToPool);

            Vector3 randomOffset = new Vector3(
                Random.Range(_randomRangeX.x, _randomRangeX.y),
                Random.Range(_randomRangeY.x, _randomRangeY.y),
                0
            );

            Vector3 spawnPosition = _treeTransform.position + _spawnOffset + randomOffset;

            if (Camera.main != null)
            {
                Vector3 toCamera = (Camera.main.transform.position - spawnPosition).normalized;
                spawnPosition += toCamera * _cameraOffsetDistance;
            }

            text.Show($"+{amount}", spawnPosition);
        }

        private void ReturnToPool(FloatingText text)
        {
            _pool.Return(text);
        }
    }
}
