using System;
using UnityEngine;

/// <summary>
/// A single entry in a zone's hazard pool: which hazard, and how likely it is to spawn
/// in this specific zone. Mirrors FishSpawnEntry.
/// </summary>
[Serializable]
public class HazardSpawnEntry
{
    public HazardData hazard;

    [Tooltip("If left at 0, falls back to hazard.rarityWeight. " +
             "Set explicitly here to override rarity per-zone.")]
    [Min(0f)]
    public float weightOverride = 0f;

    public float EffectiveWeight =>
        weightOverride > 0f ? weightOverride : (hazard != null ? hazard.rarityWeight : 0f);
}