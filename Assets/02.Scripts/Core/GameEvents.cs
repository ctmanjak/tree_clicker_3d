using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public event Action<long> OnWoodChanged;
    public event Action<long> OnWoodAdded;
    public event Action<long> OnWoodPerClickChanged;

    public event Action<Vector3> OnClickPerformed;
    public event Action<GameObject> OnTreeClicked;
    public event Action OnTreeHit;

    public event Action<string, int> OnUpgradePurchased;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }
    
    public void RaiseWoodChanged(long amount) => OnWoodChanged?.Invoke(amount);
    public void RaiseWoodAdded(long amount) => OnWoodAdded?.Invoke(amount);
    public void RaiseWoodPerClickChanged(long amount) => OnWoodPerClickChanged?.Invoke(amount);
    public void RaiseClickPerformed(Vector3 pos) => OnClickPerformed?.Invoke(pos);
    public void RaiseTreeClicked(GameObject tree) => OnTreeClicked?.Invoke(tree);
    public void RaiseTreeHit() => OnTreeHit?.Invoke();
    public void RaiseUpgradePurchased(string id, int level) => OnUpgradePurchased?.Invoke(id, level);
}
