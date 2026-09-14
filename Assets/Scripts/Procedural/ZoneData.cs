using UnityEngine;

/// <summary>
/// A single zone/stage (e.g. "Coral Reef", "Sunken Ship").
/// Multiple ZoneData assets compete inside the same ZoneTierPool for a given depth tier,
/// so which theme appears at that depth changes from run to run.
/// </summary>
[CreateAssetMenu(fileName = "Zone_", menuName = "Dive/Zone Data")]
public class ZoneData : ScriptableObject
{
    [Header("Identity")]
    public string zoneName = "New Zone";
    [TextArea(2, 4)]
    public string flavorText;

    [Header("Depth")]
    [Tooltip("Minimum depth (in meters) the player must reach for this zone to become active.")]
    [Min(0)]
    public float minDepth = 20f;

    [Header("Fish Pool")]
    [Tooltip("Every fish that can spawn in this zone, with its spawn weight.")]
    public FishSpawnEntry[] fishPool;

    [Header("Hazard Pool")]
    [Tooltip("Every hazard that can spawn in this zone, with its spawn weight.")]
    public HazardSpawnEntry[] hazardPool;

    [Header("Presentation")]
    public Color ambientTint = Color.white;
    public GameObject environmentPrefab; // background/terrain dressing for this zone

    /// <summary>
    /// Picks a random fish from this zone's pool, weighted by rarity.
    /// Returns null if the pool is empty or all weights are zero.
    /// </summary>
    public FishData GetRandomFish(System.Random rng = null)
    {
        if (fishPool == null || fishPool.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var entry in fishPool)
            totalWeight += entry.EffectiveWeight;

        if (totalWeight <= 0f) return null;

        double roll = (rng != null ? rng.NextDouble() : UnityEngine.Random.value) * totalWeight;

        float cumulative = 0f;
        foreach (var entry in fishPool)
        {
            cumulative += entry.EffectiveWeight;
            if (roll <= cumulative)
                return entry.fish;
        }

        // Fallback in case of floating point rounding at the tail end.
        return fishPool[fishPool.Length - 1].fish;
    }

    /// <summary>
    /// Picks a random hazard from this zone's pool, weighted by rarity.
    /// Returns null if the pool is empty or all weights are zero.
    /// </summary>
    public HazardData GetRandomHazard(System.Random rng = null)
    {
        if (hazardPool == null || hazardPool.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var entry in hazardPool)
            totalWeight += entry.EffectiveWeight;

        if (totalWeight <= 0f) return null;

        double roll = (rng != null ? rng.NextDouble() : UnityEngine.Random.value) * totalWeight;

        float cumulative = 0f;
        foreach (var entry in hazardPool)
        {
            cumulative += entry.EffectiveWeight;
            if (roll <= cumulative)
                return entry.hazard;
        }

        return hazardPool[hazardPool.Length - 1].hazard;
    }
}
