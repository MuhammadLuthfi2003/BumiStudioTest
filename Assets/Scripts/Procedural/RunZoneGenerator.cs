using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Builds the zone layout for a single dive by picking one ZoneData per depth tier.
/// This is a plain class (not a MonoBehaviour/SO) — instantiate it fresh each dive,
/// or hold one instance on your dive controller and call Generate() at dive start.
///
/// Depth tiers are resolved from ZoneData.minDepth rather than from the order the
/// ZoneTierPool[] array is passed in, per the GDD's "each stage has a minimum depth
/// requirement" rule. This means:
/// - You can pass tier pools in any order; they'll be sorted shallow to deep automatically.
/// - Warnings are logged if a tier pool's candidates disagree on minDepth, or if a picked
///   zone doesn't end up deeper than the tier before it (a sign of a data-setup mistake).
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
    /// Picks one ZoneData from each tier pool. Pools are sorted shallow to deep using
    /// their candidates' minDepth before picking, so the input array order doesn't matter.
    /// </summary>
    public ZoneData[] Generate(ZoneTierPool[] tierPools)
    {
        if (tierPools == null || tierPools.Length == 0)
        {
            Debug.LogWarning("RunZoneGenerator: no tier pools supplied.");
            RunZones = Array.Empty<ZoneData>();
            return RunZones;
        }

        // Order tiers by their candidates' minDepth rather than trusting array order.
        var orderedPools = tierPools
            .Where(pool => pool != null)
            .OrderBy(GetPoolDepth)
            .ToArray();

        var zones = new List<ZoneData>(orderedPools.Length);
        float previousDepth = float.NegativeInfinity;

        foreach (var pool in orderedPools)
        {
            ValidatePoolDepthConsistency(pool);

            ZoneData picked = pool.GetRandomCandidate(_rng);
            if (picked == null)
            {
                Debug.LogWarning($"RunZoneGenerator: tier pool '{pool.tierLabel}' produced no zone.");
                continue;
            }

            if (picked.minDepth <= previousDepth)
            {
                Debug.LogWarning(
                    $"RunZoneGenerator: zone '{picked.zoneName}' (minDepth {picked.minDepth}) is not " +
                    $"deeper than the previous tier (minDepth {previousDepth}). Check your ZoneData setup.");
            }

            previousDepth = picked.minDepth;
            zones.Add(picked);
        }

        RunZones = zones.ToArray();
        return RunZones;
    }

    /// <summary>
    /// Given the player's current depth, returns the zone that should currently be active —
    /// the deepest zone whose minDepth has been reached. Scans by minDepth directly, so it
    /// doesn't depend on RunZones being stored in sorted order.
    /// </summary>
    public ZoneData GetActiveZone(float currentDepth)
    {
        if (RunZones == null || RunZones.Length == 0) return null;

        ZoneData active = null;
        float bestDepth = float.NegativeInfinity;

        foreach (var zone in RunZones)
        {
            if (zone == null) continue;
            if (currentDepth >= zone.minDepth && zone.minDepth > bestDepth)
            {
                active = zone;
                bestDepth = zone.minDepth;
            }
        }

        return active;
    }

    /// <summary>
    /// Represents a tier pool's depth by its first valid candidate's minDepth.
    /// Pools with no valid candidates sort to the end.
    /// </summary>
    private static float GetPoolDepth(ZoneTierPool pool)
    {
        if (pool.candidates == null) return float.MaxValue;

        foreach (var candidate in pool.candidates)
        {
            if (candidate != null) return candidate.minDepth;
        }

        return float.MaxValue;
    }

    /// <summary>
    /// Candidates in the same tier pool compete for the same depth tier, so they should
    /// share the same minDepth. Warns (doesn't throw) if they don't, since a mismatch is
    /// almost always a data-entry mistake rather than intentional design.
    /// </summary>
    private static void ValidatePoolDepthConsistency(ZoneTierPool pool)
    {
        if (pool.candidates == null || pool.candidates.Length < 2) return;

        float? firstDepth = null;
        foreach (var candidate in pool.candidates)
        {
            if (candidate == null) continue;

            if (firstDepth == null)
            {
                firstDepth = candidate.minDepth;
            }
            else if (!Mathf.Approximately(firstDepth.Value, candidate.minDepth))
            {
                Debug.LogWarning(
                    $"RunZoneGenerator: tier pool '{pool.tierLabel}' has candidates with mismatched " +
                    $"minDepth ({firstDepth.Value} vs {candidate.minDepth}). Candidates in the same " +
                    "tier pool should share the same minDepth.");
            }
        }
    }
}