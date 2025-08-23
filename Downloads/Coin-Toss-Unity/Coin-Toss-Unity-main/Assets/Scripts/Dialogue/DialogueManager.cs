// FILE: DialogueManager.cs
// PURPOSE: Manages the flow of dialogue based on the old JSON format.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // --- NEW: Events for the UI to listen to ---
    public static event Action<DialogueLine> OnDialogueNodeChanged;
    public static event Action<List<Choice>> OnChoicesAvailable;

    [Header("Settings")]
    public float autoAdvanceDelay = 2.5f; // Time in seconds between lines in auto mode

    private List<DialogueLine> currentConversation;
    private int currentLineIndex = 0;
    private bool isAutoMode = false;
    private Coroutine autoAdvanceCoroutine;

    public static string DialogueToStart; // Add this line

    // --- NEW: Store next scene name ---
    private string nextSceneName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Automatically start dialogue if DialogueToStart is set
            if (!string.IsNullOrEmpty(DialogueToStart))
            {
                StartDialogue(DialogueToStart);
                DialogueToStart = null; // Reset after use
            }
            else
            {
                Debug.Log("No dialogue to start. Please set DialogueToStart before starting the game.");
                // Optionally, you can set a default dialogue to start if needed
                // Example: StartDialogue("prologue");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(string fileName)
    {
        LoadConversation(fileName);
        if (currentConversation != null && currentConversation.Count > 0)
        {
            currentLineIndex = 0;
            DisplayCurrentLine();
        }
    }

    private void LoadConversation(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            DialogueLine[] lines = JsonHelper.FromJson<DialogueLine>(jsonContent);
            currentConversation = new List<DialogueLine>(lines);

            // --- NEW: Get nextScene from last line if present ---
            if (currentConversation.Count > 0)
            {
                var lastLine = currentConversation[currentConversation.Count - 1];
                // Use reflection to get nextScene if it exists
                var nextSceneField = lastLine.GetType().GetField("nextScene");
                if (nextSceneField != null)
                {
                    var value = nextSceneField.GetValue(lastLine);
                    if (value != null)
                    {
                        nextSceneName = value.ToString();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Dialogue file not found: " + path);
            currentConversation = null;
        }
    }

    private void DisplayCurrentLine()
    {
        if (currentConversation == null || currentLineIndex >= currentConversation.Count)
        {
            EndConversation();
            return;
        }

        DialogueLine line = currentConversation[currentLineIndex];

        // --- CHANGE ---
        // Instead of calling the UI directly, we now fire an event.
        // The DialogueUI script will hear this and update itself.
        OnDialogueNodeChanged?.Invoke(line);
        
        if (line.choices != null && line.choices.Count > 0)
        {
            if (isAutoMode)
            {
                ToggleAutoMode(); // Turn off auto mode when choices appear
            }
            // --- CHANGE ---
            // Fire an event to tell the UI to show the choices.
            OnChoicesAvailable?.Invoke(line.choices);
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void NextLine()
    {
        if (currentConversation == null) return;

        if (currentConversation[currentLineIndex].choices.Count > 0)
        {
            return;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    public void ToggleAutoMode()
    {
        isAutoMode = !isAutoMode;
        Debug.Log("Auto mode is now: " + (isAutoMode ? "ON" : "OFF"));

        if (isAutoMode)
        {
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine());
        }
        else
        {
            if (autoAdvanceCoroutine != null)
            {
                StopCoroutine(autoAdvanceCoroutine);
                autoAdvanceCoroutine = null;
            }
        }
    }

    public void SkipToEnd()
    {
        Debug.Log("Skipping to the end of the conversation.");
        EndConversation();
    }

    // --- CORE LOGIC ---

    private IEnumerator AutoAdvanceRoutine()
    {
        while (isAutoMode)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            NextLine();
        }
    }

    public void MakeChoice(Choice choice)
    {
        Debug.Log("Player chose: " + choice.choiceText);
        
        LoadConversation(choice.nextDialogue);
        currentLineIndex = choice.nextLineIndex;
        DisplayCurrentLine();
    }

    private void EndConversation()
    {
        Debug.Log("Conversation Ended.");
        if (isAutoMode)
        {
            ToggleAutoMode();
        }
        currentConversation = null;

        // Fire the event with a null value to signal the UI to hide.
        OnDialogueNodeChanged?.Invoke(null);

        // --- NEW: Use DayCycleManager for progression if available ---
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.NextStep();
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
