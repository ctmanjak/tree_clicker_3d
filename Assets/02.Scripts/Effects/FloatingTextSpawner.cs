using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    [SerializeField] private FloatingText _prefab;
    [SerializeField] private int _poolSize = 20;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0, 2f, 0);

    private ObjectPool<FloatingText> _pool;
    private Transform _treeTransform;

    private void Awake()
    {
        _pool = new ObjectPool<FloatingText>(_prefab, transform, _poolSize);
    }

    private void Start()
    {
        _treeTransform = GameObject.FindWithTag("Tree")?.transform;

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

        text.Show($"+{amount}", _treeTransform.position + _spawnOffset + randomOffset);
    }

    private void ReturnToPool(FloatingText text)
    {
        _pool.Return(text);
    }
}
