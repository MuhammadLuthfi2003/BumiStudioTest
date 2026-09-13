using TMPro;
using UnityEngine;

public class UpgradeDisplay : MonoBehaviour
{
    [Header("Oxygen Stat Refrence")]
    [SerializeField] TextMeshProUGUI currentOxygenText;
    [SerializeField] TextMeshProUGUI nextOxygenText;
    [SerializeField] TextMeshProUGUI oxygenUpgradePriceText;

    [Header("Cargo Stat Refrence")]
    [SerializeField] TextMeshProUGUI currentCargoText;
    [SerializeField] TextMeshProUGUI nextCargoText;
    [SerializeField] TextMeshProUGUI cargoUpgradePriceText;

    private UpgradeManager upgradeManager;
    private ResourceManager resourceManager;

    private int currentOxygenLevel;
    private int currentCargoLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        upgradeManager = GameManager.Instance != null ? GameManager.Instance.UpgradeManager : null;
        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        if (upgradeManager == null || resourceManager == null) return;

        // connect to upgrademanager event
        upgradeManager.OnUpgradePurchased += UpdateDisplay;

        // DISPLAY DATA
        currentOxygenLevel = upgradeManager.GetCurrentLevel(UpgradeType.Oxygen);
        currentCargoLevel = upgradeManager.GetCurrentLevel(UpgradeType.Cargo);

        // for MVP purpose, since no player data persistence yet, get current stat from resource manager
        // TODO: READ PlayerPrefs for current level

        currentOxygenText.text = "Current : " + resourceManager.MaxOxygen.ToString();
        currentCargoText.text = "Current : " + resourceManager.CargoCapacity.ToString();

        // get next level stat, and display the values
        UpgradeLevel oxygenStat = upgradeManager.GetNextLevelStat(UpgradeType.Oxygen);
        UpgradeLevel cargoStat = upgradeManager.GetNextLevelStat(UpgradeType.Cargo);

        nextOxygenText.text = "Next : " + oxygenStat.value.ToString();
        nextCargoText.text = "Next : " + cargoStat.value.ToString();

        oxygenUpgradePriceText.text = "Upgrade ($" + oxygenStat.cost.ToString() +")";
        cargoUpgradePriceText.text = "Upgrade ($" + cargoStat.cost.ToString() + ")";
    }

    private void OnDisable()
    {
        if (upgradeManager != null)
        {
            upgradeManager.OnUpgradePurchased -= UpdateDisplay;
        }
    }

    private void UpdateDisplay(UpgradeData newData, int newCurrentLevel)
    {
        // check for type
        UpgradeType upgradeType = newData.type;
        // get current stat value
        UpgradeLevel currentStat = newData.GetLevel(newCurrentLevel);

        switch (upgradeType)
        {
            case UpgradeType.Cargo:
                currentCargoLevel = newCurrentLevel;
                currentCargoText.text = "Current : " + currentStat.value;

                if (upgradeManager.IsMaxLevel(newData))
                {
                    nextCargoText.text = "MAX";
                    cargoUpgradePriceText.text = "MAX";
                }
                else
                {
                    UpgradeLevel nextStat = upgradeManager.GetNextLevelStat(upgradeType);
                    nextCargoText.text = "Next : " + nextStat.value.ToString();
                    cargoUpgradePriceText.text = "Upgrade ($" + nextStat.cost.ToString() + ")";
                }

                break;
            case UpgradeType.Oxygen:
                currentOxygenLevel = newCurrentLevel;
                currentOxygenText.text = "Current : " + currentStat.value;

                if (upgradeManager.IsMaxLevel(newData))
                {
                    nextOxygenText.text = "MAX";
                    oxygenUpgradePriceText.text = "MAX";
                }
                else
                {
                    UpgradeLevel nextStat = upgradeManager.GetNextLevelStat(upgradeType);
                    nextOxygenText.text = "Next : " + nextStat.value.ToString();
                    oxygenUpgradePriceText.text = "Upgrade ($" + nextStat.cost.ToString() + ")";
                }
                break;
        }
    }

    // wrapper function for each stat since OnClick wont accept UpgradeType parameter
    public void TryUpgradeOxygen()
    {
        upgradeManager.TryPurchaseUpgrade(UpgradeType.Oxygen);
    }

    public void TryUpgradeCargo()
    {
        upgradeManager.TryPurchaseUpgrade(UpgradeType.Cargo);
    }
}
