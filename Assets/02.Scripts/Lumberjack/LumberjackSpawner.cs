using UnityEngine;
using System.Collections.Generic;

public class LumberjackSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _lumberjackPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private int _maxLumberjacks = 20;

    private List<LumberjackController> _activeLumberjacks = new();

    public int ActiveCount => _activeLumberjacks.Count;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }
    public bool CanSpawn => _activeLumberjacks.Count < _maxLumberjacks;

    public LumberjackController SpawnLumberjack()
    {
        if (!CanSpawn)
        {
            Debug.LogWarning("Maximum lumberjacks reached!");
            return null;
        }

        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : GetRandomSpawnPosition();
        GameObject obj = Instantiate(_lumberjackPrefab, spawnPos, Quaternion.identity);

        var controller = obj.GetComponent<LumberjackController>();
        if (controller != null)
        {
            _activeLumberjacks.Add(controller);
        }

        return controller;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = 10f;
        return new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
    }

    public void RemoveLumberjack(LumberjackController lumberjack)
    {
        if (!_activeLumberjacks.Contains(lumberjack)) return;

        _activeLumberjacks.Remove(lumberjack);
        Destroy(lumberjack.gameObject);
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
}
