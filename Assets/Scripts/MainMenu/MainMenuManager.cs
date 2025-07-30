using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class MainMenuManager : MonoBehaviour
{
    // This function will be called by the "New Game" button.
    public void NewGame()
    {
        Debug.Log("Starting a New Game...");
        // Replace "YourGameSceneName" with the actual name of your main gameplay scene.
        // For example, "Day1_Scene" or "DinerScene".
        SceneManager.LoadScene("SampleScene"); 
    }

    // This function will be called by the "Load Game" button.
    public void LoadGame()
    {
        Debug.Log("Opening the Load Game screen...");
        // In a real game, this would likely open another UI panel with save slots.
        // For now, we'll just log a message.
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