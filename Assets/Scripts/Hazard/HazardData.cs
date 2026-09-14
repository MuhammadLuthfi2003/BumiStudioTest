using UnityEngine;

/// <summary>
/// Static data describing a single hazard type (e.g. "Rock", "Mine", "Coral Spike").
/// One asset per hazard, shared across any ZoneData that wants to spawn it.
/// Mirrors FishData so hazard pools can reuse the same weighted-pick pattern as fish pools.
/// </summary>
[CreateAssetMenu(fileName = "Hazard_", menuName = "Dive/Hazard Data")]
public class HazardData : ScriptableObject
{
    [Header("Identity")]
    public string hazardName = "New Hazard";
    [TextArea(2, 4)]
    public string description;
    public GameObject hazardPrefab; // must have a HazardInstance component + trigger Collider2D

    [Header("Damage")]
    [Tooltip("How much health this hazard removes from the submarine on contact.")]
    [Min(0f)]
    public float damageAmount = 1f;

    [Header("Rarity")]
    [Tooltip("Base spawn weight for this hazard. Higher = more common. " +
             "Actual spawn chance is this weight divided by the sum of all weights in the active zone.")]
    [Min(0.01f)]
    public float rarityWeight = 1f;
}