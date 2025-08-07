// FILE: InventoryManager.cs
// PURPOSE: The single source of truth for the player's inventory.
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    public int capacity = 3;
    public List<GameItem> items = new List<GameItem>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool AddItem(GameItem item)
    {
        if (items.Count >= capacity)
        {
            UIManager.Instance.log.LogActivity("Inventory is full!", "text-red-400");
            return false;
        }
        items.Add(item);
        UIManager.Instance.inventory.UpdateDisplay(items);
        return true;
    }

    public void RemoveItem(GameItem item)
    {
        items.Remove(item);
        UIManager.Instance.inventory.UpdateDisplay(items);
    }
}

// A simple class to represent any item in the game
public class GameItem
{
    public enum Type { Ticket, Food }
    public Type type;
    public GameItemData linkedItem; // The data for what this item is (e.g., a burger)
}

// A ScriptableObject to define an item's properties
[CreateAssetMenu(fileName = "New Game Item", menuName = "Diner/Game Item Data")]
public class GameItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
}
