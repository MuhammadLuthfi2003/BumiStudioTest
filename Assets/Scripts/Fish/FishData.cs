using UnityEngine;

/// <summary>
/// Static data describing a single fish species.
/// One asset per fish, shared across any ZoneData that wants to spawn it.
/// </summary>
[CreateAssetMenu(fileName = "Fish_", menuName = "Dive/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string fishName = "New Fish";
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public GameObject fishPrefab; // the fish prefab when spawned

    [Header("Cargo")]
    [Tooltip("How many cargo slots this fish occupies when caught.")]
    [Min(1)]
    public int cargoSlots = 1;

    [Header("Economy")]
    [Tooltip("Money earned when this fish is sold on a successful return.")]
    [Min(0)]
    public int sellValue = 10;

    [Header("Rarity")]
    [Tooltip("Base spawn weight for this fish. Higher = more common. " +
             "Actual spawn chance is this weight divided by the sum of all weights in the active zone.")]
    [Min(0.01f)]
    public float rarityWeight = 1f;
}
