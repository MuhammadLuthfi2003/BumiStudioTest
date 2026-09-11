using System;
using UnityEngine;

/// <summary>
/// Core Loop:
/// Prepare > Dive > Explore > Catch Fish > Manage Resources > Continue or Return.
///
/// This script owns run generation and depth/zone tracking. Other systems
/// (fish spawner, resource manager, environment/ambient renderer, UI) subscribe
/// to its events rather than polling — keeps this decoupled the same way
/// SubmarineController.OnDistanceMoved keeps movement decoupled from fuel.
///
/// Depth convention: depth increases as the submarine's Y position drops below
/// the surface reference. Depth = surfaceY - submarine.position.y, clamped to >= 0.
/// </summary>
public class DiveManager : MonoBehaviour
{
    [Header("Zone Setup")]
    [Tooltip("One pool per depth tier (e.g. 20m/40m/60m/80m). Order in this array doesn't " +
             "matter — RunZoneGenerator sorts pools by each candidate ZoneData's minDepth.")]
    [SerializeField] private ZoneTierPool[] tierPools;

    [Header("Depth Tracking")]
    [Tooltip("The submarine's transform. Its Y position below surfaceReference determines depth.")]
    [SerializeField] private Transform submarine;

    [Tooltip("World Y position that counts as 0m depth (the surface). If left unset, the " +
             "submarine's Y position at the moment StartNewRun() is called is used instead.")]
    [SerializeField] private Transform surfaceReference;

    [Header("Debug")]
    [Tooltip("Set for reproducible testing. Leave null for a random run each time.")]
    [SerializeField] private int? debugSeed = null;

    /// <summary>The zones generated for the current run, shallow to deep.</summary>
    public ZoneData[] RunZones { get; private set; }

    /// <summary>The zone the submarine is currently inside, based on current depth.</summary>
    public ZoneData CurrentZone { get; private set; }

    /// <summary>Current depth in meters (0 = surface).</summary>
    public float CurrentDepth { get; private set; }

    /// <summary>Fired once, right after a new run's zones are generated.</summary>
    public event Action<ZoneData[]> OnRunGenerated;

    /// <summary>Fired whenever the active zone changes (previousZone, newZone).</summary>
    public event Action<ZoneData, ZoneData> OnZoneChanged;

    /// <summary>Fired every frame with the latest depth value, for UI/HUD binding.</summary>
    public event Action<float> OnDepthChanged;

    private RunZoneGenerator _generator;
    private float _surfaceY;
    private bool _diveActive;

    /// <summary>
    /// Call this at the start of a dive, after pre-dive upgrades are applied and before
    /// the player gains control. Generates this run's four zones and resets depth to 0.
    /// </summary>
    public void StartNewRun()
    {
        _generator = new RunZoneGenerator(debugSeed);
        RunZones = _generator.Generate(tierPools);

        _surfaceY = surfaceReference != null
            ? surfaceReference.position.y
            : (submarine != null ? submarine.position.y : 0f);

        CurrentZone = null;
        CurrentDepth = 0f;
        _diveActive = true;

        OnRunGenerated?.Invoke(RunZones);
        EvaluateDepth(); // sets CurrentZone for depth 0 (usually null, until first tier's minDepth)
    }

    /// <summary>
    /// Call this on a successful or failed return to the surface (GDD steps 6/7),
    /// so depth/zone tracking stops until the next dive.
    /// </summary>
    public void EndRun()
    {
        _diveActive = false;
        CurrentZone = null;
        CurrentDepth = 0f;
    }

    private void Update()
    {
        if (!_diveActive || submarine == null) return;
        EvaluateDepth();
    }

    private void EvaluateDepth()
    {
        float depth = Mathf.Max(0f, _surfaceY - submarine.position.y);
        CurrentDepth = depth;
        OnDepthChanged?.Invoke(depth);

        if (_generator == null) return;

        ZoneData active = _generator.GetActiveZone(depth);
        if (active != CurrentZone)
        {
            ZoneData previous = CurrentZone;
            CurrentZone = active;
            OnZoneChanged?.Invoke(previous, CurrentZone);
        }
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