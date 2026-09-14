using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D lives here (URP 2D Renderer package).

/// <summary>
/// Brings ZoneData's environmentPrefab and ambientTint to life.
///
/// Listens to DiveManager.OnZoneChanged and, on every zone transition:
/// - Crossfades the outgoing zone's environmentPrefab out while fading the incoming
///   zone's environmentPrefab in (all SpriteRenderers/CanvasGroups found on the prefab).
/// - Lerps a shared global Light2D's color to the incoming zone's ambientTint over the
///   same duration, so lighting and dressing change together.
///
/// Setup:
/// - Requires the URP 2D Renderer feature enabled in your render pipeline asset.
/// - Add a Light2D set to "Global" mode somewhere in the scene and assign it below.
/// - Assign an environmentParent transform for spawned zone prefabs to live under
///   (defaults to this GameObject's transform if left empty).
/// </summary>
public class ZoneEnvironmentController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Global Light2D whose color is tinted to match the active zone's ambientTint.")]
    [SerializeField] private Light2D globalLight;

    [Tooltip("Parent transform environment prefabs are instantiated under. " +
             "Defaults to this GameObject's transform if left empty.")]
    [SerializeField] private Transform environmentParent;

    [Header("Crossfade Settings")]
    [Tooltip("How long the environment crossfade and light tint take, in seconds.")]
    [Min(0f)]
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("Offset Settings")]
    [Tooltip("offset of the object from the environmentparent")]
    [SerializeField] Vector3 offset = Vector3.zero;

    private DiveManager diveManager;
    private GameObject _currentEnvironmentInstance;
    private Coroutine _crossfadeRoutine;
    private Coroutine _tintRoutine;

    private void Start()
    {
        diveManager = GameManager.Instance != null ? GameManager.Instance.DiveManager : null;

        if (diveManager != null)
        {
            diveManager.OnZoneChanged += HandleZoneChanged;
        }
        else
        {
            Debug.LogWarning("ZoneEnvironmentController: no DiveManager found (assign one directly " +
                              "or expose it on GameManager). Environment/tint won't update.");
        }
    }

    private void OnDisable()
    {
        if (diveManager != null)
            diveManager.OnZoneChanged -= HandleZoneChanged;
    }

    private void HandleZoneChanged(ZoneData previousZone, ZoneData newZone)
    {
        if (newZone == null)
            return;

        if (_crossfadeRoutine != null)
            StopCoroutine(_crossfadeRoutine);
        _crossfadeRoutine = StartCoroutine(CrossfadeEnvironment(newZone));

        if (globalLight != null)
        {
            if (_tintRoutine != null)
                StopCoroutine(_tintRoutine);
            _tintRoutine = StartCoroutine(TintLight(newZone.ambientTint));
        }
    }

    /// <summary>
    /// Instantly sets up a zone with no crossfade (e.g. call once when the very first
    /// zone of a run becomes active, if you don't want it to fade in from nothing).
    /// </summary>
    public void SnapToZone(ZoneData zone)
    {
        if (zone == null)
            return;

        if (_crossfadeRoutine != null)
        {
            StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = null;
        }

        if (_currentEnvironmentInstance != null)
            Destroy(_currentEnvironmentInstance);

        _currentEnvironmentInstance = null;
        if (zone.environmentPrefab != null)
        {
            _currentEnvironmentInstance = InstantiateEnvironment(zone.environmentPrefab);
            SetInstanceAlpha(_currentEnvironmentInstance, 1f);
        }

        if (globalLight != null)
        {
            if (_tintRoutine != null)
            {
                StopCoroutine(_tintRoutine);
                _tintRoutine = null;
            }
            globalLight.color = zone.ambientTint;
        }
    }

    private IEnumerator CrossfadeEnvironment(ZoneData zone)
    {
        GameObject oldInstance = _currentEnvironmentInstance;
        GameObject newInstance = null;

        if (zone.environmentPrefab != null)
        {
            newInstance = InstantiateEnvironment(zone.environmentPrefab);
            SetInstanceAlpha(newInstance, 0f);
        }

        _currentEnvironmentInstance = newInstance;

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = crossfadeDuration > 0f ? Mathf.Clamp01(elapsed / crossfadeDuration) : 1f;

            if (oldInstance != null) SetInstanceAlpha(oldInstance, 1f - t);
            if (newInstance != null) SetInstanceAlpha(newInstance, t);

            yield return null;
        }

        if (oldInstance != null)
            Destroy(oldInstance);

        if (newInstance != null)
            SetInstanceAlpha(newInstance, 1f);

        _crossfadeRoutine = null;
    }

    private IEnumerator TintLight(Color targetColor)
    {
        Color startColor = globalLight.color;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = crossfadeDuration > 0f ? Mathf.Clamp01(elapsed / crossfadeDuration) : 1f;
            globalLight.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        globalLight.color = targetColor;
        _tintRoutine = null;
    }

    /// <summary>
    /// Instantiates a zone's environment prefab at its own authored world position/rotation,
    /// then reparents it under environmentParent with worldPositionStays = true. Doing it in
    /// this order (rather than Instantiate(prefab, parent)) avoids Unity's default behavior of
    /// treating the prefab's transform values as local to the new parent, which would otherwise
    /// drag the environment to wherever environmentParent happens to be (e.g. a moving camera).
    /// </summary>
    private GameObject InstantiateEnvironment(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        instance.transform.SetParent(environmentParent != null ? environmentParent : transform, worldPositionStays: false);
        instance.transform.position += offset;
        return instance;
    }

    /// <summary>
    /// Fades every SpriteRenderer and CanvasGroup found on the instance (and its children)
    /// to the given alpha. Covers both world-space sprite dressing and any UI-based dressing
    /// a zone's environmentPrefab might use.
    /// </summary>
    private static void SetInstanceAlpha(GameObject instance, float alpha)
    {
        var spriteRenderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        var canvasGroups = instance.GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in canvasGroups)
            cg.alpha = alpha;
    }
}