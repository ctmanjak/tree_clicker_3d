using System;
using UnityEngine;

public class TreeController : MonoBehaviour, IClickable
{
    public event Action OnTreeHit;

    private CurrencyManager _currencyManager;
    private UpgradeManager _upgradeManager;
    private TreeShake _treeShake;
    private AudioManager _audioManager;

    public Vector3 Position => transform.position;

    private void Awake()
    {
        ServiceLocator.Register(this);
        _treeShake = GetComponent<TreeShake>();
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _currencyManager);
        ServiceLocator.TryGet(out _upgradeManager);
        ServiceLocator.TryGet(out _audioManager);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public void OnClick(Vector3 hitPoint, Vector3 hitNormal)
    {
        Hit(_upgradeManager.GetWoodPerClick());
    }

    public void Hit(CurrencyValue woodAmount, Vector3? attackerPosition = null)
    {
        _currencyManager.Add(CurrencyType.Wood, woodAmount);
        OnTreeHit?.Invoke();
        _treeShake?.Shake(attackerPosition);
        _audioManager?.PlaySFX(SFXType.HitWood);
    }
}
