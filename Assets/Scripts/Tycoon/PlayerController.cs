using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform handSlot;
    public float speed = 5f;

    public GameObject heldItem { get; private set; }
    public bool isMoving { get; private set; }
    private Coroutine movementCoroutine;

    // The MoveTo function is now a public entry point that starts the pathfinding process.
    public void MoveTo(Vector3 destination)
    {
        // Find a path to the destination.
        List<Vector3> path = Pathfinding.Instance.FindPath(transform.position, destination);

        // If a path was found, stop any current movement and start following the new path.
        if (path != null)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }
            movementCoroutine = StartCoroutine(FollowPath(path));
        }
    }

    // This coroutine moves the player along the points in the path.
    private IEnumerator FollowPath(List<Vector3> path)
    {
        isMoving = true;
        foreach (Vector3 waypoint in path)
        {
            // Loop until the player is very close to the current waypoint.
            while (Vector3.Distance(transform.position, waypoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
        }
        isMoving = false;
    }

    public void HoldItem(GameObject itemPrefab)
    {
        if (heldItem != null) Destroy(heldItem);
        heldItem = Instantiate(itemPrefab, handSlot);
    }

    public void ClearHand()
    {
        if (heldItem != null)
        {
            Destroy(heldItem);
            heldItem = null;
        }
    }
}
