using TMPro;
using UnityEngine;

public class ResourceDisplay : MonoBehaviour
{
    [Header("UI Structure")]
    [SerializeField] private TextMeshProUGUI currentDepthDisplay;
    [SerializeField] private TextMeshProUGUI currentZoneDisplay;
    [SerializeField] private TextMeshProUGUI currentOxygenDisplay;
    [SerializeField] private TextMeshProUGUI currentCargoDisplay;
    [SerializeField] private TextMeshProUGUI currentHealthDisplay;

    private ResourceManager resourceManager;
    private DiveManager diveManager;
    private DepthTracker depthTracker;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;
        diveManager = GameManager.Instance != null ? GameManager.Instance.DiveManager : null;
        depthTracker = GameManager.Instance != null ? GameManager.Instance.DepthTracker : null;

        // subscribe to events
        if (resourceManager != null)
        {
            resourceManager.OnOxygenChanged += SetOxygen;
            resourceManager.OnCargoChanged += SetCargo;
            resourceManager.OnHealthChanged += SetHealth;
        }

        if (diveManager != null)
        {
            diveManager.OnZoneChanged += SetCurrentZone;
        }

        if (depthTracker != null)
        {
            depthTracker.OnDepthChanged += SetCurrentDepth;
        }

    }

    private void OnDestroy()
    {
        // unsubscribe to events
        // subscribe to events
        if (resourceManager != null)
        {
            resourceManager.OnOxygenChanged -= SetOxygen;
            resourceManager.OnCargoChanged -= SetCargo;
            resourceManager.OnHealthChanged -= SetHealth;
        }

        if (diveManager != null)
        {
            diveManager.OnZoneChanged -= SetCurrentZone;
        }

        if (depthTracker != null)
        {
            depthTracker.OnDepthChanged -= SetCurrentDepth;
        }
    }

    void SetOxygen(float currentOxygen, float maxOxygen)
    {
        if (currentOxygenDisplay == null) return;

        currentOxygenDisplay.text = "Oxygen : " + Mathf.RoundToInt(currentOxygen).ToString() + "/" + Mathf.RoundToInt(maxOxygen).ToString();
    }

    void SetCargo(int usedCargo, int maxCapacity)
    {
        if (currentCargoDisplay == null) return;

        currentCargoDisplay.text = "Cargo : " + usedCargo.ToString() + "/" + maxCapacity.ToString();
    }

    void SetCurrentZone(ZoneData _, ZoneData currentZone)
    {
        if (currentZoneDisplay == null) return;

        currentZoneDisplay.text = "Current Zone : " + currentZone.zoneName;
    }

    void SetCurrentDepth(float currentDepth)
    {
        if (currentDepthDisplay == null) return;

        currentDepthDisplay.text = "Current Depth : " + Mathf.RoundToInt(currentDepth).ToString() + "m";
    }

    void SetHealth(float currentHealth, float maxHealth)
    {
        if (currentHealthDisplay == null) return;
        currentHealthDisplay.text = "Health : " + Mathf.RoundToInt(currentHealth) + "/" + Mathf.RoundToInt(maxHealth);
    }
}
