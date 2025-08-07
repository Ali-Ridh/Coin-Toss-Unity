// FILE: Table.cs
// PURPOSE: Manages the state of a single table.
using UnityEngine;
using System.Linq;

public class Table : MonoBehaviour
{
    public Transform customerSeat;
    public bool IsOccupied { get; private set; }
    public CustomerController currentCustomer { get; private set; }

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
        if (IsOccupied && currentCustomer.currentState == CustomerController.State.WaitingToOrder)
        {
            // Take order
            GameItem ticket = new GameItem { type = GameItem.Type.Ticket, linkedItem = currentCustomer.orderItem };
            if (InventoryManager.Instance.AddItem(ticket))
            {
                currentCustomer.OnOrderTaken();
            }
        }
        else if (IsOccupied && currentCustomer.currentState == CustomerController.State.WaitingForFood)
        {
            // Deliver food
            GameItem food = InventoryManager.Instance.items.FirstOrDefault(item => item.type == GameItem.Type.Food && item.linkedItem == currentCustomer.orderItem);
            if (food != null)
            {
                InventoryManager.Instance.RemoveItem(food);
                currentCustomer.OnFoodDelivered(food);
            }
        }
    }

    public void SeatCustomer(CustomerController customer)
    {
        IsOccupied = true;
        currentCustomer = customer;
        customer.OnSeated(this);
    }

    public void OnCustomerLeave()
    {
        IsOccupied = false;
        currentCustomer = null;
    }
}
