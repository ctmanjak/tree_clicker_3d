using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private FloatingText _prefab;
    [SerializeField] private Transform _treeTransform;
    [SerializeField] private int _poolSize = 20;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0, 2f, 0);

    private ObjectPool<FloatingText> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<FloatingText>(_prefab, transform, _poolSize);
    }

    private void Start()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodAdded += SpawnText;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnWoodAdded -= SpawnText;
        }
    }

    private void SpawnText(long amount)
    {
        if (_treeTransform == null) return;

        var text = _pool.Get();
        text.Initialize(ReturnToPool);

        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.2f, 0.2f),
            0
        );

        Vector3 spawnPosition = _treeTransform.position + _spawnOffset + randomOffset;

        // 카메라 방향으로 약간 앞으로 이동 (나무에 가려지지 않도록)
        if (Camera.main != null)
        {
            Vector3 toCamera = (Camera.main.transform.position - spawnPosition).normalized;
            spawnPosition += toCamera * 1f;
        }

        text.Show($"+{amount}", spawnPosition);
    }

    private void ReturnToPool(FloatingText text)
    {
        _pool.Return(text);
    }
}
