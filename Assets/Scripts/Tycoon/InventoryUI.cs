// FILE: InventoryUI.cs
// PURPOSE: Manages the visual display of the player's inventory on the main canvas.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Image

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    // Assign your UI Image slots in the Inspector.
    public List<Image> inventorySlots;

    void Awake()
    {
        // Setup Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start with a clear display
        ClearDisplay();
    }

    public void UpdateDisplay(List<MenuItem> playerInventory)
    {
        // Loop through all available UI slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            // Check if there is a corresponding item in the player's inventory
            if (i < playerInventory.Count)
            {
                // There is an item, so set the sprite and make the slot visible.
                inventorySlots[i].sprite = playerInventory[i].itemIcon;
                inventorySlots[i].enabled = true;
            }
            else
            {
                // There is no item for this slot, so hide it.
                inventorySlots[i].enabled = false;
            }
        }
    }

    public void ClearDisplay()
    {
        foreach (var slot in inventorySlots)
        {
            slot.sprite = null;
            slot.enabled = false;
        }
    }
}
