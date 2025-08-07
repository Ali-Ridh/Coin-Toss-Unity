// FILE: PlayerProgressManager.cs
// PURPOSE: Stores all persistent player data.
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance;

    public int day = 1;
    public int earnings = 0;
    public float cookTimeReduction = 0f;

    public PlayerController player;
    public List<GameItemData> unlockedItems; // Start with a burger unlocked

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddEarnings(int amount)
    {
        earnings += amount;
        UIManager.Instance.earningsText.text = $"${earnings}";
    }
}
