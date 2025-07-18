using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Text

public class DinerManager : MonoBehaviour
{
    public static DinerManager Instance;

    [Header("Game Objects")]
    public PlayerController player;
    public GameObject customerPrefab;
    public Transform queueSpawnPoint;
    public Station queueStation; // Assign the Queue station here

    [Header("Stations")]
    public Station kitchenStation;
    public Station dishStation;

    [Header("UI")]
    public Text scoreText;

    private int score = 0;
    private List<CustomerController> customerQueue = new List<CustomerController>();
    private List<Table> tables = new List<Table>();

    // Items the player can hold
    public GameObject orderTicketPrefab;
    public GameObject foodPrefab;
    public GameObject dirtyDishesPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Find all tables in the scene
        tables.AddRange(FindObjectsOfType<Table>());
        UpdateScoreUI();
        InvokeRepeating(nameof(SpawnCustomer), 2f, 8f); // Spawn a customer every 8 seconds
    }

    void SpawnCustomer()
    {
        GameObject newCustomerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        customerQueue.Add(newCustomerObj.GetComponent<CustomerController>());
    }

    public void OnStationClicked(Station station)
    {
        if (player.isMoving) return;
        player.MoveTo(station.transform.position);
        StartCoroutine(PlayerMoveAndInteract(station));
    }

    public void OnTableClicked(Table table)
    {
        if (player.isMoving) return;
        player.MoveTo(table.transform.position);
        StartCoroutine(PlayerMoveAndInteract(table));
    }

    private IEnumerator PlayerMoveAndInteract(Station station)
    {
        // Wait until the player has stopped moving
        yield return new WaitUntil(() => !player.isMoving);
        HandleStationInteraction(station);
    }

    private IEnumerator PlayerMoveAndInteract(Table table)
    {
        // Wait until the player has stopped moving
        yield return new WaitUntil(() => !player.isMoving);
        HandleTableInteraction(table);
    }

    void HandleStationInteraction(Station station)
    {
        if (station.type == Station.StationType.Queue)
        {
            if (customerQueue.Count > 0)
            {
                Table availableTable = tables.Find(t => !t.IsOccupied);
                if (availableTable != null)
                {
                    CustomerController customer = customerQueue[0];
                    customerQueue.RemoveAt(0);
                    availableTable.SeatCustomer(customer);
                }
            }
        }
        else if (station.type == Station.StationType.Kitchen)
        {
            if (player.heldItem != null && player.heldItem.CompareTag("OrderTicket"))
            {
                player.ClearHand();
                // Use Invoke with the name of the function as a string.
                Invoke(nameof(GivePlayerFood), 3f); // Simulate 3s cook time
            }
        }
        else if (station.type == Station.StationType.Dishes)
        {
            if (player.heldItem != null && player.heldItem.CompareTag("DirtyDishes"))
            {
                player.ClearHand();
                AddScore(15);
            }
        }
    }

    // This new function is called by Invoke after the delay.
    void GivePlayerFood()
    {
        player.HoldItem(foodPrefab);
    }

    void HandleTableInteraction(Table table)
    {
        if (table.IsOccupied && table.currentCustomer.currentState == CustomerController.State.WaitingToOrder)
        {
            table.currentCustomer.OnOrderTaken();
            player.HoldItem(orderTicketPrefab);
        }
        else if (table.IsOccupied && table.currentCustomer.currentState == CustomerController.State.WaitingForFood && player.heldItem != null && player.heldItem.CompareTag("Food"))
        {
            player.ClearHand();
            table.currentCustomer.OnFoodDelivered();
        }
        else if (table.IsDirty)
        {
            table.CleanTable();
            player.HoldItem(dirtyDishesPrefab);
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
