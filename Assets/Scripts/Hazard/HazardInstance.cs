using UnityEngine;

/// <summary>
/// Lives on every spawned hazard instance. Carries a reference back to the HazardData it
/// represents and to the HazardSpawnArea that owns it — mirrors FishInstance's role for fish.
/// Unlike fish, hazards act automatically on contact rather than through a player-driven
/// interact action, so the damage logic lives here instead of in a separate controller.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HazardInstance : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("If true, this hazard is consumed (destroyed) after one hit, e.g. a rock or mine. " +
             "If false, it can damage repeatedly on a cooldown, e.g. a current or coral patch.")]
    [SerializeField] private bool destroyOnHit = true;

    [Tooltip("Only used if destroyOnHit is false — minimum seconds between hits.")]
    [Min(0f)]
    [SerializeField] private float hitCooldown = 1f;

    public HazardData Data { get; private set; }
    private HazardSpawnArea _owner;
    private float _lastHitTime = -Mathf.Infinity;

    public void Initialize(HazardData data, HazardSpawnArea owner)
    {
        Data = data;
        _owner = owner;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!destroyOnHit)
            TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (Data == null) return;
        if (!destroyOnHit && Time.time - _lastHitTime < hitCooldown)
            return;

        // Only the submarine should trigger damage.
        if (other.GetComponent<SubmarineController>() == null)
            return;

        ResourceManager resourceManager = GameManager.Instance != null
            ? GameManager.Instance.ResourceManager
            : null;

        if (resourceManager == null)
        {
            Debug.LogWarning("HazardInstance: no ResourceManager available to damage.");
            return;
        }

        resourceManager.TakeDamage(Data.damageAmount);
        _lastHitTime = Time.time;

        if (destroyOnHit)
            Consume();
    }

    /// <summary>Removes this hazard from the world and tells its spawn area a slot opened up.</summary>
    private void Consume()
    {
        _owner?.NotifyHazardRemoved(this);
        Destroy(gameObject);
    }
}