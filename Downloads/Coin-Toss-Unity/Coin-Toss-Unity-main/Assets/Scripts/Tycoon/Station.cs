// FILE: Station.cs
// PURPOSE: Manages interactions with static stations.
using UnityEngine;
using System.Linq;

public class Station : MonoBehaviour
{
    public enum StationType { Queue, Kitchen }
    public StationType type;

    public void OnMouseDown()
    {
        if (GameStateManager.Instance.currentState == GameStateManager.GameState.DinerShift)
        {
            PlayerProgressManager.Instance.player.MoveTo(transform.position, () => {
                HandleInteraction();
            });
        }
    }

    void HandleInteraction()
    {
        if (type == StationType.Queue)
        {
            DinerManager.Instance.SeatCustomerFromQueue();
        }
        else if (type == StationType.Kitchen)
        {
            // Drop off ticket
            GameItem ticket = InventoryManager.Instance.items.FirstOrDefault(item => item.type == GameItem.Type.Ticket);
            if (ticket != null)
            {
                InventoryManager.Instance.RemoveItem(ticket);
                DinerManager.Instance.AddOrderToKitchen(ticket);
                return; // Only do one action per click
            }

            // Pick up food
            GameItem foodToPickUp = DinerManager.Instance.GetReadyFood();
            if (foodToPickUp != null)
            {
                InventoryManager.Instance.AddItem(foodToPickUp);
            }
        }
    }
}
