using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class AreaTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private float TimeToTrigger = 3f;

    [Header("Events to Trigger")]
    [SerializeField]
    private UnityEvent onTrigger;

    private Collider2D collider2d;

    private float currentActivationTime = 0;
    private bool IsInCollision = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider2d = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsInCollision)
        {
            currentActivationTime += Time.deltaTime;

            if (currentActivationTime > TimeToTrigger)
            {
                currentActivationTime = 0;
                IsInCollision = false;

                onTrigger?.Invoke();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IsInCollision = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IsInCollision = false;
        }
    }
}
