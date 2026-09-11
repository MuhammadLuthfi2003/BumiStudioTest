using System.Collections;
using UnityEngine;

/// <summary>
/// Used to control the camera while gameplay
/// Tracking the player or not (moving the camera) is determined by IsTracking
/// if IsTracking is true, moves the camera following the player,
/// else, disables camera movement
/// </summary>

public class CameraManager : MonoBehaviour
{
    [Header("Lobby Position")]
    [Tooltip("Determines where the camera should be positioned in the Pre-Dive phase")]
    [SerializeField] private Vector3 lobbyPosition;

    [Header("Start Dive Position")]
    [Tooltip("Determines where the camera should be positioned when the dive starts")]
    [SerializeField] private Vector3 startPosition;

    [Header("Camera Settings")]
    [Tooltip("Determines what the camera should look at when tracking")]
    [SerializeField] private Transform target; // refers to the player's transform

    [Tooltip("Determines the time taken to catch up to the player")]
    [SerializeField] private float smoothTime = 0.25f; // Time taken to catch up to the player

    [Tooltip("Determines z index camera offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); 

    private bool isTracking = false; // camera will follow the player when set to true
    private Vector3 velocity = Vector3.zero;

    public void SetIsTracking(bool isTracking)
    {
        this.isTracking = isTracking;
    }

    public void StartTracking()
    {
        // sets camera to the desired position first
        transform.position = startPosition;
        StartCoroutine(delayTrackingStart());
    }

    // little delay when player starts the game
    IEnumerator delayTrackingStart()
    {
        yield return new WaitForSeconds(smoothTime);
        SetIsTracking(true);
    }

    public void BackToSurface()
    {
        // set tracking bool
        SetIsTracking(false);

        // sets camera to the lobby
        transform.position = lobbyPosition;
    }

    private void LateUpdate()
    {
        if (target != null && isTracking)
        {
            // Calculate where the camera wants to go
            Vector3 targetPosition = target.position + offset;

            // Smoothly slide the camera towards that position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
