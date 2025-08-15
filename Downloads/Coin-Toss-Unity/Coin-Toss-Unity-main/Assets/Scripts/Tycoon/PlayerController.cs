// FILE: PlayerController.cs
// PURPOSE: Handles direct physics-based movement, collision detection, and interaction triggers.
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 50f; // The force applied to push the player towards the mouse
    public float maxSpeed = 6f;   // The maximum speed the player can reach
    public float linearDrag = 2.0f; // Controls how quickly the player slows down
    public float collisionPenaltyThreshold = 8f; // Speed above which a collision causes a penalty

    private Rigidbody2D rb;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        rb.isKinematic = false; 
        rb.gravityScale = 0;
        rb.linearDamping = linearDrag;
    }

    void FixedUpdate()
    {
        MoveTowardsMouse();
    }

    private void MoveTowardsMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Ensure we're working in 2D

        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed > collisionPenaltyThreshold)
        {
            if (PlayerProgressManager.Instance != null)
            {
                PlayerProgressManager.Instance.AddEarnings(-5);
            }
            
            // --- THIS IS THE FIX ---
            // Get the UIManager reference from the GameStateManager.
            if (GameStateManager.Instance.uiManager != null)
            {
                GameStateManager.Instance.uiManager.log.LogActivity("Ouch! You hit something too fast!", "text-yellow-400");
            }

            Table table = collision.gameObject.GetComponent<Table>();
            if (table != null)
            {
                table.Shake();
            }
        }

        if (DinerManager.Instance != null)
        {
            Table table = collision.gameObject.GetComponent<Table>();
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
        }
    }
}
