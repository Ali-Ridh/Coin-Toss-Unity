// FILE: Station.cs
// PURPOSE: A simple component to identify a station and its type.
using UnityEngine;

public class Station : MonoBehaviour
{
    public enum StationType { Queue, Kitchen, Dishes, TeaStand }
    public StationType type;

    // In the new system, this script has no functions.
    // Its only job is to hold the 'type' variable so the
    // PlayerController knows what it has collided with.
}
