using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public event Action<CurrencyType, CurrencyValue> OnCurrencyChanged;
    public event Action<CurrencyType, CurrencyValue> OnCurrencyAdded;
    public event Action<CurrencyType, CurrencyValue> OnPerClickChanged;

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

    public void RaiseCurrencyChanged(CurrencyType type, CurrencyValue amount) => OnCurrencyChanged?.Invoke(type, amount);
    public void RaiseCurrencyAdded(CurrencyType type, CurrencyValue amount) => OnCurrencyAdded?.Invoke(type, amount);
    public void RaisePerClickChanged(CurrencyType type, CurrencyValue amount) => OnPerClickChanged?.Invoke(type, amount);
    public void RaiseClickPerformed(Vector3 pos) => OnClickPerformed?.Invoke(pos);
    public void RaiseTreeClicked(GameObject tree) => OnTreeClicked?.Invoke(tree);
    public void RaiseTreeHit() => OnTreeHit?.Invoke();
    public void RaiseUpgradePurchased(string id, int level) => OnUpgradePurchased?.Invoke(id, level);
}
