using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Direct-velocity top-down movement for the submarine.
/// No drift/momentum: the sub moves exactly in the input direction at a fixed speed
/// and stops immediately when input is released.
///
/// Setup:
/// - Requires a Rigidbody2D (set to Gravity Scale 0, since this isn't platformer physics).
/// - Requires a PlayerInput component on the same GameObject, with an action named "Move"
///   (Vector2 / 2D Vector composite), Behavior set to "Invoke Unity Events",
///   with the Move event wired to this script's OnMove(InputAction.CallbackContext) method.
///
/// Fuel is consumed based on distance traveled, per the GDD ("Fuel is consumed as the
/// submarine explores"). This script doesn't touch fuel directly — it only reports how
/// far it moved via OnDistanceMoved, which the (future) Resource Manager subscribes to.
/// This keeps movement and resource-draining independent of each other.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SubmarineController : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Submarine Image")]
    [SerializeField] SpriteRenderer submarineImage;

    [Header("Movement")]
    [Tooltip("Maximum units per second the submarine can travel once fully accelerated.")]
    [Min(0f)]
    [SerializeField] private float maxMoveSpeed = 5f;

    [Tooltip("How quickly (units/sec^2) the sub speeds up toward maxMoveSpeed while input is held.")]
    [Min(0f)]
    [SerializeField] private float acceleration = 6f;

    [Tooltip("How quickly (units/sec^2) the sub slows back down toward zero once input is released. " +
                "Ignored if stopInstantlyOnRelease is true.")]
    [Min(0f)]
    [SerializeField] private float deceleration = 3f;

    /// <summary>
    /// Fired every FixedUpdate with the distance (in world units) moved that step.
    /// Subscribe to this from the Resource Manager to drain fuel based on travel.
    /// </summary>
    public event Action<float> OnDistanceMoved;



    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    InputAction moveAction;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true; // handle rotation manually

        // playerinput setup
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            InputActionMap inputActions = playerInput.actions.FindActionMap("Player");

            if (inputActions != null)
            {
                moveAction = inputActions.FindAction("Move");
            }
        }

        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
    }

    /// <summary>
    /// Wired to the PlayerInput "Move" action (Behavior: Invoke Unity Events).
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 previousPosition = _rb.position;

        bool hasInput = _moveInput.sqrMagnitude > 0.0001f;

        //// Target velocity is full speed in the input direction, or zero if no input.
        Vector2 targetVelocity = hasInput
            ? _moveInput.normalized * maxMoveSpeed
            : Vector2.zero;

        // Use a faster ramp when speeding up, a (usually slower) ramp when slowing down.
        bool isAccelerating = targetVelocity.sqrMagnitude > _rb.linearVelocity.sqrMagnitude;
        float rate = isAccelerating ? acceleration : deceleration;

        _rb.linearVelocity = Vector2.MoveTowards(
            _rb.linearVelocity,
            targetVelocity,
            rate * Time.fixedDeltaTime
        );

        // flip sprite
        if (submarineImage != null && Mathf.Abs(_moveInput.x) > 0.001f)
        {
            if (_moveInput.x > 0.001)
            {
                submarineImage.flipX = true;
            }
            else
            {
                submarineImage.flipX= false;
            }
        }

        float distanceThisStep = Vector2.Distance(previousPosition, _rb.position);
        if (distanceThisStep > 0f)
            OnDistanceMoved?.Invoke(distanceThisStep);

    }

}