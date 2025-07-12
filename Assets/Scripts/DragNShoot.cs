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
        if(Input.GetMouseButtonDown(1))
        {
            startPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(startPoint);
            startPoint.z = 15;
        }

        if(Input.GetMouseButton(1))
        {
            Vector3 currentPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            currentPoint.z = 15;
            tl.RenderLine( startPoint, currentPoint);
        }

        if (Input.GetMouseButtonUp(1))
        {
            endPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            endPoint.z = 15;

            force = new Vector2(Mathf.Clamp(startPoint.x - endPoint.x, minPower.x, maxPower.x), Mathf.Clamp(startPoint.y - endPoint.y, minPower.y, maxPower.y));
            rb.AddForce(force * power, ForceMode2D.Impulse);
            tl.EndLine();
            // Tell the GameManager that the player has finished their action.
            GameManager.Instance.EndPlayerTurn();
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object we collided with has the Enemy script.
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        // If the enemy component exists, it means we hit an enemy.
        if (enemy != null)
        {
            // Call the TakeDamage function on the enemy we hit.
            // You can define how much damage to deal. For now, let's say 25.
            enemy.TakeDamage(25f);
        }
    }
}
