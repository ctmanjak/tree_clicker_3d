using UnityEngine;

public class TreeController : MonoBehaviour, IClickable
{
    private GameManager _gameManager;
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
        ServiceLocator.TryGet(out _gameManager);
        ServiceLocator.TryGet(out _gameEvents);
        _treeShake = GetComponent<TreeShake>();
        ServiceLocator.TryGet(out _audioManager);
    }

    public void OnClick(Vector3 hitPoint, Vector3 hitNormal)
    {
        Hit(_gameManager.WoodPerClick);
    }

    public void Hit(long woodAmount, Vector3? attackerPosition = null)
    {
        _gameManager.AddWood(woodAmount);
        _gameEvents.RaiseTreeHit();
        _treeShake?.Shake(attackerPosition);
        _audioManager?.PlaySFX(SFXType.HitWood);
    }
}
