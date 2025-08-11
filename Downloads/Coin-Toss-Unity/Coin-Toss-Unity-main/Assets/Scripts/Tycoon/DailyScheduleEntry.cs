// FILE: DailyScheduleEntry.cs
// PURPOSE: Defines all the events and parameters for a single day in the game.
using UnityEngine;

[System.Serializable]
public class DailyScheduleEntry
{
    public int day;
    public string dayOfWeek;
    public int customerCount; // The number of customers to spawn this day
    public string availableCompanionID;
    public string gossipTopic;
    public string specialEvent;
}
