// FILE: UIManager.cs
// PURPOSE: Manages all UI elements and updates.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main Panels")]
    public GameObject upgradeShopPanel;
    public Button gameStateButton;
    public Text gameStateButtonText;

    [Header("UI Components")]
    public Text earningsText;
    public InventoryUI inventory;
    public ActivityLogUI log;
    public KitchenUI kitchen;
    public CustomerUI customer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetGameStateButtonText(string text)
    {
        gameStateButtonText.text = text;
    }

    // This function is now private because only the UIManager itself should call it.
    public void CreateLogEntry(string message, string color = "text-gray-300")
    {
        if (log.container != null && log.logTextPrefab != null)
        {
            GameObject logEntry = Instantiate(log.logTextPrefab, log.container);
            logEntry.GetComponent<Text>().text = $"> {message}";
            // Note: You will need a script to handle the color string or use a different method.
            logEntry.transform.SetAsFirstSibling();
        }
    }
}

// Sub-classes for organization
[System.Serializable]
public class InventoryUI
{
    public List<Image> slots;
    public void UpdateDisplay(List<GameItem> items)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < items.Count)
            {
                slots[i].enabled = true;
                slots[i].sprite = items[i].linkedItem.icon;
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }
}

[System.Serializable]
public class ActivityLogUI
{
    public Transform container;
    public GameObject logTextPrefab;
    public void LogActivity(string message, string color = "text-gray-300")
    {
        // --- THIS IS THE FIX ---
        // The UI class now asks the main UIManager class to create the object for it,
        // instead of trying to find the singleton instance itself.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CreateLogEntry(message, color);
        }
    }
}

[System.Serializable]
public class KitchenUI
{
    public Text kitchenText;
    public void UpdateDisplay(List<CookingSlot> slots)
    {
        int readyCount = slots.Count(s => s.isReady);
        int cookingCount = slots.Count - readyCount;
        kitchenText.text = $"Ready: {readyCount} | Cooking: {cookingCount}";
    }
}

[System.Serializable]
public class CustomerUI
{
    public Transform queueContainer;
    public Transform tableContainer;
    public void ShowInQueue(CustomerController customer)
    {
        customer.transform.SetParent(queueContainer);
    }
    public void RemoveFromQueue(CustomerController customer)
    {
        if (customer.transform.parent == queueContainer)
        {
            // Logic to re-order queue visuals would go here
        }
    }
    public void ShowAtTable(CustomerController customer)
    {
        customer.transform.SetParent(tableContainer);
    }
    public void ShowOrderBubble(CustomerController customer, bool show)
    {
        // Find the order bubble child and enable/disable it
    }
    public void UpdatePatienceBar(CustomerController customer, float value)
    {
        // Find the patience bar child and update its fill
    }
    public void HidePatienceBar(CustomerController customer)
    {
        // Find the patience bar child and disable it
    }
}
