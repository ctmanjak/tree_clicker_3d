using UnityEngine;

public class TreeController : MonoBehaviour, IClickable
{
    private GameManager _gameManager;
    private GameEvents _gameEvents;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _gameEvents = GameEvents.Instance;
    }

    public void OnClick(Vector3 hitPoint)
    {
        _gameManager.AddWood(_gameManager.WoodPerClick);
        _gameEvents.RaiseTreeHit();
    }
}
