// FILE: GameStateManager.cs
// PURPOSE: The highest-level manager. Controls the overall flow of the game.
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState { PreOpening, DinerShift, EndOfDay }
    public GameState currentState { get; private set; }

    public static GameStateManager Instance;

    public GameObject dinerManagerObject; // Assign the GameObject holding the DinerManager
    public GameObject upgradeShopPanel; // Assign the main UI panel for the shop

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Start the game in the preparation phase
        TransitionToState(GameState.PreOpening);
    }

    public void TransitionToState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.PreOpening:
                // Activate the shop, deactivate the diner gameplay
                dinerManagerObject.SetActive(false);
                upgradeShopPanel.SetActive(true);
                UIManager.Instance.SetGameStateButtonText("START DAY");
                UIManager.Instance.log.LogActivity("Prepare for the day ahead. Open the shop to buy upgrades.");
                break;

            case GameState.DinerShift:
                // Deactivate the shop, activate the diner gameplay
                upgradeShopPanel.SetActive(false);
                dinerManagerObject.SetActive(true);
                UIManager.Instance.SetGameStateButtonText("DAY IN PROGRESS...");
                break;

            case GameState.EndOfDay:
                // This state would show a summary screen before transitioning back to PreOpening
                Debug.Log("Day has ended!");
                // For now, we'll go straight back to the pre-opening phase for the next day
                TransitionToState(GameState.PreOpening);
                break;
        }
    }

    // This function will be called by the "START DAY" button in the UI
    public void OnStartDayClicked()
    {
        if (currentState == GameState.PreOpening)
        {
            TransitionToState(GameState.DinerShift);
        }
    }
}
