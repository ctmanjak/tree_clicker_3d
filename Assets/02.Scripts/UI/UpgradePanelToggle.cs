using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private Button _toggleButton;

    private static event Action<UpgradePanelToggle> OnPanelOpened;
    private static event Action OnAllPanelsClosed;

    private bool _isOpen;

    public static void CloseAllPanels()
    {
        OnAllPanelsClosed?.Invoke();
    }

    private void Start()
    {
        _toggleButton.onClick.AddListener(Toggle);
        _upgradePanel.SetActive(_isOpen);
    }

    private void OnEnable()
    {
        OnPanelOpened += HandleOtherPanelOpened;
        OnAllPanelsClosed += ClosePanel;
    }

    private void OnDisable()
    {
        OnPanelOpened -= HandleOtherPanelOpened;
        OnAllPanelsClosed -= ClosePanel;
    }

    private void ClosePanel()
    {
        _isOpen = false;
        _upgradePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        _toggleButton.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        _upgradePanel.SetActive(_isOpen);

        if (_isOpen)
        {
            OnPanelOpened?.Invoke(this);
        }
    }

    private void HandleOtherPanelOpened(UpgradePanelToggle sender)
    {
        if (sender == this) return;

        _isOpen = false;
        _upgradePanel.SetActive(false);
    }
}
