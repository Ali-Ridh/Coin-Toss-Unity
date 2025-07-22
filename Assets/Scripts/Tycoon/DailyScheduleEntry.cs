//Defines all the events and parameters for a single day in the game.
using UnityEngine;

[System.Serializable]
public class DailyScheduleEntry
{
    public int day;
    public string dayOfWeek;
    public string availableCompanionID; // The ID of the companion available to hang out with
    public string gossipTopic; // The name of the gossip JSON file to load for this day
    public string specialEvent; // e.g., "none", "RivalDinerOpens", "MajorEvaluation"
}
// This class can be used to define the structure of a daily schedule in the game,
// including the day number, day of the week, available companions, gossip topics, and special events.  
