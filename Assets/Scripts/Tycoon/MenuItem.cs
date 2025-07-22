// Defines the properties for a single item on your diner's menu.
using UnityEngine;

[System.Serializable]
public class MenuItem
{
    public string itemName;
    public string itemID; // A unique ID like "food_burger"
    public float cookTime;
    public string requiredStationID; // e.g., "station_grill"
    public int scoreValue;
    // You could add a 'public Sprite icon;' here later if you load sprites dynamically
}
