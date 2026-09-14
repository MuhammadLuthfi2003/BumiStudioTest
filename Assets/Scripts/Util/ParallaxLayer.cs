using UnityEngine;

/// <summary>
/// Makes a background layer drift at a fraction of the camera's movement instead of
/// being locked 1:1 to it, producing a parallax depth effect. Attach this directly to
/// an environment prefab's root, or let ZoneEnvironmentController add/initialize it
/// automatically on each spawned instance.
///
/// Important: for the effect to be visible, this object should live in WORLD SPACE
/// (parented under a static parent, not under the camera itself). If it's parented to
/// the camera, its local position is always the same relative offset, so it will look
/// identical to being fully locked no matter what factor you use here.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("How strongly this layer follows camera movement. " +
             "0 = fully static in the world (max parallax). " +
             "1 = moves exactly with the camera (no parallax, same as being a camera child). " +
             "Small values (0.05*0.2) give a subtle background drift.")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.1f;

    private Transform _cameraTransform;
    private Vector3 _startCameraPosition;
    private Vector3 _startLocalPosition;
    private bool _initialized;

    private void Start()
    {
        // Allow either explicit Initialize() from a spawner, or just working standalone
        // if dropped directly onto a prefab in the scene.
        if (!_initialized)
            Initialize(Camera.main != null ? Camera.main.transform : null, parallaxFactor);
    }

    /// <summary>
    /// Call this right after spawning, before the object moves, so the recorded
    /// starting positions are accurate. Safe to call multiple times (e.g. re-used pooled layers).
    /// </summary>
    public void Initialize(Transform cameraTransform, float factor)
    {
        _cameraTransform = cameraTransform;
        parallaxFactor = factor;

        _startCameraPosition = _cameraTransform != null ? _cameraTransform.position : Vector3.zero;
        _startLocalPosition = transform.position;
        _initialized = true;
    }

    // LateUpdate so this reacts to the camera's position *after* CameraManager has
    // moved it for the frame (avoids a one-frame lag/jitter relative to the camera).
    private void LateUpdate()
    {
        if (_cameraTransform == null)
            return;

        Vector3 cameraDelta = _cameraTransform.position - _startCameraPosition;
        transform.position = _startLocalPosition + cameraDelta * parallaxFactor;
    }
}