using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private Button _toggleButton;

    private bool _isOpen;

    private void Start()
    {
        _toggleButton.onClick.AddListener(Toggle);
        _upgradePanel.SetActive(_isOpen);
    }

    private void OnDestroy()
    {
        _toggleButton.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        _upgradePanel.SetActive(_isOpen);
    }
}
