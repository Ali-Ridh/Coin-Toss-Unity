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
        rb.linearDamping = linearDrag; // FIXED: use 'drag' instead of 'linearDamping'
    }

    void FixedUpdate()
    {
        Debug.Log($"[PlayerController] FixedUpdate called. Position: {transform.position}");
        MoveTowardsMouse();
    }

    private void MoveTowardsMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        if (rb.linearVelocity.magnitude < maxSpeed) // FIXED: use 'velocity' instead of 'linearVelocity'
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
                GameManager.Instance.SpendMoney(5);
                if (UIManager.Instance != null && UIManager.Instance.earningsText != null)
                {
                    UIManager.Instance.earningsText.text = $"${GameManager.Instance.Money}";
                }
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

        // --- NEW: Show note sprite for order tickets in inventory ---
        // This assumes you have a UIManager or Inventory UI script that displays inventory items.
        // You should update that script to check if item.type == GameItem.Type.Ticket and use a note sprite.
        // Example:
        // foreach (var item in InventoryManager.Instance.items)
        // {
        //     if (item.type == GameItem.Type.Ticket)
        //         inventorySlotImage.sprite = Resources.Load<Sprite>("ui/noteSprite"); // Use your note sprite path
        //     else
        //         inventorySlotImage.sprite = Resources.Load<Sprite>(item.linkedItem.spritePath);
        // }
    }
}

