// FILE: PlayerController.cs
// PURPOSE: Handles direct physics-based movement, collision detection, and interaction triggers.
using UnityEngine;
using System.Linq; // --- ADDED --- Required for FirstOrDefault

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Movement")]
    public float moveForce = 50f;
    public float maxSpeed = 6f;
    public float linearDrag = 2.0f;
    public float collisionPenaltyThreshold = 8f;

    public CustomerController customerBeingEscorted { get; private set; }

    private Rigidbody2D rb;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        rb.isKinematic = false; 
        rb.gravityScale = 0;
        // --- FIXED --- Correct property name is 'drag'
        rb.linearDamping = linearDrag;
    }

    void FixedUpdate()
    {
        MoveTowardsMouse();
    }

    private void MoveTowardsMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        // --- FIXED --- Correct property name is 'velocity'
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce);
        }
    }

    public void StartEscorting(CustomerController customer)
    {
        customerBeingEscorted = customer;
        Debug.Log("Player is now escorting " + customer.name);
    }

    public void StopEscorting()
    {
        customerBeingEscorted = null;
        Debug.Log("Player has finished escorting.");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Table table = collision.gameObject.GetComponent<Table>();
        if (table != null)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed > collisionPenaltyThreshold)
            {
                PlayerProgressManager.Instance.AddEarnings(-5);
                GameStateManager.Instance.uiManager.log.LogActivity("Ouch! You hit a table too fast!", "text-yellow-400");
                table.Shake();
            }
        }

        if (DinerManager.Instance != null)
        {
            if (table != null)
            {
                DinerManager.Instance.HandleTableInteraction(table);
                return;
            }

            Station station = collision.gameObject.GetComponent<Station>();
            if (station != null)
            {
                DinerManager.Instance.HandleStationInteraction(station);
                return;
            }

            CustomerController customer = collision.gameObject.GetComponent<CustomerController>();
            if (customer != null)
            {
                // Check if the player is holding tea to give to the customer (priority interaction)
                GameItem teaInHand = InventoryManager.Instance.items.FirstOrDefault(item => item.linkedItem.itemName == "Tea");
                if (teaInHand != null)
                {
                    DinerManager.Instance.HandleTeaDelivery(customer);
                }
                else
                {
                    // If not holding tea, this is a seating interaction.
                    DinerManager.Instance.HandleCustomerInteraction(customer);
                }
                return;
            }
        }
    }
}
