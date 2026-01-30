using UnityEngine;

public class TreeController : MonoBehaviour, IClickable
{
    private CurrencyManager _currencyManager;
    private GameEvents _gameEvents;
    private TreeShake _treeShake;
    private AudioManager _audioManager;

    public Vector3 Position => transform.position;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _gameEvents);
        ServiceLocator.TryGet(out _currencyManager);
        _treeShake = GetComponent<TreeShake>();
        ServiceLocator.TryGet(out _audioManager);
    }

    public void OnClick(Vector3 hitPoint, Vector3 hitNormal)
    {
        Hit(_currencyManager.GetPerClick(CurrencyType.Wood));
    }

    public void Hit(CurrencyValue woodAmount, Vector3? attackerPosition = null)
    {
        _currencyManager.Add(CurrencyType.Wood, woodAmount);
        _gameEvents.RaiseTreeHit();
        _treeShake?.Shake(attackerPosition);
        _audioManager?.PlaySFX(SFXType.HitWood);
    }
}
