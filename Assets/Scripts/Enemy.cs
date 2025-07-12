using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Public variable to set the enemy's health in the Inspector.
    // Default is 100.
    public float HP = 100f;

    // A public function that other objects can call to deal damage to this enemy.
    public void TakeDamage(float damageAmount)
    {
        // Subtract the damage amount from the current HP.
        HP -= damageAmount;

        // Print the remaining HP to the console for debugging.
        Debug.Log(gameObject.name + " took " + damageAmount + " damage, remaining HP: " + HP);

        // Check if the enemy's health has dropped to 0 or below.
        if (HP <= 0)
        {
            // If health is gone, call the Die() function.
            Die();
        }
    }

    // This function handles what happens when the enemy dies.
    private void Die()
    {
        // Print a death message to the console for debugging.
        Debug.Log(gameObject.name + " has been defeated!");

        // Destroy the GameObject this script is attached to, removing it from the scene.
        Destroy(gameObject);
    }
}
