// FILE: UpgradeManager.cs
// PURPOSE: Handles the logic for purchasing upgrades.
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public void PurchaseAddTable()
    {
        int cost = 100; // You can move this to a JSON file later
        if (PlayerProgressManager.Instance.earnings >= cost)
        {
            // Ask the DinerManager if a new table can be added
            if (DinerManager.Instance.CanAddTable())
            {
                PlayerProgressManager.Instance.AddEarnings(-cost);
                DinerManager.Instance.AddNewTable(); // Tell the DinerManager to place the table
                UIManager.Instance.log.LogActivity("Purchased a new table!");
            }
            else
            {
                UIManager.Instance.log.LogActivity("All table spots are already full!");
            }
        }
        else
        {
            UIManager.Instance.log.LogActivity("Not enough money for a new table.", "text-red-400");
        }
    }

    public void PurchaseKitchenUpgrade()
    {
        int cost = 150;
        if (PlayerProgressManager.Instance.earnings >= cost)
        {
            PlayerProgressManager.Instance.AddEarnings(-cost);
            PlayerProgressManager.Instance.cookTimeReduction = 2f;
            UIManager.Instance.log.LogActivity("Kitchen upgraded! Cooking is faster.");
        }
    }
}
