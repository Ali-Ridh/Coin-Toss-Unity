// FILE: GameStateManager.cs
// PURPOSE: The highest-level manager. Controls game flow and holds safe references.
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState { PreOpening, DinerShift, EndOfDay }
    public GameState currentState { get; private set; }

    public static GameStateManager Instance;

    [Header("Core Managers")]
    // --- THIS IS THE FIX ---
    // Assign your UIManager GameObject here in the Inspector.
    public UIManager uiManager; 

    [Header("Scene Objects")]
    public GameObject dinerManagerObject;
    public GameObject upgradeShopPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Safety check to ensure the UIManager is assigned.
        if (uiManager == null)
        {
            Debug.LogError("FATAL ERROR: UIManager has not been assigned in the GameStateManager Inspector!");
            this.enabled = false;
        }
    }

    void Start()
    {
        TransitionToState(GameState.PreOpening);
    }

    public void TransitionToState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.PreOpening:
                dinerManagerObject.SetActive(false);
                upgradeShopPanel.SetActive(true);
                uiManager.SetGameStateButtonText("START DAY");
                uiManager.log.LogActivity("Prepare for the day ahead. Open the shop to buy upgrades.");
                break;

            case GameState.DinerShift:
                upgradeShopPanel.SetActive(false);
                dinerManagerObject.SetActive(true);
                uiManager.SetGameStateButtonText("DAY IN PROGRESS...");
                break;

            case GameState.EndOfDay:
                Debug.Log("Day has ended!");
                TransitionToState(GameState.PreOpening);
                break;
        }
    }

    public void OnStartDayClicked()
    {
        if (currentState == GameState.PreOpening)
        {
            TransitionToState(GameState.DinerShift);
        }
    }
}
