using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelToggle : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button toggleButton;

    private bool _isOpen;

    private void Start()
    {
        toggleButton.onClick.AddListener(Toggle);
        upgradePanel.SetActive(_isOpen);
    }

    private void OnDestroy()
    {
        toggleButton.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        _isOpen = !_isOpen;
        upgradePanel.SetActive(_isOpen);
    }
}
