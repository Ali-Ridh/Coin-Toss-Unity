// FILE: CustomerTypeData.cs
// PURPOSE: Defines the properties for a single type of customer.
// This is just a data structure, it does not get attached to any GameObject.
using UnityEngine;

[System.Serializable]
public class CustomerTypeData
{
    public string customerTypeID; // e.g., "Regular", "Patient", "Demanding"
    public string description;

    [Range(0.5f, 2.0f)]
    public float patienceMultiplier = 1.0f; // e.g., 1.5 for patient, 0.75 for impatient

    public int minOrderSize = 1;
    public int maxOrderSize = 1;

    [Range(0, 100)]
    public int spawnWeight = 50; // A higher number means they are more likely to spawn
}
