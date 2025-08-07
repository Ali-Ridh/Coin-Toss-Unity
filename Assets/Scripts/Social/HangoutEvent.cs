using UnityEngine;

[System.Serializable]
public class HangoutEvent
{
    public string eventID;
    public string eventName;
    public string description;
    public string companionID;
    public int requiredRank;
    public string[] dialogueNodes; // Array of dialogue node IDs for this event
    public string[] outcomes; // Possible outcomes of this event
    public string sceneToLoad; // Optional scene to load for this event
}