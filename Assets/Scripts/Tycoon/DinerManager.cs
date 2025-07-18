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
        tables.AddRange(FindObjectsOfType<Table>());
        UpdateScoreUI();
        InvokeRepeating(nameof(SpawnCustomer), 2f, 8f);
    }

    void Update()
    {
        customerQueue.RemoveAll(customer => customer == null);
    }

    void SpawnCustomer()
    {
        GameObject newCustomerObj = Instantiate(customerPrefab, queueSpawnPoint.position, Quaternion.identity);
        customerQueue.Add(newCustomerObj.GetComponent<CustomerController>());
        Debug.Log("A new customer has arrived in the queue.");
    }

    public void OnStationClicked(Station station)
    {
        if (player.isMoving) return;

        Debug.Log("Player clicked on station: " + station.name);
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

        Debug.Log("Player clicked on table: " + table.name);
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
        Debug.Log("Player arrived at " + station.name + ". Handling interaction.");
        HandleStationInteraction(station);
    }

    private IEnumerator PlayerMoveAndInteract(Table table)
    {
        yield return new WaitUntil(() => !player.isMoving);
        Debug.Log("Player arrived at " + table.name + ". Handling interaction.");
        HandleTableInteraction(table);
    }

    void HandleStationInteraction(Station station)
    {
        if (station.type == Station.StationType.Queue)
        {
            Debug.Log("Interaction: Queue. Trying to seat a customer.");
            if (customerQueue.Count > 0)
            {
                Table availableTable = tables.Find(t => !t.IsOccupied);
                if (availableTable != null)
                {
                    CustomerController customer = customerQueue[0];
                    customerQueue.RemoveAt(0);
                    if (customer != null)
                    {
                        Debug.Log("Seating customer at table " + availableTable.name);
                        availableTable.SeatCustomer(customer);
                    }
                }
            }
        }
        else if (station.type == Station.StationType.Kitchen)
        {
            Debug.Log("Interaction: Kitchen.");
            if (player.heldItem != null && player.heldItem.CompareTag("OrderTicket"))
            {
                Debug.Log("Player dropped off an order ticket.");
                player.ClearHand();
                StartCoroutine(CookFoodRoutine());
            }
        }
        else if (station.type == Station.StationType.Dishes)
        {
            Debug.Log("Interaction: Dishes.");
            if (player.heldItem != null && player.heldItem.CompareTag("DirtyDishes"))
            {
                Debug.Log("Player dropped off dirty dishes.");
                player.ClearHand();
                AddScore(15);
            }
        }
    }

    private IEnumerator CookFoodRoutine()
    {
        Debug.Log("Cooking food...");
        yield return new WaitForSeconds(3f);
        Debug.Log("Food is ready! Giving to player.");
        player.HoldItem(foodPrefab);
    }

    void HandleTableInteraction(Table table)
    {
        if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingToOrder)
        {
            Debug.Log("Interaction: Taking order from " + table.currentCustomer.name + " at table " + table.name);
            table.currentCustomer.OnOrderTaken();
            player.HoldItem(orderTicketPrefab);
        }
        else if (table.IsOccupied && table.currentCustomer != null && table.currentCustomer.currentState == CustomerController.State.WaitingForFood && player.heldItem != null && player.heldItem.CompareTag("Food"))
        {
            Debug.Log("Interaction: Delivering food to " + table.currentCustomer.name + " at table " + table.name);
            player.ClearHand();
            table.currentCustomer.OnFoodDelivered();
        }
        else if (table.IsDirty)
        {
            Debug.Log("Interaction: Clearing dirty dishes from table " + table.name);
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