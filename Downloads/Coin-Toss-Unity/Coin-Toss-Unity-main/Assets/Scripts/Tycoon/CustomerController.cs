// FILE: CustomerController.cs
// PURPOSE: Manages the state and behavior of a single customer, including their patience.
using UnityEngine;
using UnityEngine.UI; // Required for the Slider component

public class CustomerController : MonoBehaviour
{
    public enum State { InQueue, FollowingPlayer, Seated, WaitingToOrder, WaitingForFood, Eating }
    public State currentState = State.InQueue;

    [Header("Patience")]
    public float maxPatience = 30f;
    public float currentPatience;

    [Header("UI")]
    public GameObject patienceBarPrefab; // Assign your Slider prefab here in the Inspector
    private Slider patienceBarInstance;

    [Header("Gameplay")]
    public float followSpeed = 4f;
    public float followDistance = 1.5f;
    public GameItemData orderItem;
    public Table seatedTable;
    private Transform targetToFollow;

    void Start()
    {
        currentPatience = maxPatience;
        
        // Find the player instance once and store it
        if (PlayerController.Instance != null)
        {
            targetToFollow = PlayerController.Instance.transform;
        }

        // Order the first available item
        if (PlayerProgressManager.Instance.unlockedItems.Count > 0)
        {
            orderItem = PlayerProgressManager.Instance.unlockedItems[0]; 
        }
        DinerManager.Instance.AddCustomerToQueue(this);

        // --- NEW --- Create the patience bar when the customer spawns
        CreatePatienceBar();
    }

    void Update()
    {
        if (currentState == State.FollowingPlayer && targetToFollow != null)
        {
            if (Vector3.Distance(transform.position, targetToFollow.position) > followDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetToFollow.position, followSpeed * Time.deltaTime);
            }
        }

        // --- NEW --- Decrease patience and update the bar
        if (currentState != State.Eating)
        {
            currentPatience -= Time.deltaTime;
            if (patienceBarInstance != null)
            {
                patienceBarInstance.value = currentPatience / maxPatience;
            }

            if (currentPatience <= 0)
            {
                Leave(false);
            }
        }
    }

    private void CreatePatienceBar()
    {
        // Find the main Canvas in the scene
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null && patienceBarPrefab != null)
        {
            // Create an instance of the slider prefab as a child of the main canvas
            GameObject sliderObj = Instantiate(patienceBarPrefab, mainCanvas.transform);
            
            patienceBarInstance = sliderObj.GetComponent<Slider>();
            UIFollowTarget followScript = sliderObj.GetComponent<UIFollowTarget>();
            
            // Tell the follow script to follow this specific customer
            if (followScript != null)
            {
                followScript.targetToFollow = this.transform;
            }
        }
    }

    // --- NEW --- This function will be called by other scripts to restore patience.
    public void RestorePatience()
    {
        currentPatience = maxPatience;
        Debug.Log("Patience restored for " + gameObject.name);
    }

    public void StartFollowing()
    {
        currentState = State.FollowingPlayer;
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void OnSeated(Table table)
    {
        seatedTable = table;
        currentState = State.Seated;
        transform.position = table.customerSeat.position;
        GetComponent<Collider2D>().isTrigger = false;
        Invoke(nameof(ReadyToOrder), 1.5f);
    }

    void ReadyToOrder()
    {
        currentState = State.WaitingToOrder;
    }

    public void OnOrderTaken()
    {
        currentState = State.WaitingForFood;
    }

    public void OnFoodDelivered(GameItem food)
    {
        if (food.linkedItem == orderItem)
        {
            currentState = State.Eating;
            // Hide the patience bar while the customer is eating
            if (patienceBarInstance != null) patienceBarInstance.gameObject.SetActive(false);
            Invoke(nameof(FinishEating), 5f);
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
            // Log angry leave
        }
        if (seatedTable != null)
        {
            seatedTable.OnCustomerLeave();
        }
        
        if (PlayerController.Instance.customerBeingEscorted == this)
        {
            PlayerController.Instance.StopEscorting();
        }

        DinerManager.Instance.OnCustomerFinished();
        Destroy(gameObject); // The OnDestroy function will handle cleaning up the slider
    }

    // --- NEW --- Make sure the slider is destroyed when the customer is
    void OnDestroy()
    {
        if (patienceBarInstance != null)
        {
            Destroy(patienceBarInstance.gameObject);
        }
    }
}
