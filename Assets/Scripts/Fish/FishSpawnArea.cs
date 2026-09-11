using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents one zone's physical fishing ground in the scene. Placed at a depth tier's
/// location, its Collider2D defines the boundary that fish are randomly scattered inside.
///
/// This component doesn't know which ZoneData it will show — that's decided per-run by
/// RunZoneGenerator. Whatever assigns the run's zones (see ZoneSpawnCoordinator) calls
/// AssignZone() once the theme for this tier has been picked, and this component takes
/// care of populating (and optionally repopulating) fish from there on.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FishSpawnArea : MonoBehaviour
{
    [Header("Boundary")]
    [Tooltip("Defines the area fish can spawn in. Set 'Is Trigger' so it doesn't block the submarine. " +
             "Any Collider2D shape works (Box, Polygon, etc.) — spawn points are rejection-sampled " +
             "against its actual shape, not just its bounding box.")]
    [SerializeField] private Collider2D bounds;

    [Header("Population")]
    [Tooltip("How many fish can be alive in a zone at once.")]
    [Min(0)]
    [SerializeField] private int maxFishAlive = 10;

    [Tooltip("If true, catching a fish here spawns a replacement so the zone stays topped up " +
             "at maxFishAlive. If false, the zone gradually empties out over the course of a dive.")]
    [SerializeField] private bool respawnOnCatch = true;

    [Tooltip("Delay before a replacement fish appears after one is caught. Only used if " +
             "respawnOnCatch is true. Set to 0 for an instant respawn.")]
    [Min(0f)]
    [SerializeField] private float respawnDelay = 2f;

    /// <summary>The zone currently assigned to this area (set by ZoneSpawnCoordinator each run).</summary>
    public ZoneData CurrentZone { get; private set; }

    /// <summary>Fish currently alive in this area.</summary>
    public IReadOnlyList<FishInstance> ActiveFish => _activeFish;
    private readonly List<FishInstance> _activeFish = new List<FishInstance>();

    private void Reset()
    {
        bounds = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (bounds == null)
            bounds = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Assigns this run's zone to this area and (re)populates it up to maxFishAlive.
    /// Call this once per run, after RunZoneGenerator has picked zones
    /// (e.g. from DiveManager.OnRunGenerated via ZoneSpawnCoordinator).
    /// </summary>
    public void AssignZone(ZoneData zone)
    {
        ClearAllFish();
        CurrentZone = zone;

        if (CurrentZone == null)
            return;

        for (int i = 0; i < maxFishAlive; i++)
            SpawnOneFish();
    }

    /// <summary>Destroys every fish currently alive in this area without spawning replacements.</summary>
    public void ClearAllFish()
    {
        foreach (var fish in _activeFish)
        {
            if (fish != null)
                Destroy(fish.gameObject);
        }
        _activeFish.Clear();
    }

    /// <summary>Called by FishInstance.Catch() when a fish belonging to this area is removed.</summary>
    public void NotifyFishRemoved(FishInstance instance)
    {
        _activeFish.Remove(instance);

        if (!respawnOnCatch || CurrentZone == null)
            return;

        if (respawnDelay <= 0f)
            SpawnOneFish();
        else
            Invoke(nameof(SpawnOneFishFromInvoke), respawnDelay);
    }

    // Invoke() requires a zero-argument method.
    private void SpawnOneFishFromInvoke() => SpawnOneFish();

    private void SpawnOneFish()
    {
        if (CurrentZone == null || bounds == null)
            return;

        FishData fishData = CurrentZone.GetRandomFish();
        if (fishData == null || fishData.fishPrefab == null)
            return;

        Vector2 point = SpawnUtility.GetRandomPointInCollider(bounds);
        GameObject instance = Instantiate(fishData.fishPrefab, point, Quaternion.identity);

        instance.transform.SetParent(transform, worldPositionStays: true);

        FishInstance fishInstance = instance.GetComponent<FishInstance>();
        if (fishInstance == null)
            fishInstance = instance.AddComponent<FishInstance>();

        fishInstance.Initialize(fishData, this);
        _activeFish.Add(fishInstance);
    }
}