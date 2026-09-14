using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and maintains HazardInstance objects for one depth tier, mirroring FishSpawnArea's
/// role for fish. GameManager assigns this area a ZoneData per run; the area keeps itself
/// topped up to maxConcurrentHazards, respawning whenever a hazard is consumed.
/// </summary>
public class HazardSpawnArea : MonoBehaviour
{
    [Header("Spawn Area")]
    [Tooltip("Collider defining the region hazards can spawn inside.")]
    [SerializeField] private Collider2D spawnArea;

    [Header("Spawn Settings")]
    [Tooltip("How many hazards this area tries to keep alive at once.")]
    [Min(0)]
    [SerializeField] private int maxConcurrentHazards = 3;

    private ZoneData _currentZone;
    private readonly List<HazardInstance> _activeHazards = new List<HazardInstance>();

    /// <summary>Call this when a new run generates zones (see GameManager.HandleRunGenerated).</summary>
    public void AssignZone(ZoneData zone)
    {
        ClearAllHazards();
        _currentZone = zone;

        if (_currentZone == null || _currentZone.hazardPool == null || _currentZone.hazardPool.Length == 0)
            return;

        for (int i = 0; i < maxConcurrentHazards; i++)
            SpawnOne();
    }

    /// <summary>Destroys every currently active hazard in this area (e.g. between runs).</summary>
    public void ClearAllHazards()
    {
        foreach (var hazard in _activeHazards)
        {
            if (hazard != null)
                Destroy(hazard.gameObject);
        }
        _activeHazards.Clear();
    }

    ///// <summary>Called by a HazardInstance when it's consumed, so this area can respawn a replacement.</summary>
    public void NotifyHazardRemoved(HazardInstance instance)
    {
        _activeHazards.Remove(instance);
        SpawnOne();
    }

    private void SpawnOne()
    {
        if (_currentZone == null || spawnArea == null) return;

        HazardData data = _currentZone.GetRandomHazard();
        if (data == null || data.hazardPrefab == null) return;

        Vector2 point = SpawnUtility.GetRandomPointInCollider(spawnArea);
        GameObject instanceObj = Instantiate(data.hazardPrefab, point, Quaternion.identity);

        instanceObj.transform.SetParent(transform, worldPositionStays: true);

        HazardInstance instance = instanceObj.GetComponent<HazardInstance>();
        if (instance == null)
        {
            Debug.LogWarning($"HazardSpawnArea: prefab for '{data.hazardName}' has no HazardInstance component.");
            return;
        }

        instance.Initialize(data, this);
        _activeHazards.Add(instance);
    }
}