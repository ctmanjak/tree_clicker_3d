using UnityEngine;
using UnityEngine.UI;

public class CloseAllPanelsButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        UpgradePanelToggle.CloseAllPanels();
    }
}
