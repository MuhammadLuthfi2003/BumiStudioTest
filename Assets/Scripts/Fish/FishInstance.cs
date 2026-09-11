using UnityEngine;

/// <summary>
/// Lives on every spawned fish instance. Carries a reference back to the FishData it
/// represents (so catching code knows what was caught) and to the FishSpawnArea that
/// owns it (so the area can be told to top itself back up, if it respawns).
/// </summary>
public class FishInstance : MonoBehaviour
{
    [Tooltip("Fish Data, READONLY")]
    [SerializeField] private FishData fishData;

    public FishData Data { get; private set; }
    private FishSpawnArea _owner;

    public void Initialize(FishData data, FishSpawnArea owner)
    {
        Data = data;
        _owner = owner;

        fishData = data;
    }

    /// <summary>
    /// Call this from whatever system handles the player catching a fish
    ///  Removes this fish from the world and tells its spawn area a slot has opened up.
    /// </summary>
    public void Catch()
    {
        _owner?.NotifyFishRemoved(this);
        Destroy(gameObject);
    }
}