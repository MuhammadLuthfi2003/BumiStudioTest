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

    // current game state
    public RunStates currentState = RunStates.PreDive;

    public DiveManager DiveManager { get { return diveManager; } }
    public ResourceManager ResourceManager { get { return resourceManager; } }

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

    private void HandleRunGenerated(ZoneData[] zones)
    {
        // DEBUG: Print out generated zones
        // e.g. build a depth-chart UI, pre-warm environment prefabs, log the run's theme sequence
        foreach (var zone in zones)
            Debug.Log($"{zone.zoneName} at {zone.minDepth}m");
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
    }


}
