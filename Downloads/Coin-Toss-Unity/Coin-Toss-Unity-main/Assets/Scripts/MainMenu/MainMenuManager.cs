using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class MainMenuManager : MonoBehaviour
{
    // This function will be called by the "New Game" button.
    public void NewGame()
    {
        Debug.Log("Starting a New Game...");
        
        if (DialogueManager.Instance != null)
        {
            // --- THIS IS THE FIX ---
            // The StartDialogue function only needs the name of the conversation file.
            // The .json part is added automatically by the DialogueManager.
            DialogueManager.Instance.StartDialogue("TestDialogue");
        }
        else
        {
            Debug.LogError("DialogueManager not found!");
        }
    }

    // This function will be called by the "Load Game" button.
    public void LoadGame()
    {
        Debug.Log("Opening the Load Game screen...");
        // In a real game, this would likely open another UI panel with save slots.
    }

    // This function will be called by the "Options" button.
    public void OpenOptions()
    {
        Debug.Log("Opening the Options menu...");
        // This would open your settings/options UI panel.
    }

    // This function will be called by the "Quit" button.
    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        // This command only works in a built game, not in the Unity Editor.
        Application.Quit();
    }
}
