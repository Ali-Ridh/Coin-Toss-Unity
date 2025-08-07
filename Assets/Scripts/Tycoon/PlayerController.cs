using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public bool isMoving { get; private set; }
    private Coroutine movementCoroutine;

    [Header("Inventory")]
    public int inventoryCapacity = 3;
    
    // The player's inventory is now a list of MenuItems
    public List<MenuItem> inventory = new List<MenuItem>();

    public void MoveTo(Vector3 destination)
    {
        List<Vector3> path = Pathfinding.Instance.FindPath(transform.position, destination);
        if (path != null)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }
            movementCoroutine = StartCoroutine(FollowPath(path));
        }
    }

    private IEnumerator FollowPath(List<Vector3> path)
    {
        isMoving = true;
        foreach (Vector3 waypoint in path)
        {
            while (Vector3.Distance(transform.position, waypoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);
                yield return null;
            }
        }
        isMoving = false;
    }

    // --- MODIFIED INVENTORY LOGIC ---

    public bool CanAddItem()
    {
        return inventory.Count < inventoryCapacity;
    }

    public void AddItem(MenuItem itemToAdd)
    {
        if (CanAddItem())
        {
            inventory.Add(itemToAdd);
            // Call the UI manager to update the display
            InventoryUI.Instance.UpdateDisplay(inventory);
            Debug.Log("Player picked up: " + itemToAdd.itemName);
        }
        else
        {
            Debug.LogWarning("Player inventory is full! Cannot pick up " + itemToAdd.itemName);
        }
    }

    public void RemoveItems(List<MenuItem> itemsToRemove)
    {
        foreach (MenuItem item in itemsToRemove)
        {
            inventory.Remove(item);
        }
        // Call the UI manager to update the display
        InventoryUI.Instance.UpdateDisplay(inventory);
    }
}
