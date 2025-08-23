using UnityEngine;

public class DragNShoot : MonoBehaviour
{
    public float HP = 100;
    public float power = 10;
    public Rigidbody2D rb;

    public Vector2 minPower;
    public Vector2 maxPower;

    TrajectoryLine tl;

    Camera cam;
    Vector2 force;
    Vector3 startPoint;
    Vector3 endPoint;

    private bool hasShot = false; // Track if player has just shot

    private void Start()
    {
        cam = Camera.main;
        tl = GetComponent<TrajectoryLine>();
    }

    private void Update()
    {
        if (CoinGameManager.Instance == null || CoinGameManager.Instance.currentState != CoinGameManager.GameState.PlayerTurn)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            startPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            startPoint.z = 15;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            currentPoint.z = 15;
            tl.RenderLine(startPoint, currentPoint);
        }


        if (Input.GetMouseButtonUp(1))
        {
            endPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            endPoint.z = 15;

            float forceMultiplier = PlayerUpgradeManager.Instance != null ? PlayerUpgradeManager.Instance.ForceMultiplier : 1f;
            force = new Vector2(
                Mathf.Clamp(startPoint.x - endPoint.x, minPower.x, maxPower.x),
                Mathf.Clamp(startPoint.y - endPoint.y, minPower.y, maxPower.y)
            );
            rb.AddForce(force * power * forceMultiplier, ForceMode2D.Impulse);
            tl.EndLine();

            hasShot = true;

            CoinGameManager.Instance.EndPlayerTurn();
            hasShot = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var gameManager = CoinGameManager.Instance;
        if (gameManager == null) return;

        // Verbose logging for every collision
        string otherName = collision.gameObject.name;
        string otherType = collision.gameObject.GetType().Name;
        string otherTag = collision.gameObject.tag;
        string logMsg = $"[PLAYER COLLISION] Player '{gameObject.name}' collided with '{otherName}' (Type: {otherType}, Tag: {otherTag}) at position {collision.contacts[0].point}";

        // Check if collided object has EnemyAI
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            logMsg += " [Has EnemyAI Component]";
            float damage = PlayerUpgradeManager.Instance != null ? PlayerUpgradeManager.Instance.PlayerDamage : 25f;
            enemy.TakeDamage(damage);
        }
        else
        {
            logMsg += " [No EnemyAI Component]";
        }

        Debug.Log(logMsg);

        // Player's turn: deal damage to enemy only if just shot
        if (gameManager.currentState == CoinGameManager.GameState.PlayerTurn)
        {
            if (enemy != null)
            {
                Debug.Log("[PLAYER ACTION] Inflicting damage to enemy: " + enemy.name);
                enemy.TakeDamage(25f);
                // hasShot = false; // REMOVE THIS LINE
            }
        }
        // Enemy's turn: take damage from enemy collision
        else if (gameManager.currentState == CoinGameManager.GameState.EnemyTurn)
        {
            if (enemy != null)
            {
                Debug.Log("[PLAYER ACTION] Taking damage from enemy: " + enemy.name);
                TakeDamage(25f);
            }
        }
    }

    //Function to allow the player to take damage.
    public void TakeDamage(float damageAmount)
    {
        HP -= damageAmount;
        Debug.Log("Player took " + damageAmount + " damage, remaining HP: " + HP);

        // Notify UI manager
        GameUIManager.Instance?.UpdatePlayerHealth(HP);

        if (HP <= 0)
        {
            Debug.Log("Player has been defeated!");
            Destroy(gameObject);
        }
    }
}
