using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action<Vector3> OnClickPerformed;
    public event Action<GameObject> OnTreeClicked;

    private const float MaxRaycastDistance = 100f;
    private const string TreeTag = "Tree";

    [SerializeField] private LayerMask _clickableLayers;

    private Camera _mainCamera;

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


    private void Update()
    {
        if (TryGetClickPosition(out Vector2 screenPosition))
        {
            HandleClick(screenPosition);
        }
    }

    private bool TryGetClickPosition(out Vector2 position)
    {
        if (IsPointerOverUI())
        {
            position = default;
            return false;
        }

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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(touchId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _clickableLayers))
        {
            OnClickPerformed?.Invoke(hit.point);

            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick(hit.point, hit.normal);
            }

            if (hit.collider.CompareTag(TreeTag))
            {
                OnTreeClicked?.Invoke(hit.collider.gameObject);
            }
        }
    }
}
