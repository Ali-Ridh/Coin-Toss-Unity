// FILE: PlayerProgressManager.cs
// PURPOSE: Stores all persistent player data.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Make sure you have this for TextMeshPro or Text

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
        // This is the correct structure for a singleton
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
        // Call this here for testing to ensure the UIManager is ready.
        AddEarnings(1000); 
    }

    public void AddEarnings(int amount)
    {
        earnings += amount;

        // Safety check to prevent errors if the UI isn't set up
        if (UIManager.Instance != null && UIManager.Instance.earningsText != null)
        {
            UIManager.Instance.earningsText.text = $"${earnings}";
        }
        else
        {
            Debug.LogWarning("Could not update earnings text. UIManager or its earningsText is not assigned.");
        }
    }
}
