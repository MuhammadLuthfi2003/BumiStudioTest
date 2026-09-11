using UnityEngine;

public enum RunStates
{
    PreDive,
    Dive,
    Lose,
}

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] DiveManager diveManager;
    [SerializeField] ResourceManager resourceManager;
    [SerializeField] CameraManager cameraManager;

    [Header("Fish Spawn Areas")]
    [Tooltip("One FishSpawnArea per depth tier, ordered shallow to deep to match DiveManager.RunZones.")]
    [SerializeField] private FishSpawnArea[] spawnAreas;



    // current game state
    public RunStates currentState = RunStates.PreDive;

    public DiveManager DiveManager { get { return diveManager; } }
    public ResourceManager ResourceManager { get { return resourceManager; } }
    public CameraManager CameraManager { get { return cameraManager; } }

    // The public static reference used by other scripts to access this instance
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        diveManager.OnRunGenerated += HandleRunGenerated;
    }

    private void OnDisable()
    {
        diveManager.OnRunGenerated -= HandleRunGenerated;
    }

    // stores the generated zones into spawn areas
    private void HandleRunGenerated(ZoneData[] zones)
    {
        if (zones == null) return;

        int count = Mathf.Min(zones.Length, spawnAreas.Length);

        if (zones.Length != spawnAreas.Length)
        {
            Debug.LogWarning($"GameManager: {zones.Length} zones generated but " +
                              $"{spawnAreas.Length} spawn areas configured. Only the first {count} will be populated.");
        }

        for (int i = 0; i < count; i++)
            spawnAreas[i]?.AssignZone(zones[i]);

        // Clear any leftover areas beyond what this run generated (e.g. array mismatch).
        for (int i = count; i < spawnAreas.Length; i++)
            spawnAreas[i]?.ClearAllFish();
    }

    // general start run sequence
    public void StartRun()
    {
        if (resourceManager != null)
        {
            resourceManager.StartDive();
        }

        if (diveManager != null)
        {
            diveManager.StartNewRun();
        }

        if (cameraManager != null)
        {
            cameraManager.StartTracking();
        }
    }

    // general stop run sequence
    public void StopRun()
    {
        if (resourceManager != null)
        {
            resourceManager.ReturnToSurface();
        }

        if (diveManager != null)
        {
            diveManager.EndRun();
        }

        if (cameraManager != null)
        {
            cameraManager.BackToSurface();
        }
    }

}
