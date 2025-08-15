// FILE: DinerManager.cs
// PURPOSE: Manages the core gameplay loop ONLY during the DinerShift state.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;

public class DinerManager : MonoBehaviour
{
    public static DinerManager Instance;

    [Header("Spawning")]
    public GameObject customerPrefab;
    public Transform queueSpawnPoint;
    public float spawnInterval = 5f;

    [Header("Table Management")]
    public GameObject tablePrefab;
    public List<Transform> tableSpawnPoints;

    [Header("Game Balance")]
    public float baseCookTime = 8f;

    private List<Table> activeTables = new List<Table>();
    private List<CustomerController> customerQueue = new List<CustomerController>();
    private List<CustomerController> allCustomers = new List<CustomerController>();
    private List<CookingSlot> cookingSlots = new List<CookingSlot>();

    private int customersToSpawnToday;
    private int customersSpawnedToday;
    private int customersFinishedToday;
    private bool isShiftActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("DinerManager instance created: " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("Duplicate DinerManager found. Destroying: " + gameObject.name);
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Debug.Log("DinerManager.OnEnable() called. Starting shift...");
        StartShift();
    }

    void StartShift()
    {
        if (activeTables.Count == 0)
        {
            AddNewTable();
        }
        
        customerQueue.Clear();
        allCustomers.Clear();
        cookingSlots.Clear();
        customersSpawnedToday = 0;
        customersFinishedToday = 0;

        int currentDay = PlayerProgressManager.Instance != null ? PlayerProgressManager.Instance.day : 1;
        
        Debug.LogWarning("No schedule found for day " + currentDay + ". Defaulting to 5 customers.");
        customersToSpawnToday = 5;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.log.LogActivity($"Day {currentDay} has started! Customers to serve: {customersToSpawnToday}");
        }
        
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
        isShiftActive = true;
    }
    
    public bool CanAddTable()
    {
        return activeTables.Count < tableSpawnPoints.Count;
    }

    public void AddNewTable()
    {
        if (!CanAddTable()) return;

        Transform spawnPoint = tableSpawnPoints[activeTables.Count];
        GameObject tableObj = Instantiate(tablePrefab, spawnPoint.position, spawnPoint.rotation);
        activeTables.Add(tableObj.GetComponent<Table>());
    }

    public void SeatCustomerFromQueue()
    {
        if (customerQueue.Count == 0) return;
        Table availableTable = activeTables.Find(t => !t.IsOccupied);
        if (availableTable != null)
        {
            CustomerController customer = customerQueue[0];
            customerQueue.RemoveAt(0);
            availableTable.SeatCustomer(customer);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.log.LogActivity($"Seating Customer #{customer.GetInstanceID()} at Table {availableTable.GetInstanceID()}.");
            }
        }
    }

    public void OnCustomerFinished()
    {
        customersFinishedToday++;
    }

    void Update()
    {
        if (!isShiftActive) return;

        customerQueue.RemoveAll(c => c == null);
        allCustomers.RemoveAll(c => c == null);

        foreach (var slot in cookingSlots)
        {
            slot.cookTimer -= Time.deltaTime;
            if (slot.cookTimer <= 0 && !slot.isReady)
            {
                slot.isReady = true;
                if(UIManager.Instance != null)
                {
                    UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
                    UIManager.Instance.log.LogActivity($"A {slot.itemData.itemName} is ready for pickup!");
                }
            }
        }

        if (customersFinishedToday >= customersToSpawnToday && customersToSpawnToday > 0)
        {
            EndShift();
        }
    }

    void EndShift()
    {
        Debug.LogError("EndShift() has been called. Cancelling customer spawning.");
        isShiftActive = false;
        CancelInvoke(nameof(SpawnCustomer));
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.day++;
        }
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.TransitionToState(GameStateManager.GameState.EndOfDay);
        }
    }

    void SpawnCustomer()
    {
        Debug.Log("SpawnCustomer() function was successfully called!");

        if (customersSpawnedToday >= customersToSpawnToday)
        {
            CancelInvoke(nameof(SpawnCustomer));
            return;
        }

        GameObject customerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        CustomerController customer = customerObj.GetComponent<CustomerController>();
        
        customerQueue.Add(customer);
        allCustomers.Add(customer);
        customersSpawnedToday++;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.log.LogActivity($"A new customer has arrived ({customersSpawnedToday}/{customersToSpawnToday}).");
        }
    }

    public void AddOrderToKitchen(GameItem orderTicket)
    {
        cookingSlots.Add(new CookingSlot(orderTicket.linkedItem, baseCookTime));
        if (UIManager.Instance != null)
        {
            UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
            UIManager.Instance.log.LogActivity($"An order for a {orderTicket.linkedItem.itemName} was placed.");
        }
    }

    public GameItem GetReadyFood()
    {
        CookingSlot readySlot = cookingSlots.FirstOrDefault(s => s.isReady);
        if (readySlot != null)
        {
            cookingSlots.Remove(readySlot);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.kitchen.UpdateDisplay(cookingSlots);
            }
            return new GameItem { type = GameItem.Type.Food, linkedItem = readySlot.itemData };
        }
        return null;
    }
    
    public void HandleTableInteraction(Table table)
    {
        if (table.IsOccupied && table.currentCustomer.currentState == CustomerController.State.WaitingToOrder)
        {
            GameItem ticket = new GameItem { type = GameItem.Type.Ticket, linkedItem = table.currentCustomer.orderItem };
            if (InventoryManager.Instance.AddItem(ticket))
            {
                table.currentCustomer.OnOrderTaken();
            }
        }
        else if (table.IsOccupied && table.currentCustomer.currentState == CustomerController.State.WaitingForFood)
        {
            GameItem food = InventoryManager.Instance.items.FirstOrDefault(item => item.type == GameItem.Type.Food && item.linkedItem == table.currentCustomer.orderItem);
            if (food != null)
            {
                InventoryManager.Instance.RemoveItem(food);
                table.currentCustomer.OnFoodDelivered(food);
            }
        }
    }

    // --- FIXED: ADDED MISSING FUNCTION ---
    public void HandleStationInteraction(Station station)
    {
        if (station.type == Station.StationType.Queue)
        {
            SeatCustomerFromQueue();
        }
        else if (station.type == Station.StationType.Kitchen)
        {
            // Drop off ticket
            GameItem ticket = InventoryManager.Instance.items.FirstOrDefault(item => item.type == GameItem.Type.Ticket);
            if (ticket != null)
            {
                InventoryManager.Instance.RemoveItem(ticket);
                AddOrderToKitchen(ticket);
                return;
            }

            // Pick up food
            GameItem foodToPickUp = GetReadyFood();
            if (foodToPickUp != null)
            {
                InventoryManager.Instance.AddItem(foodToPickUp);
            }
        }
    }
}


public class CookingSlot
{
    public GameItemData itemData;
    public float cookTimer;
    public bool isReady = false;

    public CookingSlot(GameItemData item, float time)
    {
        itemData = item;
        cookTimer = time;
    }
}
