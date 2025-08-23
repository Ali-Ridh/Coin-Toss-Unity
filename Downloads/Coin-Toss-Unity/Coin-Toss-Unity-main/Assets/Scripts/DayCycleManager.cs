// FILE: DayCycleManager.cs
// PURPOSE: Manages the 7-day cycle and scene progression for the game.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DayStep
{
    public string sceneName;
    public string dialogueFile; // null if not a dialogue scene
    public DayStep(string scene, string dialogue = null)
    {
        sceneName = scene;
        dialogueFile = dialogue;
    }
}

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance;
    public int currentDay = 0;
    public int currentStep = 0;

    // Define the week structure
    private List<List<DayStep>> weekCycle = new List<List<DayStep>> {
        // Day 0
        new List<DayStep> { new DayStep("DialogueScene", "prologue") },
        // Day 1
        new List<DayStep> { new DayStep("Tycoon-Scene"), new DayStep("DialogueScene", "day1") },
        // Day 2
        new List<DayStep> {
            new DayStep("Tycoon-Scene"),
            new DayStep("DialogueScene", "day2a"),
            new DayStep("BattleSceneA"),
            new DayStep("DialogueScene", "day2b"),
            new DayStep("BattleSceneB"),
            new DayStep("DialogueScene", "day2c")
        },
        // Day 3
        new List<DayStep> { new DayStep("Tycoon-Scene"), new DayStep("DialogueScene", "day3") },
        // Day 4
        new List<DayStep> {
            new DayStep("Tycoon-Scene"),
            new DayStep("DialogueScene", "day4a"),
            new DayStep("BattleSceneA"),
            new DayStep("DialogueScene", "day4b"),
            new DayStep("BattleSceneB"),
            new DayStep("DialogueScene", "day4c")
        },
        // Day 5
        new List<DayStep> { new DayStep("Tycoon-Scene"), new DayStep("DialogueScene", "day5") },
        // Day 6
        new List<DayStep> {
            new DayStep("Tycoon-Scene"),
            new DayStep("Dialogue-Scene", "day6a"),
            new DayStep("Battle-SceneA"),
            new DayStep("Dialogue-Scene", "day6b"),
            new DayStep("Battle-SceneB"),
            new DayStep("Dialogue-Scene", "day6c")
        },
        // Day 7
        new List<DayStep> { new DayStep("Dialogue-Scene", "epilogue") }
    };

    void Awake()
    {
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

    public void NextStep()
    {
        currentStep++;
        if (currentDay < weekCycle.Count && currentStep >= weekCycle[currentDay].Count)
        {
            currentDay++;
            currentStep = 0;
        }
        if (currentDay < weekCycle.Count)
        {
            var step = weekCycle[currentDay][currentStep];
            if (!string.IsNullOrEmpty(step.dialogueFile))
                DialogueManager.DialogueToStart = step.dialogueFile;
            SceneManager.LoadScene(step.sceneName);
        }
        else
        {
            Debug.Log("End of week/game reached.");
            // Optionally show credits or restart
        }
    }

    public void StartDayCycle()
    {
        currentDay = 0;
        currentStep = 0;
        var step = weekCycle[currentDay][currentStep];
        if (!string.IsNullOrEmpty(step.dialogueFile))
            DialogueManager.DialogueToStart = step.dialogueFile;
        SceneManager.LoadScene(step.sceneName);
    }

    // --- NEW: Hard-coded customer counts for each day ---
    public int GetCustomerCountForDay(int day)
    {
        switch (day)
        {
            case 1: return 8;
            case 2: return 10;
            case 3: return 12;
            case 4: return 15;
            case 5: return 20;
            // Add more days as needed
            default: return 5; // fallback value
        }
    }
}
