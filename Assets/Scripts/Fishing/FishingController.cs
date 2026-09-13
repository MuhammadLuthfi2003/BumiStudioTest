using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] PlayerInput playerInput;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask fishLayer;

    [SerializeField] private Vector2 boxSize = new Vector2(2f, 2f);

    [SerializeField] private Vector2 boxOffset = Vector2.zero;

    private ContactFilter2D _contactFilter;
    private readonly List<Collider2D> _results = new List<Collider2D>(10);

    private InputAction interactAction;

    /// <summary>The closest fish currently inside the detection box, or null if none.</summary>
    public FishInstance DetectedFish { get; private set; }

    /// <summary>
    /// Fired when the player tries to catch a fish but cargo is full (GDD step 3: "Fishing").
    /// UI should prompt the player to discard a fish via ResourceManager.DiscardFish, then
    /// call CompletePendingCatch() to retry, or CancelPendingCatch() if they back out.
    /// </summary>
    public event Action<FishInstance> OnCargoFull;

    /// <summary>Fired right after a fish is successfully caught and added to cargo.</summary>
    public event Action<FishInstance> OnFishCaught;

    private FishInstance _pendingCatch;

    private void Awake()
    {
        // contactfilter setup
        _contactFilter = new ContactFilter2D();
        _contactFilter.SetLayerMask(fishLayer);
        _contactFilter.useTriggers = true;

        // playerinput setup
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            InputActionMap inputActions = playerInput.actions.FindActionMap("Player");

            if (inputActions != null)
            {
                interactAction = inputActions.FindAction("Interact");
            }

            if (interactAction != null)
            {
                interactAction.Enable();
                interactAction.performed += TryCatchFish;
            }
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.Disable();
            interactAction.performed -= TryCatchFish;
        }
    }

    private void TryCatchFish(InputAction.CallbackContext context)
    {
        if (DetectedFish == null)
            return;

        AttemptCatch(DetectedFish);
    }

    /// <summary>
    /// Tries to add the fish's data to cargo via ResourceManager. On success, removes the
    /// fish from the world. On failure (cargo full), stashes it as a pending catch and lets
    /// the UI resolve the discard decision before retrying.
    /// </summary>
    private void AttemptCatch(FishInstance instance)
    {
        print("attempt to catch fish " + instance.name);
        if (instance == null || instance.Data == null)
            return;

        ResourceManager resourceManager = GameManager.Instance != null
            ? GameManager.Instance.ResourceManager
            : null;

        if (resourceManager == null)
        {
            Debug.LogWarning("FishingController: no ResourceManager available to catch fish into.");
            return;
        }

        bool caught = resourceManager.TryCatchFish(instance.Data);

        if (caught)
        {
            _pendingCatch = null;
            instance.Catch();
            OnFishCaught?.Invoke(instance);
        }
        else
        {
            _pendingCatch = instance;
            OnCargoFull?.Invoke(instance);
        }
    }

    /// <summary>Call after the player discards a fish (ResourceManager.DiscardFish) to retry the catch.</summary>
    public void CompletePendingCatch()
    {
        if (_pendingCatch != null)
            AttemptCatch(_pendingCatch);
    }

    /// <summary>Call if the player cancels the discard prompt instead of freeing up cargo space.</summary>
    public void CancelPendingCatch()
    {
        _pendingCatch = null;
    }

    private void Update()
    {
        DetectFish();
    }

    private void DetectFish()
    {
        Vector2 center = (Vector2)transform.position + boxOffset;

        int hitCount = Physics2D.OverlapBox(center, boxSize, transform.eulerAngles.z, _contactFilter, _results);

        DetectedFish = FindClosestFish(center, hitCount);
    }

    private FishInstance FindClosestFish(Vector2 center, int hitCount)
    {
        FishInstance closest = null;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _results[i];
            if (hit == null) continue;

            FishInstance instance = hit.GetComponent<FishInstance>();
            if (instance == null) continue;

            float sqrDist = ((Vector2)hit.transform.position - center).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = instance;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + boxOffset;
        Gizmos.DrawWireCube(center, boxSize);
    }
}