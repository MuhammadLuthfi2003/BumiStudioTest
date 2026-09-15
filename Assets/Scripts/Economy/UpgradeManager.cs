using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the pre-dive upgrade shop
/// Upgrade levels are permanent progression: once purchased, a level is never lost,
/// not even on a failed dive (only cargo *contents* are lost) — and, with SaveManager
/// wired in, not even between play sessions.
///
/// Each UpgradeData track (Oxygen, Cargo, ...) is tracked independently and starts
/// at level 0 (base stats, as configured on ResourceManager itself) unless a save file
/// says otherwise. Purchasing a level spends money via CurrencyManager, then pushes the
/// new stat value into ResourceManager so it's in effect for the next dive, and writes
/// the new level to SaveManager so it survives a restart.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Tracks")]
    [Tooltip("Every upgrade track available in the shop (e.g. Oxygen, Cargo).")]
    [SerializeField] private UpgradeData[] upgrades;


    private CurrencyManager currencyManager;
    private ResourceManager resourceManager;

    /// <summary>Fired after a successful purchase. Params: the upgrade track, its new current level.</summary>
    public event Action<UpgradeData, int> OnUpgradePurchased;

    // current level per track; 0 = not yet purchased (base stats).
    private readonly Dictionary<UpgradeData, int> _levels = new Dictionary<UpgradeData, int>();

    private void Start()
    {
        currencyManager = GameManager.Instance != null ? GameManager.Instance.CurrencyManager : null;
        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        if (upgrades != null)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade == null) continue;

                // Pull the persisted level for this track (0 if never purchased / no save yet).
                // SaveManager's own Awake() has already loaded the file by the time any
                // Start() executes, so this is safe regardless of component order.
                int savedLevel = SaveManager.Instance != null
                    ? SaveManager.Instance.Data.GetUpgradeLevel(upgrade.type)
                    : 0;

                _levels[upgrade] = savedLevel;

                // Re-apply the saved level's stat onto ResourceManager so the sub actually
                // has the upgraded oxygen/cargo capacity before the player's first dive,
                // rather than only remembering the level number.
                if (savedLevel > 0)
                {
                    UpgradeLevel level = upgrade.GetLevel(savedLevel);
                    if (level != null)
                    {
                        ApplyUpgrade(upgrade, level);
                        // fires to the UI to load the data
                        OnUpgradePurchased?.Invoke(upgrade, savedLevel);
                    }
                }
            }
        }
    }

    /// <summary>Current purchased level for this track (0 = base stats, not yet upgraded).</summary>
    public int GetCurrentLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return 0;
        return _levels.TryGetValue(upgrade, out int level) ? level : 0;
    }

    /// <summary>Overload function of GetCurrentLevel</summary>
    public int GetCurrentLevel(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgradeData(type);
        if (upgrade == null) return 0;
        return _levels.TryGetValue(upgrade, out int level) ? level : 0;
    }

    /// <summary>Whether this track has reached its max level (levels.Length).</summary>
    public bool IsMaxLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return true;
        return GetCurrentLevel(upgrade) >= upgrade.MaxLevel;
    }
    /// <summary>Overload function of IsMaxLevel with UpgradeType Param</summary>
    public bool IsMaxLevel(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgradeData(type);
        if (upgrade == null) return true;
        return GetCurrentLevel(upgrade) >= upgrade.MaxLevel;
    }

    /// <summary>
    /// Cost to purchase the NEXT level of this track, or -1 if already at max level.
    /// </summary>
    public int GetNextLevelCost(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgradeData(type);
        if (upgrade == null || IsMaxLevel(upgrade))
            return -1;

        UpgradeLevel next = upgrade.GetLevel(GetCurrentLevel(upgrade) + 1);
        return next != null ? next.cost : -1;
    }

    /// <summary>
    /// Get UpgradeLevel next stat and cost, by returning the UpgradeLevel from the upgrades[] array
    /// </summary>
    public UpgradeLevel GetNextLevelStat(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgradeData(type);
        if (upgrade == null || IsMaxLevel(upgrade))
            return null;

        UpgradeLevel next = upgrade.GetLevel(GetCurrentLevel(upgrade) + 1);
        return next;
    }

    /// <summary>
    /// Attempts to purchase the next level of the given upgrade track.
    /// Fails if already at max level, or the player can't afford it.
    /// Returns true on success, after applying the new stat to ResourceManager and
    /// persisting the new level via SaveManager.
    /// </summary>
    public bool TryPurchaseUpgrade(UpgradeType type)
    {
        UpgradeData upgrade = GetUpgradeData(type);
        if (upgrade == null)
            return false;

        if (IsMaxLevel(upgrade))
        {
            Debug.LogWarning($"UpgradeManager: '{upgrade.upgradeName}' is already at max level.");
            return false;
        }

        int nextLevelNumber = GetCurrentLevel(upgrade) + 1;
        UpgradeLevel nextLevel = upgrade.GetLevel(nextLevelNumber);

        if (nextLevel == null)
            return false;

        if (currencyManager == null || !currencyManager.TrySpend(nextLevel.cost))
        {
            Debug.LogWarning("Insufficient Money");
            return false;
        }
        _levels[upgrade] = nextLevelNumber;
        ApplyUpgrade(upgrade, nextLevel);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Data.SetUpgradeLevel(upgrade.type, nextLevelNumber);
            SaveManager.Instance.Save();
        }

        OnUpgradePurchased?.Invoke(upgrade, nextLevelNumber);
        return true;
    }

    /// <summary>Pushes the purchased level's value into ResourceManager's matching stat.</summary>
    private void ApplyUpgrade(UpgradeData upgrade, UpgradeLevel level)
    {
        if (resourceManager == null)
        {
            Debug.LogWarning("UpgradeManager: no ResourceManager available to apply the upgrade to.");
            return;
        }

        switch (upgrade.type)
        {
            case UpgradeType.Oxygen:
                resourceManager.SetMaxOxygen(level.value);
                break;

            case UpgradeType.Cargo:
                resourceManager.SetCargoCapacity(Mathf.RoundToInt(level.value));
                break;

            case UpgradeType.Health:
                resourceManager.SetMaxHealth(level.value);
                break;
        }
    }

    /// <summary> Helper function to get upgrade data</summary>
    private UpgradeData GetUpgradeData(UpgradeType type)
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (upgrades[i].type == type)
            {
                return upgrades[i];
            }
        }

        Debug.LogWarning("Upgrade Data Not Found");
        return null;
    }
}