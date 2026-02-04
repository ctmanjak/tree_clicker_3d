using UnityEngine;
using UnityEngine.UI;

namespace Ingame
{
    public class CloseAllPanelsButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            UpgradePanelToggle.CloseAllPanels();
        }
    }
}
