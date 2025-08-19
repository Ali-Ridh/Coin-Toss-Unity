// FILE: DinerManager.cs
// PURPOSE: Manages the core gameplay loop ONLY during the DinerShift state.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DinerManager : MonoBehaviour
{
    public static DinerManager Instance;

    [Header("Spawning")]
    public GameObject customerPrefab;
    public Transform queueSpawnPoint;
    public float spawnInterval = 5f; // Time between each customer spawn

    [Header("Table Management")]
    public GameObject tablePrefab;
    public List<Transform> tableSpawnPoints;

    [Header("Game Balance")]
    public float baseCookTime = 8f;
    public GameItemData teaItemData; // Assign your "Tea" GameItemData asset here
    public int teaCost = 2;
    
    // --- THIS IS THE FIX ---
    // The list that holds the active tables was missing.
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

    // OnEnable is called when the script component is enabled by the GameStateManager
    void OnEnable()
    {
        StartShift();
    }

    // --- NEW --- OnDisable is called when the script is disabled
    void OnDisable()
    {
        // This is a safe way to stop the spawning process when the day ends.
        CancelInvoke(nameof(SpawnCustomer));
        isShiftActive = false;
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
        DailyScheduleEntry todaySchedule = DataLoader.Instance.FullSchedule.Find(d => d.day == currentDay);
        if (todaySchedule != null)
        {
            customersToSpawnToday = todaySchedule.customerCount;
        }
        else
        {
        // Because the code above is skipped, the game always runs this part
            Debug.LogWarning("No schedule found for day " + currentDay + ". Defaulting to 5 customers.");
            customersToSpawnToday = 5; // Fallback value
        }
        if (GameStateManager.Instance.uiManager != null && PlayerProgressManager.Instance != null)
        {
            GameStateManager.Instance.uiManager.log.LogActivity($"Day {PlayerProgressManager.Instance.day} has started!");
        }
        
        // --- THIS IS THE FIX ---
        // Use InvokeRepeating to call SpawnCustomer every 'spawnInterval' seconds, starting after 2 seconds.
        InvokeRepeating(nameof(SpawnCustomer), 2f, spawnInterval);
        isShiftActive = true;
    }
    
    void SpawnCustomer()
    {
        // Stop spawning if we've reached the daily limit
        if (customersSpawnedToday >= customersToSpawnToday)
        {
            CancelInvoke(nameof(SpawnCustomer)); // Stop the spawner for the rest of the day
            return;
        }

        GameObject customerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        CustomerController customer = customerObj.GetComponent<CustomerController>();
        
        allCustomers.Add(customer);
        customersSpawnedToday++;
        
        // The customer adds itself to the queue in its Start() method.
        
        if (GameStateManager.Instance.uiManager != null)
        {
            GameStateManager.Instance.uiManager.log.LogActivity($"A new customer has arrived ({customersSpawnedToday}/{customersToSpawnToday}).");
        }
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
                if(GameStateManager.Instance.uiManager != null)
                {
                    GameStateManager.Instance.uiManager.kitchen.UpdateDisplay(cookingSlots);
                    GameStateManager.Instance.uiManager.log.LogActivity($"A {slot.itemData.itemName} is ready for pickup!");
                }
            }
        }

        // The day ends when all spawned customers have finished.
        if (customersFinishedToday >= customersToSpawnToday && customersToSpawnToday > 0)
        {
            EndShift();
        }
    }

    void EndShift()
    {
        Debug.LogError("EndShift() has been called.");
        isShiftActive = false;
        CancelInvoke(nameof(SpawnCustomer)); // Ensure the spawner is stopped

        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.day++;
        }
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.TransitionToState(GameStateManager.GameState.EndOfDay);
        }
    }

    public void AddCustomerToQueue(CustomerController customer)
    {
        customerQueue.Add(customer);
    }

    public void AddOrderToKitchen(GameItem orderTicket)
    {
        cookingSlots.Add(new CookingSlot(orderTicket.linkedItem, baseCookTime));
        if (GameStateManager.Instance.uiManager != null)
        {
            GameStateManager.Instance.uiManager.kitchen.UpdateDisplay(cookingSlots);
            GameStateManager.Instance.uiManager.log.LogActivity($"An order for a {orderTicket.linkedItem.itemName} was placed.");
        }
    }

    public GameItem GetReadyFood()
    {
        CookingSlot readySlot = cookingSlots.FirstOrDefault(s => s.isReady);
        if (readySlot != null)
        {
            cookingSlots.Remove(readySlot);
            if (GameStateManager.Instance.uiManager != null)
            {
                GameStateManager.Instance.uiManager.kitchen.UpdateDisplay(cookingSlots);
            }
            return new GameItem { type = GameItem.Type.Food, linkedItem = readySlot.itemData };
        }
        return null;
    }
    
    public void HandleTableInteraction(Table table)
    {
        // Prioritize delivering tea if the player is holding it.
        GameItem teaInHand = InventoryManager.Instance.items.FirstOrDefault(item => item.linkedItem == teaItemData);
        if (teaInHand != null && table.IsOccupied)
        {
            HandleTeaDelivery(table.currentCustomer);
            return; // Tea delivery takes precedence over other interactions.
        }

        // Escort seating logic
        PlayerController player = PlayerController.Instance;
        if (player.customerBeingEscorted != null && !table.IsOccupied)
        {
            table.SeatCustomer(player.customerBeingEscorted);
            player.StopEscorting();
            UIManager.Instance.log.LogActivity($"Customer seated at Table #{table.GetInstanceID()}.");
            return;
        }

        // Logic for taking orders & delivering food
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

    public void HandleStationInteraction(Station station)
    {
        if (station.type == Station.StationType.Queue)
        {
            // This interaction is now handled by colliding with the customer directly.
        }
        else if (station.type == Station.StationType.Kitchen)
        {
            GameItem ticket = InventoryManager.Instance.items.FirstOrDefault(item => item.type == GameItem.Type.Ticket);
            if (ticket != null)
            {
                InventoryManager.Instance.RemoveItem(ticket);
                AddOrderToKitchen(ticket);
                return;
            }

            GameItem foodToPickUp = GetReadyFood();
            if (foodToPickUp != null)
            {
                InventoryManager.Instance.AddItem(foodToPickUp);
            }
        }
        else if (station.type == Station.StationType.TeaStand)
        {
            if (PlayerProgressManager.Instance.earnings >= teaCost)
            {
                GameItem tea = new GameItem { type = GameItem.Type.Food, linkedItem = teaItemData };
                if (InventoryManager.Instance.AddItem(tea))
                {
                    PlayerProgressManager.Instance.AddEarnings(-teaCost);
                    UIManager.Instance.log.LogActivity("Purchased a Tea for $2.");
                }
            }
            else
            {
                UIManager.Instance.log.LogActivity("Not enough money for Tea!", "text-red-400");
            }
        }
    }

    public void HandleTeaDelivery(CustomerController customer)
    {
        GameItem teaInHand = InventoryManager.Instance.items.FirstOrDefault(item => item.linkedItem == teaItemData);
        if (teaInHand != null)
        {
            // --- THIS IS THE FIX ---
            // The condition now includes all valid "waiting" states for a customer.
            if (customer.currentState == CustomerController.State.InQueue || 
                customer.currentState == CustomerController.State.Seated ||
                customer.currentState == CustomerController.State.WaitingToOrder ||
                customer.currentState == CustomerController.State.WaitingForFood)
            {
                InventoryManager.Instance.RemoveItem(teaInHand);
                customer.RestorePatience();
                UIManager.Instance.log.LogActivity($"Gave Tea to Customer #{customer.GetInstanceID()}.");
            }
        }
    }

    public void HandleCustomerInteraction(CustomerController customer)
    {
        // Only interact with customers who are waiting in the queue.
        if (customer.currentState == CustomerController.State.InQueue)
        {
            Table availableTable = activeTables.Find(t => !t.IsOccupied);
            if (availableTable != null)
            {
                customerQueue.Remove(customer); // Remove from the waiting list
                availableTable.SeatCustomer(customer);
                UIManager.Instance.log.LogActivity($"Seating a customer at Table #{activeTables.IndexOf(availableTable) + 1}.");
            }
            else
            {
                UIManager.Instance.log.LogActivity("No available tables to seat the customer!");
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
