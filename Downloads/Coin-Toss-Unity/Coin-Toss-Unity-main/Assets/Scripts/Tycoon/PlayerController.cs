// FILE: PlayerController.cs
// PURPOSE: Handles player movement and triggers interactions.
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }

    public void MoveTo(Vector3 destination, System.Action onArrivalCallback = null)
    {
        targetPosition = destination;
        StartCoroutine(WaitForArrival(onArrivalCallback));
    }

    private IEnumerator WaitForArrival(System.Action onArrivalCallback)
    {
        yield return new WaitUntil(() => !isMoving);
        onArrivalCallback?.Invoke();
    }
}
