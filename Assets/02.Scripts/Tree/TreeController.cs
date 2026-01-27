using UnityEngine;

public class TreeController : MonoBehaviour, IClickable
{
    private GameManager _gameManager;
    private GameEvents _gameEvents;
    private TreeShake _treeShake;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _gameEvents = GameEvents.Instance;
        _treeShake = GetComponent<TreeShake>();
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
    }
}
