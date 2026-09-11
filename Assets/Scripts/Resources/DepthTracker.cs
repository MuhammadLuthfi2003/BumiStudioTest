using System;
using UnityEngine;

/// <summary>
/// Tracks the submarine's current depth in meters, independent of zone/run logic.
/// Depth is derived from the vertical distance between the submarine and a surface
/// reference point: depth = surfaceY - submarine.position.y, clamped to >= 0.
///
/// Kept as its own component (rather than folded into DiveManager) so depth is
/// available to any system that needs it, like HUD, resource manager, ambience the
/// same way SubmarineController.OnDistanceMoved is kept independent of fuel logic.
/// </summary>
public class DepthTracker : MonoBehaviour
{
    /// <summary>Current depth in meters (0 = surface). Updated every frame while tracking.</summary>
    public float CurrentDepth { get; private set; }

    /// <summary>True while depth is actively being tracked (i.e. during a dive).</summary>
    public bool IsTracking { get; private set; }

    /// <summary>Fired every frame while tracking, with the latest depth value.</summary>
    public event Action<float> OnDepthChanged;

    private float _surfaceY;
    private Transform submarine;

    private void Start()
    {
        submarine = GameManager.Instance.Submarine;
    }

    /// <summary>
    /// Call this at the start of a dive (e.g. from DiveManager.StartNewRun()).
    /// Establishes the 0m reference point and starts updating CurrentDepth every frame.
    /// </summary>
    public void BeginTracking()
    {
        _surfaceY = GameManager.Instance.Surface.position.y;

        IsTracking = true;
        CurrentDepth = 0f;
        OnDepthChanged?.Invoke(CurrentDepth);
    }

    /// <summary>Call this when the dive ends (return to surface or oxygen failure).</summary>
    public void StopTracking()
    {
        IsTracking = false;
        CurrentDepth = 0f;
    }

    private void Update()
    {
        if (!IsTracking || submarine == null)
            return;

        float depth = Mathf.Max(0f, _surfaceY - submarine.position.y);
        CurrentDepth = depth;
        OnDepthChanged?.Invoke(depth);
    }
}