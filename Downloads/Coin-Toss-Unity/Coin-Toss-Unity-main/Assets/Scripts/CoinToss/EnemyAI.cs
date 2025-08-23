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

    void Update()
    {
        // Ensure the Rigidbody2D is not kinematic.
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
        }
        // Check if the enemy's health has dropped to 0 or below.
        if (HP <= 0)
        {
            Die();
        }
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

        // Notify UI manager
        GameUIManager.Instance?.UpdateEnemyHealth(this);


    }
    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");
        // Remove from CoinGameManager's enemy list before destroying
        CoinGameManager.Instance?.RemoveEnemy(this);
        Destroy(gameObject);
        // Notify UI manager (enemy removed)
        GameUIManager.Instance?.UpdateEnemyHealth(this);
    }

    // --- AI Logic ---

    // This function is called by the CoinGameManager.
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        var gameManager = CoinGameManager.Instance;
        if (gameManager == null) return;

        // Only damage player during enemy turn
        if (gameManager.currentState == CoinGameManager.GameState.EnemyTurn)
        {
            DragNShoot player = collision.gameObject.GetComponent<DragNShoot>();
            if (player != null)
            {
                player.TakeDamage(25f);
            }
        }
    }
}
