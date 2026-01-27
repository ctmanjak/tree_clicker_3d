using UnityEngine;

public class UpgradePanelUI : MonoBehaviour
{
    [SerializeField] private UpgradeButtonUI buttonPrefab;
    [SerializeField] private Transform contentParent;

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
        foreach (var upgrade in _upgradeManager.Upgrades)
        {
            var button = Instantiate(buttonPrefab, contentParent);
            button.Init(upgrade, _upgradeManager);
        }
    }
}
