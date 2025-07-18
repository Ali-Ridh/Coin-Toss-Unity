using UnityEngine;

public class Table : MonoBehaviour
{


    public Transform customerSeat;
    public GameObject dirtyDishesPrefab; // Prefab for the dirty dishes visual

    public bool IsOccupied { get; private set; }
    public bool IsDirty { get; private set; }

    private GameObject dirtyDishesInstance;
    public CustomerController currentCustomer { get; private set; }

    public void OnMouseDown()
    {
        DinerManager.Instance.OnTableClicked(this);
    }

    public void SeatCustomer(CustomerController customer)
    {
        currentCustomer = customer;
        IsOccupied = true;
        customer.OnSeated(this);
    }

    public void OnCustomerLeave()
    {
        currentCustomer = null;
        IsOccupied = false;
        IsDirty = true;
        dirtyDishesInstance = Instantiate(dirtyDishesPrefab, transform.position, Quaternion.identity);
    }

    public void CleanTable()
    {
        IsDirty = false;
        if (dirtyDishesInstance != null)
        {
            Destroy(dirtyDishesInstance);
        }
    }
}


