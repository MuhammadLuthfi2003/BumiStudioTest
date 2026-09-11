using System;
using UnityEngine;

/// <summary>
/// Orchestrates a single dive per the GDD's core loop:
/// Prepare > Dive > Explore > Catch Fish > Manage Resources > Continue or Return.
///
/// This script owns run generation and the active-zone lookup. Depth itself is
/// tracked by a separate DepthTracker component, DiveManager just listens to it
/// and figures out which zone that depth falls into.
/// </summary>
public class DiveManager : MonoBehaviour
{
    [Header("Zone Setup")]
    [Tooltip("One pool per depth tier (e.g. 20m/40m/60m/80m). Order in this array doesn't " +
             "matter — RunZoneGenerator sorts pools by each candidate ZoneData's minDepth.")]
    [SerializeField] private ZoneTierPool[] tierPools;

    [Header("Depth")]
    [Tooltip("Reports the submarine's current depth. DiveManager subscribes to its OnDepthChanged event.")]
    [SerializeField] private DepthTracker depthTracker;

    [Header("Debug")]
    [Tooltip("Set for reproducible testing. Leave null for a random run each time.")]
    [SerializeField] private int? debugSeed = null;

    /// <summary>The zones generated for the current run, shallow to deep.</summary>
    public ZoneData[] RunZones { get; private set; }

    /// <summary>The zone the submarine is currently inside, based on current depth.</summary>
    public ZoneData CurrentZone { get; private set; }

    /// <summary>Current depth in meters (0 = surface), mirrored from DepthTracker for convenience.</summary>
    public float CurrentDepth { get; private set; }

    /// <summary>Fired once, right after a new run's zones are generated.</summary>
    public event Action<ZoneData[]> OnRunGenerated;

    /// <summary>Fired whenever the active zone changes (previousZone, newZone).</summary>
    public event Action<ZoneData, ZoneData> OnZoneChanged;

    private RunZoneGenerator _generator;

    private void OnEnable()
    {
        if (depthTracker != null)
            depthTracker.OnDepthChanged += HandleDepthChanged;
    }

    private void OnDisable()
    {
        if (depthTracker != null)
            depthTracker.OnDepthChanged -= HandleDepthChanged;
    }

    /// <summary>
    /// Call this at the start of a dive, after pre-dive upgrades are applied and before
    /// the player gains control. Generates this run's zones and starts depth tracking.
    /// </summary>
    public void StartNewRun()
    {
        _generator = new RunZoneGenerator(debugSeed);
        RunZones = _generator.Generate(tierPools);
        CurrentZone = null;

        OnRunGenerated?.Invoke(RunZones);

        if (depthTracker != null)
            depthTracker.BeginTracking(); // fires OnDepthChanged(0), which evaluates the zone below
    }

    /// <summary>
    /// Call this on a successful or failed return to the surface (GDD steps 6/7),
    /// so depth/zone tracking stops until the next dive.
    /// </summary>
    public void EndRun()
    {
        depthTracker?.StopTracking();
        CurrentZone = null;
        CurrentDepth = 0f;
    }

    private void HandleDepthChanged(float depth)
    {
        CurrentDepth = depth;

        if (_generator == null) return;

        ZoneData active = _generator.GetActiveZone(depth);
        if (active != CurrentZone)
        {
            ZoneData previous = CurrentZone;
            CurrentZone = active;
            OnZoneChanged?.Invoke(previous, CurrentZone);
        }
        print(CurrentZone);
    }

    /// <summary>
    /// Call this when the player catches a fish, to roll one from the currently
    /// active zone's weighted pool (GDD step 3, "Fishing").
    /// </summary>
    public FishData RollFishForCurrentZone()
    {
        if (CurrentZone == null)
        {
            Debug.LogWarning("DiveManager: no active zone to roll a fish from " +
                              "(submarine hasn't reached the first tier's minDepth yet).");
            return null;
        }

        return CurrentZone.GetRandomFish();
    }
}