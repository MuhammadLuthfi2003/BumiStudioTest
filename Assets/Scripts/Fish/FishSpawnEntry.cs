using System;
using UnityEngine;

/// <summary>
/// A single entry in a zone's fish pool: which fish, and how likely it is to spawn
/// *in this specific zone*. Lets the same FishData appear in multiple zones with
/// different rarity (e.g. a fish that's common in Coral Reef but rare in Deep Ocean).
/// </summary>
[Serializable]
public class FishSpawnEntry
{
    public FishData fish;

    [Tooltip("If left at 0, falls back to fish.rarityWeight. " +
             "Set explicitly here to override rarity per-zone.")]
    [Min(0f)]
    public float weightOverride = 0f;

    public float EffectiveWeight =>
        weightOverride > 0f ? weightOverride : (fish != null ? fish.rarityWeight : 0f);
}
