// FILE: DataLoader.cs
// PURPOSE: A simplified loader that ONLY reads the daily schedule.
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DataLoader : MonoBehaviour
{
    // --- THIS IS THE FIX ---
    // The public static Instance variable was missing.
    public static DataLoader Instance;

    public List<DailyScheduleEntry> FullSchedule { get; private set; }
    public bool isDataLoaded { get; private set; } = false;

    void Awake()
    {
        // --- THIS IS THE FIX ---
        // The logic to set the singleton instance was missing.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        FullSchedule = new List<DailyScheduleEntry>();
        LoadScheduleData();
    }

    private void LoadScheduleData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Schedule.json");
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            DailyScheduleEntry[] schedule = JsonHelper.FromJson<DailyScheduleEntry>(jsonContent);
            FullSchedule.AddRange(schedule);
            isDataLoaded = true;
            Debug.Log("Successfully loaded " + FullSchedule.Count + " days from Schedule.json");
        }
        else
        {
            Debug.LogError("Schedule.json not found in StreamingAssets!");
        }
    }
}
