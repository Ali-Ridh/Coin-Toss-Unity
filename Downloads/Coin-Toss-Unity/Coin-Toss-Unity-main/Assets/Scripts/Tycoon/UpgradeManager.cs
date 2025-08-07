// FILE: UpgradeManager.cs
// PURPOSE: Handles the logic for purchasing upgrades.
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public void PurchaseAddTable()
    {
        int cost = 100;
        if (PlayerProgressManager.Instance.earnings >= cost)
        {
            PlayerProgressManager.Instance.AddEarnings(-cost);
            // Logic to instantiate a new table prefab
            UIManager.Instance.log.LogActivity("Purchased a new table!");
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
