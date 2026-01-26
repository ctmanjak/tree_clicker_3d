using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _clickableLayers;

    private Camera _mainCamera;
    private GameEvents _gameEvents;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera not found! InputHandler requires a camera tagged 'MainCamera'.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        _gameEvents = GameEvents.Instance;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick(Mouse.current.position.ReadValue());
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandleClick(Touchscreen.current.primaryTouch.position.ReadValue());
        }
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _clickableLayers))
        {
            _gameEvents.RaiseClickPerformed(hit.point);

            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick(hit.point);
            }

            if (hit.collider.CompareTag("Tree"))
            {
                _gameEvents.RaiseTreeClicked(hit.collider.gameObject);
            }
        }
    }
}
