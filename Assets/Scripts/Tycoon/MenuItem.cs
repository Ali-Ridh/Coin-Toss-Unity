// FILE: MenuItem.cs
// PURPOSE: Defines the properties for a single item on your diner's menu.
using UnityEngine;

[System.Serializable]
public class MenuItem
{
    public string itemName;
    public string itemID; // A unique ID like "food_burger"
    public float cookTime;
    public string requiredStationID; // e.g., "station_grill"
    public int scoreValue;
    
    // --- MODIFIED ---
    // We now use a Sprite for the icon, which is better for UI.
    public Sprite itemIcon; 
}
