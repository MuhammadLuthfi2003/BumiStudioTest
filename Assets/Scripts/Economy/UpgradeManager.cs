using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the pre-dive upgrade shop
/// Upgrade levels are permanent progression: once purchased, a level is never lost,
/// not even on a failed dive (only cargo *contents* are lost).
///
/// Each UpgradeData track (Oxygen, Cargo, ...) is tracked independently and starts
/// at level 0 (base stats, as configured on ResourceManager itself). Purchasing a
/// level spends money via CurrencyManager, then pushes the new stat value into
/// ResourceManager so it's in effect for the next dive.
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

    private void Awake()
    {
        currencyManager = GameManager.Instance != null ? GameManager.Instance.CurrencyManager : null;
        resourceManager = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        if (upgrades != null)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade != null && !_levels.ContainsKey(upgrade))
                    _levels[upgrade] = 0;
            }
        }
    }

    /// <summary>Current purchased level for this track (0 = base stats, not yet upgraded).</summary>
    public int GetCurrentLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return 0;
        return _levels.TryGetValue(upgrade, out int level) ? level : 0;
    }

    /// <summary>Whether this track has reached its max level (levels.Length).</summary>
    public bool IsMaxLevel(UpgradeData upgrade)
    {
        if (upgrade == null) return true;
        return GetCurrentLevel(upgrade) >= upgrade.MaxLevel;
    }

    /// <summary>
    /// Cost to purchase the NEXT level of this track, or -1 if already at max level.
    /// </summary>
    public int GetNextLevelCost(UpgradeData upgrade)
    {
        if (upgrade == null || IsMaxLevel(upgrade))
            return -1;

        UpgradeLevel next = upgrade.GetLevel(GetCurrentLevel(upgrade) + 1);
        return next != null ? next.cost : -1;
    }

    /// <summary>
    /// Attempts to purchase the next level of the given upgrade track.
    /// Fails if already at max level, or the player can't afford it.
    /// Returns true on success, after applying the new stat to ResourceManager.
    /// </summary>
    public bool TryPurchaseUpgrade(UpgradeData upgrade)
    {
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
            return false;

        _levels[upgrade] = nextLevelNumber;
        ApplyUpgrade(upgrade, nextLevel);

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
        }
    }
}