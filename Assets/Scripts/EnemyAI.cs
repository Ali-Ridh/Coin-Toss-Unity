using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // --- Health Properties ---
    public float HP = 100f;

    // --- AI Properties ---
    public float power = 8f;
    public Rigidbody2D rb;

    private Transform playerTransform;

    void Awake()
    {
        // Get the Rigidbody2D component attached to this enemy.
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Find the player's transform so the enemy knows where to aim.
        // This assumes your player GameObject is tagged as "Player".
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("EnemyAI could not find GameObject with tag 'Player'");
        }
    }

    // --- Health Logic ---

    // A public function that other objects can call to deal damage to this enemy.
    public void TakeDamage(float damageAmount)
    {
        // Subtract the damage amount from the current HP.
        HP -= damageAmount;

        // Print the remaining HP to the console for debugging.
        Debug.Log(gameObject.name + " took " + damageAmount + " damage, remaining HP: " + HP);

        // Check if the enemy's health has dropped to 0 or below.
        if (HP <= 0)
        {
            // If health is gone, call the Die() function.
            Die();
        }
    }

    // This function handles what happens when the enemy dies.
    private void Die()
    {
        // Print a death message to the console for debugging.
        Debug.Log(gameObject.name + " has been defeated!");

        // Destroy the GameObject this script is attached to, removing it from the scene.
        Destroy(gameObject);
    }

    // --- AI Logic ---

    // This function is called by the GameManager.
    public void TakeTurn()
    {
        // Safety Check: Do not act if this coin is already moving.
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Debug.LogWarning(gameObject.name + " tried to take its turn but was already moving.");
            return; // Exit the function early.
        }

        if (playerTransform == null) return;

        // Calculate the direction from the enemy to the player.
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Apply force to launch the enemy towards the player.
        rb.AddForce(direction * power, ForceMode2D.Impulse);
        Debug.Log(gameObject.name + " takes its turn.");
    }
}
