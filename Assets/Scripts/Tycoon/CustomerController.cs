using System.Collections.Generic;
using System.Linq; // Required for advanced list queries
using UnityEngine;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    public enum State { WaitingToOrder, WaitingForFood, Eating, Leaving }
    public State currentState;

    public float patience { get; private set; }
    public float maxPatience { get; private set; }
    public List<MenuItem> currentOrder { get; private set; }
    public CustomerTypeData customerType { get; private set; }

    [Header("UI Settings")]
    public GameObject patienceBarPrefab;
    public Transform orderIconsContainer; 
    private Slider patienceBarInstance;

    public Table seatedTable;

    public void Initialize(CustomerTypeData type, List<MenuItem> availableMenuItems)
    {
        customerType = type;
        maxPatience = 100f * type.patienceMultiplier;
        patience = maxPatience;

        currentOrder = new List<MenuItem>();
        int orderSize = Random.Range(type.minOrderSize, type.maxOrderSize + 1);

        for (int i = 0; i < orderSize; i++)
        {
            if (availableMenuItems.Count > 0)
            {
                currentOrder.Add(availableMenuItems[Random.Range(0, availableMenuItems.Count)]);
            }
        }
        
        // Use FindFirstObjectByType instead of the obsolete FindObjectOfType
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas != null && patienceBarPrefab != null)
        {
            GameObject sliderObj = Instantiate(patienceBarPrefab, mainCanvas.transform);
            patienceBarInstance = sliderObj.GetComponent<Slider>();
            UIFollowTarget followScript = sliderObj.GetComponent<UIFollowTarget>();
            if (followScript != null)
            {
                followScript.targetToFollow = this.transform;
            }
        }

        DisplayOrder();
    }

    void DisplayOrder()
    {
        // Clear any previous icons
        foreach (Transform child in orderIconsContainer)
        {
            Destroy(child.gameObject);
        }

        // Create an icon for each item in the order
        foreach (MenuItem item in currentOrder)
        {
            // --- THIS IS THE FIX ---
            // We now check for itemIcon (Sprite) instead of orderIconPrefab (GameObject)
            if (item.itemIcon != null)
            {
                // Create a new GameObject with an Image component to display the sprite
                GameObject iconObj = new GameObject(item.itemName + " Icon");
                iconObj.transform.SetParent(orderIconsContainer, false); // Set parent
                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = item.itemIcon;
                iconImage.preserveAspect = true;
            }
        }
    }

    void Update()
    {
        if (currentState != State.Eating && currentState != State.Leaving)
        {
            patience -= Time.deltaTime * 5;
            if (patienceBarInstance != null)
            {
                patienceBarInstance.value = patience / maxPatience;
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
        orderIconsContainer.gameObject.SetActive(false);
    }

    public List<MenuItem> TryDeliverItems(List<MenuItem> playerInventory)
    {
        List<MenuItem> deliveredItems = new List<MenuItem>();
        List<MenuItem> tempOrder = new List<MenuItem>(currentOrder);

        foreach (MenuItem itemInHand in playerInventory)
        {
            MenuItem matchingOrderItem = tempOrder.FirstOrDefault(orderItem => orderItem.itemID == itemInHand.itemID);
            if (matchingOrderItem != null)
            {
                deliveredItems.Add(matchingOrderItem);
                tempOrder.Remove(matchingOrderItem);
                DinerManager.Instance.AddScore(matchingOrderItem.scoreValue);
                Debug.Log("Successfully delivered " + matchingOrderItem.itemName);
            }
        }

        if (deliveredItems.Count > 0)
        {
            currentOrder = tempOrder;
            DisplayOrder();
            if (currentOrder.Count == 0)
            {
                StartEating();
            }
        }
        
        return deliveredItems;
    }

    private void StartEating()
    {
        currentState = State.Eating;
        patience = maxPatience;
        if (patienceBarInstance != null) patienceBarInstance.gameObject.SetActive(false);
        Invoke(nameof(FinishEating), 5f);
    }

    void FinishEating()
    {
        currentState = State.Leaving;
        seatedTable.OnCustomerLeave();
        DinerManager.Instance.AddScore(10);
        Destroy(gameObject, 1f);
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

    void OnDestroy()
    {
        if (patienceBarInstance != null)
        {
            Destroy(patienceBarInstance.gameObject);
        }
    }
}
