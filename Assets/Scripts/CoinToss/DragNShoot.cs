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

            force = new Vector2(Mathf.Clamp(startPoint.x - endPoint.x, minPower.x, maxPower.x), Mathf.Clamp(startPoint.y - endPoint.y, minPower.y, maxPower.y));
            rb.AddForce(force * power, ForceMode2D.Impulse);
            tl.EndLine();

            CoinGameManager.Instance.EndPlayerTurn();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Only deal damage if it's the player's turn.
        if (CoinGameManager.Instance.currentState == CoinGameManager.GameState.PlayerTurn)
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(25f);
            }
        }
    }

    //Function to allow the player to take damage.
    public void TakeDamage(float damageAmount)
    {
        HP -= damageAmount;
        Debug.Log("Player took " + damageAmount + " damage, remaining HP: " + HP);
        if (HP <= 0)
        {
            Debug.Log("Player has been defeated!");
            Destroy(gameObject);
        }
    }
}
