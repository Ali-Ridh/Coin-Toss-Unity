using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    // Public variables to tweak the AI's behavior.
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

    // This function is called by the GameManager.
    public void TakeTurn()
    {
        if (playerTransform == null) return;

        // Calculate the direction from the enemy to the player.
        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Apply force to launch the enemy towards the player.
        rb.AddForce(direction * power, ForceMode2D.Impulse);
        Debug.Log(gameObject.name + " takes its turn.");
    }
}
