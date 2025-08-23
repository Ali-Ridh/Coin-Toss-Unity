// FILE: UpgradeManager.cs
// PURPOSE: Handles the logic for purchasing upgrades.
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public void PurchaseAddTable()
    {
        int cost = 100; // You can move this to a JSON file later
        if (GameManager.Instance != null && GameManager.Instance.SpendMoney(cost))
        {
            if (DinerManager.Instance.CanAddTable())
            {
                DinerManager.Instance.AddNewTable(); // Tell the DinerManager to place the table
                UIManager.Instance.log.LogActivity("Purchased a new table!");
                UIManager.Instance.UpdateEarningsDisplay();
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
        if (GameManager.Instance != null && GameManager.Instance.SpendMoney(cost))
        {
            PlayerProgressManager.Instance.cookTimeReduction = 2f;
            UIManager.Instance.log.LogActivity("Kitchen upgraded! Cooking is faster.");
            UIManager.Instance.UpdateEarningsDisplay();
        }
        else
        {
            UIManager.Instance.log.LogActivity("Not enough money for kitchen upgrade.", "text-red-400");
        }
    }
}
