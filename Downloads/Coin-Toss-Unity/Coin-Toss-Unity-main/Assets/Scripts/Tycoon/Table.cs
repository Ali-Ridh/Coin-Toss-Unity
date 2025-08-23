// FILE: Table.cs
// PURPOSE: Manages the state of a single table and its visual effects.
using System.Collections;
using UnityEngine;

public class Table : MonoBehaviour
{
    public Transform customerSeat;
    public bool IsOccupied { get; private set; }
    public CustomerController currentCustomer { get; private set; }

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Start()
    {
        originalPosition = transform.position;
    }

    // This function starts the shake visual effect
    public void Shake()
    {
        Debug.Log($"[Table] Shake started for {gameObject.name}");
        // Stop any previous shake to prevent conflicts
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.position = originalPosition; // Reset position
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Debug.Log($"[Table] ShakeCoroutine started for {gameObject.name}");
        float duration = 0.4f;
        float magnitude = 0.1f;
        float elapsed = 0.0f;
        float maxDuration = 2.0f; // Safety timeout
        while (elapsed < duration)
        {
            if (elapsed > maxDuration)
            {
                Debug.LogWarning($"[Table] ShakeCoroutine timed out for {gameObject.name}");
                break;
            }
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        // Snap back to the original position when done
        transform.position = originalPosition;
        Debug.Log($"[Table] ShakeCoroutine ended for {gameObject.name}");
    }

    public void SeatCustomer(CustomerController customer)
    {
        IsOccupied = true;
        currentCustomer = customer;
        customer.OnSeated(this);
    }

    public void OnCustomerLeave()
    {
        IsOccupied = false;
        currentCustomer = null;
    }
}