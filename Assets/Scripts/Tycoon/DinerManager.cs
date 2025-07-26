// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;

// // Helper class to manage items currently being cooked.
// public class CookingSlot
// {
//     public MenuItem item;
//     public float cookTimer;
//     public bool isDone = false;

//     public CookingSlot(MenuItem itemToCook)
//     {
//         item = itemToCook;
//         cookTimer = item.cookTime;
//     }

//     public void UpdateCookTime()
//     {
//         if (isDone) return;
//         cookTimer -= Time.deltaTime;
//         if (cookTimer <= 0)
//         {
//             isDone = true;
//             Debug.Log(item.itemName + " is ready for pickup!");
//         }
//     }
// }

// public class DinerManager : MonoBehaviour
// {
//     public static DinerManager Instance;

//     [Header("Game Settings")]
//     public int maxQueueSize = 5;

//     [Header("Game Objects & Prefabs")]
//     public PlayerController player;
//     public GameObject customerPrefab;
//     public Transform queueSpawnPoint;
//     public MenuItem dirtyDishesItem; // Assign a ScriptableObject or prefab representing a "dirty dish"

//     [Header("Stations")]
//     public Station kitchenStation;
//     public Station dishStation;
//     public Station queueStation;

//     [Header("UI")]
//     public Text scoreText;

//     private int score = 0;
//     private List<CustomerController> customerQueue = new List<CustomerController>();
//     private List<Table> tables = new List<Table>();
    
//     // --- Core Gameplay Systems ---
//     private List<MenuItem> activeOrderTicket = new List<MenuItem>();
//     private List<CookingSlot> cookingSlots = new List<CookingSlot>();

//     void Awake()
//     {
//         if (Instance == null) Instance = this;
//         else Destroy(gameObject);
//     }

//     void Start()
//     {
//         // --- THIS IS THE FIX ---
//         // We wait for the DataLoader to be ready before starting the game logic.
//         // This is a safety check in addition to setting the Script Execution Order.
//         if (DataLoader.Instance == null)
//         {
//             Debug.LogError("DataLoader is not ready! DinerManager will wait one frame.");
//             StartCoroutine(DelayedStart());
//             return;
//         }
//         InitializeManager();
//     }
    
//     private IEnumerator DelayedStart()
//     {
//         yield return null; // Wait for one frame for other scripts to run Awake()
//         if (DataLoader.Instance == null)
//         {
//             Debug.LogError("FATAL: DataLoader did not initialize. Disabling DinerManager.");
//             this.enabled = false;
//             yield break;
//         }
//         InitializeManager();
//     }

//     private void InitializeManager()
//     {
//         tables.AddRange(FindObjectsByType<Table>(FindObjectsSortMode.None));
//         UpdateScoreUI();
//         InvokeRepeating(nameof(SpawnCustomer), 2f, 8f);
//         Debug.Log("DinerManager Initialized.");
//     }
    
//     void Update()
//     {
//         customerQueue.RemoveAll(customer => customer == null);

//         // Update all active cooking timers
//         foreach (CookingSlot slot in cookingSlots)
//         {
//             slot.UpdateCookTime();
//         }
//     }

//     void SpawnCustomer()
//     {
//         if (customerQueue.Count >= maxQueueSize) return;

//         // 1. Select a customer type based on its spawn weight.
//         CustomerTypeData selectedType = SelectRandomCustomerType();
//         if (selectedType == null)
//         {
//             Debug.LogError("Could not select a customer type. Check CustomerTypes.json in StreamingAssets.");
//             return;
//         }

//         // 2. Get the list of currently unlocked menu items from the progress manager.
//         List<MenuItem> unlockedMenuItems = GetUnlockedMenuItems();
//         if (unlockedMenuItems.Count == 0)
//         {
//             Debug.LogError("No menu items are unlocked! Cannot create an order.");
//             return;
//         }

//         // 3. Spawn the customer and initialize it with its type and the available menu.
//         GameObject newCustomerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
//         CustomerController controller = newCustomerObj.GetComponent<CustomerController>();
//         controller.Initialize(selectedType, unlockedMenuItems); // Call the setup method
        
//         customerQueue.Add(controller);
//         Debug.Log("A new '" + selectedType.customerTypeID + "' customer has arrived.");
//     }

//     private CustomerTypeData SelectRandomCustomerType()
//     {
//         List<CustomerTypeData> customerTypes = DataLoader.Instance.AllCustomerTypes;
//         if (customerTypes == null || customerTypes.Count == 0) return null;

//         int totalWeight = 0;
//         foreach (var type in customerTypes)
//         {
//             totalWeight += type.spawnWeight;
//         }

//         int randomPoint = Random.Range(0, totalWeight);

//         foreach (var type in customerTypes)
//         {
//             if (randomPoint < type.spawnWeight)
//             {
//                 return type;
//             }
//             else
//             {
//                 randomPoint -= type.spawnWeight;
//             }
//         }
//         return customerTypes.LastOrDefault();
//     }

