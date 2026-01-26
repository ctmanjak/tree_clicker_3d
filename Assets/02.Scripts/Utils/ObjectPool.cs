using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 오브젝트 풀링 시스템
/// M2+에서 파티클, 벌목꾼 등에도 활용
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> pool = new();
    private readonly List<T> activeObjects = new();

    public ObjectPool(T prefab, Transform parent, int initialSize = 10)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }
    }

    private T CreateNew()
    {
        T obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        activeObjects.Remove(obj);
        pool.Enqueue(obj);
    }

    public void ReturnAll()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = activeObjects[i];
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
        activeObjects.Clear();
    }
}
