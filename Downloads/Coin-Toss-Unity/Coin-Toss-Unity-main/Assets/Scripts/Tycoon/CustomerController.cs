// FILE: CustomerController.cs
// PURPOSE: Manages the state and behavior of a single customer.
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public enum State { InQueue, Seated, WaitingToOrder, WaitingForFood, Eating }
    public State currentState = State.InQueue;

    public float maxPatience = 30f; // 30 seconds
    public float currentPatience;

    public GameItemData orderItem; // For now, they only order one thing
    public Table seatedTable;

    void Start()
    {
        currentPatience = maxPatience;
        orderItem = PlayerProgressManager.Instance.unlockedItems[0]; // Simple: order the first available item
    }

    void Update()
    {
        if (currentState != State.Eating)
        {
            currentPatience -= Time.deltaTime;
            UIManager.Instance.customer.UpdatePatienceBar(this, currentPatience / maxPatience);

            if (currentPatience <= 0)
            {
                Leave(false);
            }
        }
    }

    public void OnSeated(Table table)
    {
        seatedTable = table;
        currentState = State.Seated;
        transform.position = table.customerSeat.position;
        UIManager.Instance.customer.RemoveFromQueue(this);
        UIManager.Instance.customer.ShowAtTable(this);
        Invoke(nameof(ReadyToOrder), 1.5f); // Wait a moment before ordering
    }

    void ReadyToOrder()
    {
        currentState = State.WaitingToOrder;
        UIManager.Instance.customer.ShowOrderBubble(this, true);
    }

    public void OnOrderTaken()
    {
        currentState = State.WaitingForFood;
        UIManager.Instance.customer.ShowOrderBubble(this, false);
    }

    public void OnFoodDelivered(GameItem food)
    {
        if (food.linkedItem == orderItem)
        {
            currentState = State.Eating;
            UIManager.Instance.customer.HidePatienceBar(this);
            Invoke(nameof(FinishEating), 5f); // Eat for 5 seconds
        }
    }

    void FinishEating()
    {
        PlayerProgressManager.Instance.AddEarnings(20);
        Leave(true);
    }

    void Leave(bool wasHappy)
    {
        if (!wasHappy)
        {
            UIManager.Instance.log.LogActivity($"Customer #{GetInstanceID()} left angrily!", "text-red-400");
        }
        if (seatedTable != null)
        {
            seatedTable.OnCustomerLeave();
        }
        UIManager.Instance.customer.RemoveFromQueue(this); // Also removes from table view
        Destroy(gameObject);
    }
}
