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
        _gameManager.AddWood(_gameManager.WoodPerClick);
        _gameEvents.RaiseTreeHit();

        if (_treeShake != null)
        {
            _treeShake.Shake();
        }
    }
}