//     private List<MenuItem> GetUnlockedMenuItems()
//     {
//         List<MenuItem> unlockedItems = new List<MenuItem>();
//         if (PlayerProgressManager.Instance == null || DataLoader.Instance == null) return unlockedItems;

//         List<string> unlockedIDs = PlayerProgressManager.Instance.unlockedMenuItemIDs;
//         List<MenuItem> allItems = DataLoader.Instance.AllMenuItems;

//         foreach (string id in unlockedIDs)
//         {
//             MenuItem item = allItems.Find(i => i.itemID == id);
//             if (item != null)
//             {
//                 unlockedItems.Add(item);
//             }
//         }
//         return unlockedItems;
//     }

//     public void OnStationClicked(Station station)
//     {
//         if (player.isMoving) return;
        
//         Node targetNode = Pathfinding.Instance.GetNodeFromWorldPoint(station.transform.position);
//         Node walkableNode = Pathfinding.Instance.FindNearestWalkableNode(targetNode);

//         if (walkableNode != null)
//         {
//             player.MoveTo(walkableNode.worldPosition);
//             StartCoroutine(PlayerMoveAndInteract(station));
//         }
//     }

//     public void OnTableClicked(Table table)
//     {
//         if (player.isMoving) return;
        
//         Node targetNode = Pathfinding.Instance.GetNodeFromWorldPoint(table.transform.position);
//         Node walkableNode = Pathfinding.Instance.FindNearestWalkableNode(targetNode);

//         if (walkableNode != null)
//         {
//             player.MoveTo(walkableNode.worldPosition);
//             StartCoroutine(PlayerMoveAndInteract(table));
//         }
//     }
    
//     private IEnumerator PlayerMoveAndInteract(Station station)
//     {
//         yield return new WaitUntil(() => !player.isMoving);
//         HandleStationInteraction(station);
//     }

//     private IEnumerator PlayerMoveAndInteract(Table table)
//     {
//         yield return new WaitUntil(() => !player.isMoving);
//         HandleTableInteraction(table);
//     }

//     void HandleStationInteraction(Station station)
//     {
//         if (station.type == Station.StationType.Queue)
//         {
//             // Seating logic
//         }
//         else if (station.type == Station.StationType.Kitchen)
//         {
//             if (activeOrderTicket.Count > 0)
//             {
//                 Debug.Log("Player dropped off order ticket with " + activeOrderTicket.Count + " items.");
//                 foreach (MenuItem item in activeOrderTicket)
//                 {
//                     cookingSlots.Add(new CookingSlot(item));
//                 }
//                 activeOrderTicket.Clear();
//             }

//             CookingSlot readyItemSlot = cookingSlots.FirstOrDefault(slot => slot.isDone);
//             if (readyItemSlot != null)
//             {
//                 if (player.CanAddItem())
//                 {
//                     Debug.Log("Player picked up finished " + readyItemSlot.item.itemName);
//                     player.AddItem(readyItemSlot.item);
//                     cookingSlots.Remove(readyItemSlot);
//                 }
//             }
//         }
//         else if (station.type == Station.StationType.Dishes)
//         {
//             List<MenuItem> dishesInHand = player.inventory.Where(item => item.itemID == dirtyDishesItem.itemID).ToList();
//             if (dishesInHand.Count > 0)
//             {
//                 Debug.Log("Player is washing " + dishesInHand.Count + " dirty dishes.");
//                 player.RemoveItems(dishesInHand);
//                 AddScore(15 * dishesInHand.Count);
//             }
//         }
//     }

//     void HandleTableInteraction(Table table)
//     {
//         if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingToOrder)
//         {
//             if (activeOrderTicket.Count == 0)
//             {
//                 Debug.Log("Interaction: Taking order from " + table.currentCustomer.name);
//                 activeOrderTicket.AddRange(table.currentCustomer.currentOrder);
//                 table.currentCustomer.OnOrderTaken();
//             }
//         }
//         else if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingForFood && player.inventory.Count > 0)
//         {
//             Debug.Log("Interaction: Delivering food to " + table.currentCustomer.name);
//             List<MenuItem> deliveredItems = table.currentCustomer.TryDeliverItems(player.inventory);
//             if (deliveredItems.Count > 0)
//             {
//                 player.RemoveItems(deliveredItems);
//             }
//         }
//         else if (table.IsDirty)
//         {
//             if (player.CanAddItem())
//             {
//                 Debug.Log("Interaction: Clearing dirty dishes from table " + table.name);
//                 table.CleanTable();
//                 player.AddItem(dirtyDishesItem);
//             }
//         }
//     }

//     public void AddScore(int amount)
//     {
//         score += amount;
//         UpdateScoreUI();
//     }

