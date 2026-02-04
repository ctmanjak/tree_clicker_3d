using Core;
using Outgame;
using UnityEngine;

namespace Ingame
{
    public class UpgradePanelUI : MonoBehaviour
    {
        [SerializeField] private UpgradeButtonUI _buttonPrefab;
        [SerializeField] private Transform _contentParent;
        [SerializeField] private UpgradeType[] _filterTypes;

        private UpgradeManager _upgradeManager;

        private void Start()
        {
            ServiceLocator.TryGet(out _upgradeManager);

            if (_upgradeManager == null)
            {
                Debug.LogError("UpgradeManager not found");
                return;
            }

            CreateButtons();
        }

        private void CreateButtons()
        {
            foreach (var filterType in _filterTypes)
            {
                foreach (var upgrade in _upgradeManager.GetUpgradesByType(filterType))
                {
                    var button = Instantiate(_buttonPrefab, _contentParent);
                    button.Init(upgrade, _upgradeManager);
                }
            }
        }
    }
}
