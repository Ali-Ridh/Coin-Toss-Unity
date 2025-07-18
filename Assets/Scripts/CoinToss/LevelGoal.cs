using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This enum defines the different types of conditions you can have.
// You can easily add more here later!
public enum ConditionType
{
    DefeatAllTagged,
    SurviveForTime,
    PlayerDies
}

// This class holds the data for a single win or lose condition.
[System.Serializable]
public class Condition
{
    public ConditionType type;
    public bool isFulfilled = false;

    // --- Variables for specific condition types ---
    [Tooltip("The tag of enemies to be defeated.")]
    public string targetTag;

    [Tooltip("The amount of time to survive in seconds.")]
    public float timeValue;

    // --- Internal state for tracking progress ---
    private float timer;
    private int initialCount;

    public void Initialize()
    {
        timer = timeValue;
        if (type == ConditionType.DefeatAllTagged)
        {
            initialCount = GameObject.FindGameObjectsWithTag(targetTag).Length;
            if (initialCount == 0)
            {
                Debug.LogWarning("DefeatAllTagged condition started with 0 enemies of tag: " + targetTag);
            }
        }
    }

    // This function checks if the condition has been met.
    public bool CheckCondition()
    {
        if (isFulfilled) return true;

        switch (type)
        {
            case ConditionType.DefeatAllTagged:
                if (GameObject.FindGameObjectsWithTag(targetTag).Length == 0 && initialCount > 0)
                {
                    isFulfilled = true;
                }
                break;

            case ConditionType.SurviveForTime:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    isFulfilled = true;
                }
                break;

            case ConditionType.PlayerDies:
                if (CoinGameManager.Instance.player == null) // Assumes CoinGameManager has a reference to the player
                {
                    isFulfilled = true;
                }
                break;
        }
        return isFulfilled;
    }
}


public class LevelGoal : MonoBehaviour
{
    public List<Condition> winConditions = new List<Condition>();
    public List<Condition> loseConditions = new List<Condition>();

    private bool levelOver = false;

    void Start()
    {
        // Initialize all conditions
        foreach (var condition in winConditions) condition.Initialize();
        foreach (var condition in loseConditions) condition.Initialize();
    }

    void Update()
    {
        if (levelOver) return;

        // Check for lose conditions first
        foreach (var condition in loseConditions)
        {
            if (condition.CheckCondition())
            {
                TriggerLose();
                return; // Exit once a lose condition is met
            }
        }

        // Check for win conditions
        bool allWinConditionsMet = true;
        foreach (var condition in winConditions)
        {
            if (!condition.CheckCondition())
            {
                allWinConditionsMet = false;
                break; // No need to check further if one isn't met
            }
        }

        if (allWinConditionsMet)
        {
            TriggerWin();
        }
    }

    void TriggerWin()
    {
        levelOver = true;
        Debug.Log("LEVEL WON!");
        // Add your win screen logic here
    }

    void TriggerLose()
    {
        levelOver = true;
        Debug.Log("LEVEL LOST!");
        // Add your lose screen logic here
    }
}
