using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGameManager : MonoBehaviour
{
    public enum GameState { PlayerTurn, EnemyTurn, Waiting }
    public GameState currentState;

    public static CoinGameManager Instance;

    public DragNShoot player;
    public List<EnemyAI> enemies;

    // --- NEW --- A flag to prevent the enemy turn from starting multiple times.
    private bool isEnemyTurnRunning = false;

    void Awake()
    {
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
        StartPlayerTurn();
    }

    // --- PLAYER TURN ---
    public void StartPlayerTurn()
    {
        Debug.Log("--- Player Turn Started ---");
        currentState = GameState.PlayerTurn;
        player.enabled = true;
        // Update UI
        GameUIManager.Instance?.UpdateTurnStatus(currentState);
    }

    public void EndPlayerTurn()
    {
        Debug.Log("--- Player Turn Ended ---");
        player.enabled = false;
        currentState = GameState.Waiting;
        // Update UI
        GameUIManager.Instance?.UpdateTurnStatus(currentState);
        StartCoroutine(TransitionToEnemyTurn());
    }

    // --- ENEMY TURN ---
    public void StartEnemyTurn()
    {
        // --- MODIFIED --- Add a guard clause to prevent re-entry.
        if (isEnemyTurnRunning)
        {
            Debug.LogWarning("Attempted to start Enemy Turn while it was already running.");
            return;
        }

        Debug.Log("--- Enemy Turn Started ---");
        currentState = GameState.EnemyTurn;
        // Update UI
        GameUIManager.Instance?.UpdateTurnStatus(currentState);
        StartCoroutine(EnemyTurnRoutine());
    }

    public void EndEnemyTurn()
    {
        Debug.Log("--- Enemy Turn Ended ---");
        // Update UI
        GameUIManager.Instance?.UpdateTurnStatus(GameState.PlayerTurn);
        StartPlayerTurn();
    }

    // --- TRANSITION LOGIC ---
    private IEnumerator TransitionToEnemyTurn()
    {
        Debug.Log("Waiting for all coins to stop moving...");
        yield return StartCoroutine(WaitForAllCoinsToSleep());
        Debug.Log("All coins are sleeping. Starting Enemy Turn.");
        StartEnemyTurn();
    }

    private IEnumerator EnemyTurnRoutine()
    {
        // --- NEW --- Set the flag to true to "lock" this routine.
        isEnemyTurnRunning = true;

        CleanUpEnemiesList();

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log(enemies[i].name + " is taking its turn.");
                enemies[i].TakeTurn();

                yield return StartCoroutine(WaitForAllCoinsToSleep());
                Debug.Log("All coins are sleeping after " + enemies[i].name + "'s move.");
            }
        }

        // --- NEW --- Clear the flag to "unlock" the routine for the next turn.
        isEnemyTurnRunning = false;
        EndEnemyTurn();
    }

    // --- NEW, MORE ROBUST WAITING COROUTINE ---
    private IEnumerator WaitForAllCoinsToSleep()
    {
        float waitTimer = 0f;
        float maxWaitTime = 10f; // Max 10 seconds to wait for coins to stop.

        while (!AreAllCoinsSleeping())
        {
            if (waitTimer >= maxWaitTime)
            {
                Debug.LogWarning("Wait for sleep timed out! Forcing turn to continue.");
                break;
            }

            waitTimer += Time.deltaTime;
            yield return null;
        }
    }

    // --- HELPER FUNCTIONS ---
    private bool AreAllCoinsSleeping()
    {
        if (player != null && !player.rb.IsSleeping())
        {
            return false;
        }
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null && !enemy.rb.IsSleeping())
            {
                return false;
            }
        }
        return true;
    }

    private void CleanUpEnemiesList()
    {
        enemies.RemoveAll(item => item == null);
    }


    // Add this method for starting the battle from UI
    public void StartBattle()
    {
        StartPlayerTurn();
    }

    // --- NEW: Call this when the battle ends to progress the day cycle ---
    public void EndBattle()
    {
        Debug.Log("Battle Ended. Progressing to next step in day cycle.");
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.NextStep();
        }
    }

    public void RemoveEnemy(EnemyAI enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }
}
