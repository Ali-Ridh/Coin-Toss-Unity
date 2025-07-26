using UnityEngine;
using System.Collections.Generic; // --- ADD THIS LINE ---

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private List<UpgradeData> allUpgrades;

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
        // Get the list of all possible upgrades from the DataLoader
        allUpgrades = DataLoader.Instance.AllUpgrades;
    }

    // This is the main function that the UI buttons will call.
    public void PurchaseUpgrade(string upgradeID)
    {
        // 1. Find the upgrade data from the master list.
        UpgradeData upgradeToPurchase = allUpgrades.Find(u => u.upgradeID == upgradeID);
        if (upgradeToPurchase == null)
        {
            Debug.LogError("Attempted to purchase an invalid upgrade with ID: " + upgradeID);
            return;
        }

        // 2. Check if the player can afford it.
        if (PlayerProgressManager.Instance.CanAfford(upgradeToPurchase.cost))
        {
            // 3. Subtract the money.
            PlayerProgressManager.Instance.SpendMoney(upgradeToPurchase.cost);

            // 4. Unlock the content associated with the upgrade.
            if (!string.IsNullOrEmpty(upgradeToPurchase.unlocksMenuItemID))
            {
                PlayerProgressManager.Instance.UnlockMenuItem(upgradeToPurchase.unlocksMenuItemID);
            }
            if (!string.IsNullOrEmpty(upgradeToPurchase.unlocksStationID))
            {
                PlayerProgressManager.Instance.UnlockStation(upgradeToPurchase.unlocksStationID);
                // You would also add logic here to physically place the new station in the diner.
            }

            Debug.Log("Successfully purchased upgrade: " + upgradeToPurchase.upgradeName);
            // Here you would refresh the Upgrade Shop UI to show the item as "Purchased".
        }
        else
        {
            Debug.Log("Cannot afford " + upgradeToPurchase.upgradeName + ". Needs " + upgradeToPurchase.cost + " money.");
            // Here you could show a "Not enough money!" message to the player.
        }
    }
}