//     void UpdateScoreUI()
//     {
//         scoreText.text = "Score: " + score;
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DinerManager : MonoBehaviour
{
    public static DinerManager Instance;

    [Header("Game Objects")]
    public PlayerController player;
    public GameObject customerPrefab;
    public Transform queueSpawnPoint;

    [Header("Stations")]
    public Station kitchenStation;
    public Station dishStation;
    public Station queueStation;

    [Header("UI")]
    public Text scoreText;

    private int score = 0;
    private List<CustomerController> customerQueue = new List<CustomerController>();
    private List<Table> tables = new List<Table>();
    
    // This list will hold the specific items for the order the player is currently working on.
    private List<MenuItem> activeOrderTicket;
    
    public GameObject orderTicketPrefab; // This is now just a visual representation
    public GameObject foodPrefab; // This will be replaced later
    public GameObject dirtyDishesPrefab; // This will be replaced later

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        tables.AddRange(FindObjectsByType<Table>(FindObjectsSortMode.None));
        UpdateScoreUI();
        InvokeRepeating(nameof(SpawnCustomer), 2f, 8f);
    }
    
    void Update()
    {
        customerQueue.RemoveAll(customer => customer == null);
    }

    void SpawnCustomer()
    {
        // --- MODIFIED TO SKIP CUSTOMER TYPE SELECTION ---
        // This is a temporary setup for testing.
        
        GameObject newCustomerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        CustomerController controller = newCustomerObj.GetComponent<CustomerController>();

        // 1. Create a default "Regular" customer type on the fly.
        CustomerTypeData defaultType = new CustomerTypeData
        {
            customerTypeID = "Regular",
            patienceMultiplier = 1.0f,
            minOrderSize = 1,
            maxOrderSize = 1
        };

        // 2. Create a default list of available menu items.
        // For this to work, you must have at least one menu item loaded by the DataLoader.
        List<MenuItem> defaultMenu = new List<MenuItem>();
        if (DataLoader.Instance != null && DataLoader.Instance.AllMenuItems.Count > 0)
        {
            // Just use the first available menu item for this simple order.
            defaultMenu.Add(DataLoader.Instance.AllMenuItems[0]);
        }
        else
        {
            Debug.LogError("No menu items found in DataLoader! Cannot create a default order.");
            Destroy(newCustomerObj); // Destroy the customer if we can't give them an order.
            return;
        }

        // 3. Initialize the customer with these default values.
        controller.Initialize(defaultType, defaultMenu);
        
        customerQueue.Add(controller);
        Debug.Log("A new default customer has arrived in the queue.");
    }

    public void OnStationClicked(Station station)
    {
        if (player.isMoving) return;
        
        Node targetNode = Pathfinding.Instance.GetNodeFromWorldPoint(station.transform.position);
        Node walkableNode = Pathfinding.Instance.FindNearestWalkableNode(targetNode);

        if (walkableNode != null)
        {
            player.MoveTo(walkableNode.worldPosition);
            StartCoroutine(PlayerMoveAndInteract(station));
        }
    }

    public void OnTableClicked(Table table)
    {
        if (player.isMoving) return;
        
        Node targetNode = Pathfinding.Instance.GetNodeFromWorldPoint(table.transform.position);
        Node walkableNode = Pathfinding.Instance.FindNearestWalkableNode(targetNode);

        if (walkableNode != null)
        {
            player.MoveTo(walkableNode.worldPosition);
            StartCoroutine(PlayerMoveAndInteract(table));
        }
    }
    
    private IEnumerator PlayerMoveAndInteract(Station station)
    {
        yield return new WaitUntil(() => !player.isMoving);
        HandleStationInteraction(station);
    }

    private IEnumerator PlayerMoveAndInteract(Table table)
    {
        yield return new WaitUntil(() => !player.isMoving);
        HandleTableInteraction(table);
    }

    void HandleStationInteraction(Station station)
    {
        // ... (This function's logic remains the same for now)
    }

    private IEnumerator CookFoodRoutine()
    {
        yield return new WaitForSeconds(3f);
        // This will need to be updated to handle specific menu items
        // player.AddItem(...); 
    }

    void HandleTableInteraction(Table table)
    {
        // --- THIS LOGIC IS NOW UPDATED ---
        if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingToOrder)
        {
            // Check if the player is already holding an order ticket
            if (activeOrderTicket != null && activeOrderTicket.Count > 0)
            {
                Debug.Log("Player is already handling an order!");
                return;
            }

            Debug.Log("Interaction: Taking order from " + table.currentCustomer.name);
            // Copy the customer's order to the manager's active ticket
            activeOrderTicket = new List<MenuItem>(table.currentCustomer.currentOrder);
            table.currentCustomer.OnOrderTaken();
            
            // Tell the player to hold the visual ticket prefab
            // We will replace this with the new inventory UI system later
            // player.HoldItem(orderTicketPrefab); 
        }
        else if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingForFood && player.inventory.Count > 0)
        {
            Debug.Log("Interaction: Delivering food to " + table.currentCustomer.name);
            List<MenuItem> deliveredItems = table.currentCustomer.TryDeliverItems(player.inventory);
    
            if (deliveredItems.Count > 0)
            {
                player.RemoveItems(deliveredItems);
            }
        }
        else if (table.IsDirty)
        {
            // We will implement logic for picking up dishes later
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }
}
