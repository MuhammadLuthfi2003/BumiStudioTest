using System;
using System.Collections.Generic;

/// <summary>
/// Plain data container for everything that persists between play sessions.
/// Kept free of MonoBehaviour/ScriptableObject references so it can be serialized
/// straight to JSON via JsonUtility (JsonUtility can't serialize object references
/// like UpgradeData assets, so upgrade progress is keyed by UpgradeType instead of
/// by asset reference).
/// </summary>
[Serializable]
public class SaveData
{
    public int currentMoney = 0;

    public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();

    /// <summary>Current saved level for a track, or 0 if never purchased.</summary>
    public int GetUpgradeLevel(UpgradeType type)
    {
        foreach (var entry in upgradeLevels)
        {
            if (entry.type == type)
                return entry.level;
        }
        return 0;
    }

    /// <summary>Adds or updates the saved level for a track.</summary>
    public void SetUpgradeLevel(UpgradeType type, int level)
    {
        foreach (var entry in upgradeLevels)
        {
            if (entry.type == type)
            {
                entry.level = level;
                return;
            }
        }

        upgradeLevels.Add(new UpgradeLevelEntry { type = type, level = level });
    }
}

/// <summary>
/// One (track, level) pair. JsonUtility needs a concrete serializable type for
/// list elements, so this stands in for what would otherwise be a Dictionary entry.
/// </summary>
[Serializable]
public class UpgradeLevelEntry
{
    public UpgradeType type;
    public int level;
}