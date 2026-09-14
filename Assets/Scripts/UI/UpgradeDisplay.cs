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

    [Header("Health Stat Refrence")]
    [SerializeField] TextMeshProUGUI currentHealthText;
    [SerializeField] TextMeshProUGUI nextHealthText;
    [SerializeField] TextMeshProUGUI healthUpgradePriceText;

    private UpgradeManager upgradeManager;
    private ResourceManager resourceManager;

    private int currentOxygenLevel;
    private int currentCargoLevel;
    private int currentHealthLevel;

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
        currentHealthLevel = upgradeManager.GetCurrentLevel(UpgradeType.Health);

        // for MVP purpose, since no player data persistence yet, get current stat from resource manager
        // TODO: READ PlayerPrefs for current level

        currentOxygenText.text = "Current : " + resourceManager.MaxOxygen.ToString();
        currentCargoText.text = "Current : " + resourceManager.CargoCapacity.ToString();
        currentHealthText.text = "Current : " + resourceManager.MaxHealth.ToString();

        // get next level stat, and display the values
        UpgradeLevel oxygenStat = upgradeManager.GetNextLevelStat(UpgradeType.Oxygen);
        UpgradeLevel cargoStat = upgradeManager.GetNextLevelStat(UpgradeType.Cargo);
        UpgradeLevel healthStat = upgradeManager.GetNextLevelStat(UpgradeType.Health);

        nextOxygenText.text = "Next : " + oxygenStat.value.ToString();
        nextCargoText.text = "Next : " + cargoStat.value.ToString();
        nextHealthText.text = "Next : " + healthStat.value.ToString();

        oxygenUpgradePriceText.text = "Upgrade ($" + oxygenStat.cost.ToString() +")";
        cargoUpgradePriceText.text = "Upgrade ($" + cargoStat.cost.ToString() + ")";
        healthUpgradePriceText.text = "Upgrade ($" + healthStat.cost.ToString() + ")";
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

            case UpgradeType.Health:
                currentHealthLevel = newCurrentLevel;
                currentHealthText.text = "Current : " + currentStat.value;

                if (upgradeManager.IsMaxLevel(newData))
                {
                    nextHealthText.text = "MAX";
                    healthUpgradePriceText.text = "MAX";
                }
                else
                {
                    UpgradeLevel nextStat = upgradeManager.GetNextLevelStat(upgradeType);
                    nextHealthText.text = "Next : " + nextStat.value.ToString();
                    healthUpgradePriceText.text = "Upgrade ($" + nextStat.cost.ToString() + ")";
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

    public void TryUpgradeHealth()
    {
        upgradeManager.TryPurchaseUpgrade(UpgradeType.Health);
    }
}
