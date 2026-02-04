using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ingame
{
    public class UpgradePanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private Button _toggleButton;

        private static event Action<UpgradePanelToggle> OnPanelOpened;
        private static event Action OnAllPanelsClosed;

        private PanelTransition _panelTransition;
        private bool _isOpen;

        public static void CloseAllPanels()
        {
            OnAllPanelsClosed?.Invoke();
        }

        private void Awake()
        {
            _panelTransition = _upgradePanel.GetComponent<PanelTransition>();
        }

        private void Start()
        {
            _toggleButton.onClick.AddListener(Toggle);
            _upgradePanel.SetActive(false);
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

        private void OnDestroy()
        {
            _toggleButton.onClick.RemoveListener(Toggle);
        }

        private void Toggle()
        {
            if (_isOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OpenPanel()
        {
            _isOpen = true;

            if (_panelTransition != null)
            {
                _panelTransition.Open();
            }
            else
            {
                _upgradePanel.SetActive(true);
            }

            OnPanelOpened?.Invoke(this);
        }

        private void ClosePanel()
        {
            if (!_isOpen) return;

            _isOpen = false;

            if (_panelTransition != null)
            {
                _panelTransition.Close();
            }
            else
            {
                _upgradePanel.SetActive(false);
            }
        }

        private void HandleOtherPanelOpened(UpgradePanelToggle sender)
        {
            if (sender == this) return;
            ClosePanel();
        }
    }
}
