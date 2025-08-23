// FILE: PlayerProgressManager.cs
// PURPOSE: Stores all persistent player data.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Make sure you have this for TextMeshPro or Text

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance;

    public int day = 1;
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
        
    }
}
