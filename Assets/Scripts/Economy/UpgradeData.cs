using UnityEngine;

/// <summary>
/// Which submarine stat this upgrade track affects.
/// </summary>
public enum UpgradeType
{
    Oxygen,
    Cargo,
}

/// <summary>
/// One entry in an upgrade track: the cost to reach this level, and the resulting
/// stat value once purchased.
/// </summary>
[System.Serializable]
public class UpgradeLevel
{
    [Tooltip("Money required to purchase this level.")]
    [Min(0)]
    public int cost = 100;

    [Tooltip("The resulting stat value once this level is reached " +
             "(e.g. new maxOxygen, or new cargoCapacity).")]
    [Min(0f)]
    public float value = 0f;
}

/// <summary>
/// Static data describing a single upgrade track (Oxygen or Cargo).
/// Levels are ordered index 0 = level 1, so levels.Length is the max level (e.g. 5).
/// One asset per upgradeable stat, shared/read by UpgradeManager.
/// </summary>
[CreateAssetMenu(fileName = "Upgrade_", menuName = "Stats/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string upgradeName = "New Upgrade";
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Which submarine stat this upgrade track applies to.")]
    public UpgradeType type;

    [Header("Levels")]
    [Tooltip("Index 0 = level 1. Array length defines the max level for this track (e.g. 5 entries = max level 5).")]
    public UpgradeLevel[] levels;

    /// <summary>The highest level this track can reach.</summary>
    public int MaxLevel => levels != null ? levels.Length : 0;

    /// <summary>
    /// Returns the level entry for the given level number (1-based), or null if out of range.
    /// </summary>
    public UpgradeLevel GetLevel(int levelNumber)
    {
        if (levels == null || levelNumber < 1 || levelNumber > levels.Length)
            return null;

        return levels[levelNumber - 1];
    }
}