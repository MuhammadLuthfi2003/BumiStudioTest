using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the zone layout for a single dive by picking one ZoneData per depth tier.
/// This is a plain class (not a MonoBehaviour/SO) — instantiate it fresh each dive,
/// or hold one instance on your dive controller and call Generate() at dive start.
/// </summary>
public class RunZoneGenerator
{
    /// <summary>
    /// The resolved zones for this run, ordered shallow to deep.
    /// </summary>
    public ZoneData[] RunZones { get; private set; }

    private readonly System.Random _rng;

    public RunZoneGenerator(int? seed = null)
    {
        _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    /// <summary>
    /// Picks one ZoneData from each tier pool, in the order the pools are given.
    /// Pools should be ordered shallow to deep (e.g. [Tier20m, Tier40m, Tier60m, Tier80m]).
    /// </summary>
    public ZoneData[] Generate(ZoneTierPool[] orderedTierPools)
    {
        var zones = new List<ZoneData>(orderedTierPools.Length);

        foreach (var pool in orderedTierPools)
        {
            ZoneData picked = pool.GetRandomCandidate(_rng);
            if (picked == null)
                Debug.LogWarning($"RunZoneGenerator: tier pool '{pool.tierLabel}' produced no zone.");
            zones.Add(picked);
        }

        RunZones = zones.ToArray();
        return RunZones;
    }

    /// <summary>
    /// Given the player's current depth, returns the zone that should currently be active.
    /// Assumes RunZones is sorted shallow to deep and each ZoneData.minDepth is ascending.
    /// </summary>
    public ZoneData GetActiveZone(float currentDepth)
    {
        if (RunZones == null || RunZones.Length == 0) return null;

        ZoneData active = null;
        foreach (var zone in RunZones)
        {
            if (zone == null) continue;
            if (currentDepth >= zone.minDepth)
                active = zone; // keep taking the deepest tier we've reached
            else
                break;
        }
        return active;
    }
}
