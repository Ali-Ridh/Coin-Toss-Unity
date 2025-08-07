// FILE: DinerManager.cs
// PURPOSE: Manages the core gameplay loop ONLY during the DinerShift state.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DinerManager : MonoBehaviour
{
    public static DinerManager Instance;

    [Header("Spawning")]
    public GameObject customerPrefab;
    public Transform queueSpawnPoint;
    public float spawnInterval = 8f;

    [Header("Game Balance")]
    public float baseCookTime = 8f;
    
    private List<Table> allTables = new List<Table>();
    private List<CustomerController> customerQueue = new List<CustomerController>();
    private List<CustomerController> allCustomers = new List<CustomerController>();
    private List<CookingSlot> cookingSlots = new List<CookingSlot>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        // This is called when the GameStateManager activates this object
        StartShift();
    }

    void StartShift()
    {
        allTables.Clear();
        allTables.AddRange(FindObjectsByType<Table>(FindObjectsSortMode.None));
        
        // Clear any leftover state from the previous day
        customerQueue.Clear();
        allCustomers.Clear();
        cookingSlots.Clear();
        
        UIManager.Instance.log.LogActivity($"Day {PlayerProgressManager.Instance.day} has started!");
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
    }

    void Update()
    {
        // Clean up lists of customers who have been destroyed
        customerQueue.RemoveAll(c => c == null);
        allCustomers.RemoveAll(c => c == null);

        // Update cooking timers
        foreach (var slot in cookingSlots)
        {
            slot.cookTimer -= Time.deltaTime;
            if (slot.cookTimer <= 0 && !slot.isReady)
            {
                slot.isReady = true;
                UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
                UIManager.Instance.log.LogActivity("A Burger is ready for pickup!");
            }
        }

        // Check for end of day condition
        if (allCustomers.Count == 0 && customerQueue.Count == 0 && cookingSlots.Count == 0)
        {
            EndShift();
        }
    }

    void EndShift()
    {
        CancelInvoke(nameof(SpawnCustomer));
        PlayerProgressManager.Instance.day++;
        GameStateManager.Instance.TransitionToState(GameStateManager.GameState.EndOfDay);
    }

    void SpawnCustomer()
    {
        GameObject customerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        CustomerController customer = customerObj.GetComponent<CustomerController>();
        customerQueue.Add(customer);
        allCustomers.Add(customer);
        UIManager.Instance.log.LogActivity($"Customer #{customer.GetInstanceID()} has arrived.");
    }

    public void SeatCustomerFromQueue()
    {
        if (customerQueue.Count == 0) return;
        Table availableTable = allTables.Find(t => !t.IsOccupied);
        if (availableTable != null)
        {
            CustomerController customer = customerQueue[0];
            customerQueue.RemoveAt(0);
            availableTable.SeatCustomer(customer);
            UIManager.Instance.log.LogActivity($"Seating Customer #{customer.GetInstanceID()} at Table {availableTable.GetInstanceID()}.");
        }
    }

    public void AddOrderToKitchen(GameItem orderTicket)
    {
        cookingSlots.Add(new CookingSlot(orderTicket.linkedItem, baseCookTime));
        UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
        UIManager.Instance.log.LogActivity("An order for a Burger was placed.");
    }

    public GameItem GetReadyFood()
    {
        CookingSlot readySlot = cookingSlots.FirstOrDefault(s => s.isReady);
        if (readySlot != null)
        {
            cookingSlots.Remove(readySlot);
            UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
            return new GameItem { type = GameItem.Type.Food, linkedItem = readySlot.itemData };
        }
        return null;
    }
}

// Helper class for cooking
public class CookingSlot
{
    public GameItemData itemData;
    public float cookTimer;
    public bool isReady = false;
    public CookingSlot(GameItemData item, float time) { itemData = item; cookTimer = time; }
}
