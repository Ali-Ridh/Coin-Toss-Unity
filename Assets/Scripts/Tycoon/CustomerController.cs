using UnityEngine;
using UnityEngine.UI; // For Slider

public class CustomerController : MonoBehaviour
{
    public enum State { WaitingToOrder, WaitingForFood, Eating, Leaving }
    public State currentState;

    public float patience = 100f;
    public float maxPatience = 100f;
    public Slider patienceBar; // Assign a UI Slider in the Inspector

    public GameObject orderPrefab; // Prefab representing the food this customer wants
    public Table seatedTable;

    void Update()
    {
        // Decrease patience over time
        if (currentState != State.Eating && currentState != State.Leaving)
        {
            patience -= Time.deltaTime * 5; // Lose 5 patience per second
            if (patienceBar != null)
            {
                patienceBar.value = patience / maxPatience;
            }

            if (patience <= 0)
            {
                LeaveUnhappy();
            }
        }
    }

    public void OnSeated(Table table)
    {
        seatedTable = table;
        currentState = State.WaitingToOrder;
        transform.position = table.customerSeat.position;
    }

    public void OnOrderTaken()
    {
        currentState = State.WaitingForFood;
    }

    public void OnFoodDelivered()
    {
        currentState = State.Eating;
        patience = maxPatience; // Reset patience while eating
        Invoke(nameof(FinishEating), 5f); // Eat for 5 seconds
    }

    void FinishEating()
    {
        currentState = State.Leaving;
        seatedTable.OnCustomerLeave();
        DinerManager.Instance.AddScore(25);
        Destroy(gameObject, 1f); // Disappear after 1 second
    }

    void LeaveUnhappy()
    {
        currentState = State.Leaving;
        if (seatedTable != null)
        {
            seatedTable.OnCustomerLeave();
        }
        DinerManager.Instance.AddScore(-10);
        Destroy(gameObject);
    }
}
