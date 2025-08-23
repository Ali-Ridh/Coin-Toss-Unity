using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    // ...existing code...
    public static GameUIManager Instance;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI upgradeResultText;
    public TextMeshProUGUI playerHealthText;
    public GameObject enemyHealthContainer; // Should have VerticalLayoutGroup
    public GameObject enemyHealthEntryPrefab; // Prefab with TextMeshProUGUI for enemy health
    public TextMeshProUGUI turnStatusText;
    public GameObject upgradePanel; // Assign in Inspector

    private Dictionary<EnemyAI, GameObject> enemyEntries = new Dictionary<EnemyAI, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            int money = GameManager.Instance != null ? GameManager.Instance.Money : 0;
            moneyText.text = $"Money: ${money}";
        }
    }

    public void ShowUpgradeResult(string message, bool success)
    {
        if (upgradeResultText != null)
        {
            upgradeResultText.text = message;
            upgradeResultText.color = success ? Color.green : Color.red;
        }
    }
    public void UpdatePlayerHealth(float hp)
    {
        if (playerHealthText != null)
            playerHealthText.text = $"Player HP: {hp:0}";
    }

    public void UpdateEnemyHealth(EnemyAI changedEnemy = null)
    {
        if (enemyHealthContainer == null || enemyHealthEntryPrefab == null) return;
        var enemies = CoinGameManager.Instance?.enemies;
        if (enemies == null) return;

        // Remove entries for destroyed enemies
        var keysToRemove = new List<EnemyAI>();
        foreach (var kvp in enemyEntries)
        {
            if (!enemies.Contains(kvp.Key) || kvp.Key == null)
            {
                Destroy(kvp.Value);
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            enemyEntries.Remove(key);
        }

        // Add/update entries for current enemies
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (!enemyEntries.ContainsKey(enemy))
            {
                var entry = Instantiate(enemyHealthEntryPrefab, enemyHealthContainer.transform);
                enemyEntries[enemy] = entry;
            }
            var entryText = enemyEntries[enemy].GetComponentInChildren<TextMeshProUGUI>();
            if (entryText != null)
            {
                entryText.text = $"{enemy.name}: {enemy.HP:0}";
            }
        }
    }

    public void UpdateTurnStatus(CoinGameManager.GameState state)
    {
        if (turnStatusText != null)
            turnStatusText.text = $"Turn: {state}";
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    // Optionally, call this in Start to initialize UI
    void Start()
    {
        UpdateMoneyDisplay();
        UpdatePlayerHealth(CoinGameManager.Instance?.player?.HP ?? 0);
        UpdateEnemyHealth();
        UpdateTurnStatus(CoinGameManager.Instance?.currentState ?? CoinGameManager.GameState.Waiting);
    }
}
