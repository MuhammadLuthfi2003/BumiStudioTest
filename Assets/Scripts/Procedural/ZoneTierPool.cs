using UnityEngine;

/// <summary>
/// The set of candidate ZoneData assets for a single depth tier (e.g. "the 20m tier").
/// At the start of each run, RunZoneGenerator picks ONE ZoneData from each tier's pool,
/// which is what makes the theme at each depth change between runs
/// (e.g. 20m might be Coral Reef this run, Seagrass next run).
/// </summary>
[CreateAssetMenu(fileName = "TierPool_", menuName = "Dive/Zone Tier Pool")]
public class ZoneTierPool : ScriptableObject
{
    [Tooltip("Label for readability only, e.g. 'Tier 1 (20m)'.")]
    public string tierLabel = "Tier";

    [Tooltip("Candidate zones for this tier. One is chosen at random per run.")]
    public ZoneData[] candidates;

    public ZoneData GetRandomCandidate(System.Random rng = null)
    {
        if (candidates == null || candidates.Length == 0) return null;
        int index = rng != null
            ? rng.Next(candidates.Length)
            : UnityEngine.Random.Range(0, candidates.Length);
        return candidates[index];
    }
}
