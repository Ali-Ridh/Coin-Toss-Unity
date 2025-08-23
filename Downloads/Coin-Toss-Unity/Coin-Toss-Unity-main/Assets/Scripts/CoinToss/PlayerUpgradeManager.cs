using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance;

    private const string DamageKey = "PlayerDamage";
    private const string ForceKey = "PlayerForceMultiplier";

    public float PlayerDamage => PlayerPrefs.GetFloat(DamageKey, 25f);
    public float ForceMultiplier => PlayerPrefs.GetFloat(ForceKey, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool UpgradeDamage(float amount, int cost)
    {
        if (GameManager.Instance != null && GameManager.Instance.SpendMoney(cost))
        {
            float newDamage = PlayerDamage + amount;
            PlayerPrefs.SetFloat(DamageKey, newDamage);
            GameUIManager.Instance?.ShowUpgradeResult($"Upgraded Damage for ${cost}. New Damage: {newDamage}", true);
            GameUIManager.Instance?.UpdateMoneyDisplay();
            return true;
        }
        GameUIManager.Instance?.ShowUpgradeResult($"Not enough money to upgrade Damage!", false);
        return false;
    }

    public bool UpgradeForce(float amount, int cost)
    {
        if (GameManager.Instance != null && GameManager.Instance.SpendMoney(cost))
        {
            float newForce = ForceMultiplier + amount;
            PlayerPrefs.SetFloat(ForceKey, newForce);
            GameUIManager.Instance?.ShowUpgradeResult($"Upgraded Force for ${cost}. New Force: {newForce}", true);
            GameUIManager.Instance?.UpdateMoneyDisplay();
            return true;
        }
        GameUIManager.Instance?.ShowUpgradeResult($"Not enough money to upgrade Force!", false);
        return false;
    }

    // Example: Call this from a button in the Inspector, set values as needed
    public void OnUpgradeDamageButton(float amount, int cost)
    {
        UpgradeDamage(amount, cost);
    }

    public void OnUpgradeForceButton(float amount, int cost)
    {
        UpgradeForce(amount, cost);
    }

    // Optional: If you want fixed upgrades for buttons, add methods like:
    public void UpgradeDamageButton()
    {
        UpgradeDamage(5f, 100);
    }

    public void UpgradeForceButton()
    {
        UpgradeForce(0.2f, 150);
    }

    // Make sure this is public, non-static, and has no parameters
    public void CloseUpgradePanelAndStartBattle()
    {
        GameUIManager.Instance?.HideUpgradePanel();
        CoinGameManager.Instance?.StartBattle();
    }
}