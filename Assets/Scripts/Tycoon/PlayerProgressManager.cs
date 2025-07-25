using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance;

    [Header("Player Stats")]
    public int currentMoney;

    [Header("Unlocked Content")]
    // We use lists of strings to track the IDs of what the player has unlocked.
    public List<string> unlockedMenuItemIDs = new List<string>();
    public List<string> unlockedStationIDs = new List<string>();

    void Awake()
    {
        // Setup Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // For testing purposes, let's give the player some starting money
        // and unlock the basic "Burger" and "Grill".
        currentMoney = 100;
        unlockedMenuItemIDs.Add("food_burger");
        unlockedStationIDs.Add("station_grill");
    }

    // --- Public Functions to Modify Progress ---

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log("Player money is now: " + currentMoney);
        // Here you would update your money UI
    }

    public void SpendMoney(int amount)
    {
        currentMoney -= amount;
        Debug.Log("Player spent " + amount + ". Money is now: " + currentMoney);
        // Here you would update your money UI
    }

    public void UnlockMenuItem(string itemID)
    {
        if (!unlockedMenuItemIDs.Contains(itemID))
        {
            unlockedMenuItemIDs.Add(itemID);
            Debug.Log("Unlocked new menu item: " + itemID);
        }
    }

    public void UnlockStation(string stationID)
    {
        if (!unlockedStationIDs.Contains(stationID))
        {
            unlockedStationIDs.Add(stationID);
            Debug.Log("Unlocked new station: " + stationID);
        }
    }
}
