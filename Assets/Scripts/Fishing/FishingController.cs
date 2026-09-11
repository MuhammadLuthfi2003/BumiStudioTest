using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] PlayerInput playerInput;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask fishLayer;

    [SerializeField] private Vector3 boxSize = new Vector3(2f, 2f, 2f);

    [SerializeField] private Vector3 boxOffset = new Vector3(0f, 0f, 2f);

    // Reusable array to avoid garbage collection
    private readonly Collider[] results = new Collider[10];

    private InputAction interactAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
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

    }

    // Update is called once per frame
    void Update()
    {
        DetectFish();
    }

    private void DetectFish()
    {
        Vector3 center =
        transform.position +
        transform.TransformDirection(boxOffset);

        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            boxSize * 0.5f,
            results,
            transform.rotation,
            fishLayer
        );

        bool fishDetected = hitCount > 0;


    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + transform.TransformDirection(boxOffset),
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = oldMatrix;
    }
}
