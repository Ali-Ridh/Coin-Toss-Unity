//Defines a single purchasable upgrade for the diner.
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    public string upgradeID; // e.g., "unlock_fryer", "unlock_lemonade"
    public string upgradeName;
    public string description;
    public int cost;
    public string unlocksMenuItemID; // The ID of the menu item this upgrade unlocks
    public string unlocksStationID; // The ID of the station this upgrade unlocks
}
