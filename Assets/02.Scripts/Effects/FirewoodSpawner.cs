using UnityEngine;

public class FirewoodSpawner : MonoBehaviour
{
    [SerializeField] private Firewood _prefab;
    [SerializeField] private Transform _treeTransform;
    [SerializeField] private int _poolSize = 20;
    [SerializeField] private int _spawnCountMin = 3;
    [SerializeField] private int _spawnCountMax = 5;
    [SerializeField] private float _launchForce = 5f;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0, 1.5f, 0);

    private ObjectPool<Firewood> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Firewood>(_prefab, transform, _poolSize);
    }

    private void Start()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit += SpawnFirewood;
        }
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnTreeHit -= SpawnFirewood;
        }
    }

    private void SpawnFirewood()
    {
        if (_treeTransform == null) return;

        int count = Random.Range(_spawnCountMin, _spawnCountMax + 1);
        Vector3 spawnPosition = _treeTransform.position + _spawnOffset;

        for (int i = 0; i < count; i++)
        {
            var firewood = _pool.Get();
            firewood.Initialize(ReturnToPool);

            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;

            firewood.Launch(spawnPosition, randomDirection * _launchForce);
        }
    }

    private void ReturnToPool(Firewood firewood)
    {
        _pool.Return(firewood);
    }
}
