using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private const float MaxRaycastDistance = 100f;
    private const string TreeTag = "Tree";

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
        if (TryGetClickPosition(out Vector2 screenPosition))
        {
            HandleClick(screenPosition);
        }
    }

    private bool TryGetClickPosition(out Vector2 position)
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        position = default;
        return false;
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _clickableLayers))
        {
            _gameEvents.RaiseClickPerformed(hit.point);

            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick(hit.point);
            }

            if (hit.collider.CompareTag(TreeTag))
            {
                _gameEvents.RaiseTreeClicked(hit.collider.gameObject);
            }
        }
    }
}
