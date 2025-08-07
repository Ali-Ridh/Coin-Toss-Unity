using UnityEngine;
using System.Collections.Generic;
using System.IO;

// ... (JsonHelper tetap sama) ...


[System.Serializable]
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        // --- THIS IS THE FIX ---
        // We manually add the "wrapper" around the JSON array string.
        // This turns "[...]" into "{ "Items": [...] }", which JsonUtility can understand.
        string newJson = "{ \"Items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public class DataLoader : MonoBehaviour
{
    public static DataLoader Instance;

    // --- BARU ---
    [Header("Asset Databases")]
    public SpriteDatabase spriteDatabase; // Tugaskan aset SpriteDatabase Anda di sini melalui Inspector.

    public bool isDataLoaded { get; private set; } = false;

    public List<MenuItem> AllMenuItems { get; private set; }
    public List<UpgradeData> AllUpgrades { get; private set; }
    public List<RankData> AllRankData { get; private set; }
    public List<DailyScheduleEntry> FullSchedule { get; private set; }
    public List<CustomerTypeData> AllCustomerTypes { get; private set; }
    
    public Dictionary<string, List<GossipSnippet>> AllGossipTopics { get; private set; }
    public Dictionary<string, List<HangoutEvent>> AllHangoutEvents { get; private set; }

    void Awake()
    {
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

        AllMenuItems = new List<MenuItem>();
        AllUpgrades = new List<UpgradeData>();
        AllRankData = new List<RankData>();
        FullSchedule = new List<DailyScheduleEntry>();
        AllCustomerTypes = new List<CustomerTypeData>();
        AllGossipTopics = new Dictionary<string, List<GossipSnippet>>();
        AllHangoutEvents = new Dictionary<string, List<HangoutEvent>>();

        LoadAllGameData();
    }

    // private void LoadAllGameData()
    // {
    //     Debug.Log("Starting to load all game data...");

    //     AllMenuItems.AddRange(LoadJsonData<MenuItem>("MenuItems.json"));
    //     AllUpgrades.AddRange(LoadJsonData<UpgradeData>("Upgrades.json"));
    //     AllRankData.AddRange(LoadJsonData<RankData>("RankData.json"));
    //     FullSchedule.AddRange(LoadJsonData<DailyScheduleEntry>("Schedule.json"));
    //     AllCustomerTypes.AddRange(LoadJsonData<CustomerTypeData>("CustomerTypes.json"));

    //     LoadGossipTopic("Gossip_Topic_Template.json");
    //     LoadHangoutEvents("Companion_Hangouts_Template.json");

    //     isDataLoaded = true;
    //     Debug.Log("All game data loaded successfully!");
    // }
        private void LoadAllGameData()
    {
        Debug.Log("Starting to load all game data...");

        // 1. Muat semua data dari JSON seperti biasa.
        AllMenuItems.AddRange(LoadJsonData<MenuItem>("MenuItems.json"));
        AllUpgrades.AddRange(LoadJsonData<UpgradeData>("Upgrades.json"));
        AllMenuItems.AddRange(LoadJsonData<MenuItem>("MenuItems.json"));
        AllUpgrades.AddRange(LoadJsonData<UpgradeData>("Upgrades.json"));
        AllRankData.AddRange(LoadJsonData<RankData>("RankData.json"));
        FullSchedule.AddRange(LoadJsonData<DailyScheduleEntry>("Schedule.json"));
        AllCustomerTypes.AddRange(LoadJsonData<CustomerTypeData>("CustomerTypes.json"));

        LoadGossipTopic("Gossip_Topic_Template.json");
        LoadHangoutEvents("Companion_Hangouts_Template.json");

        // 2. --- LANGKAH PENTING BARU ---
        // Setelah data teks dimuat, kita sekarang menautkan aset Sprite.
        LinkMenuItemSprites();

        isDataLoaded = true;
        Debug.Log("All game data loaded successfully!");
    }
        private void LinkMenuItemSprites()
    {
        if (spriteDatabase == null)
        {
            Debug.LogError("SpriteDatabase is not assigned in the DataLoader Inspector!");
            return;
        }

        Debug.Log("Linking menu item sprites...");
        foreach (MenuItem item in AllMenuItems)
        {
            // Gunakan database untuk menemukan dan menugaskan sprite yang benar
            // ke setiap menu item berdasarkan itemID-nya.
            item.itemIcon = spriteDatabase.GetSpriteByID(item.itemID);
        }
    }
    private T[] LoadJsonData<T>(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            T[] data = JsonHelper.FromJson<T>(jsonContent);
            return data;
        }
        else
        {
            Debug.LogError("Cannot find file: " + fileName + " at path: " + path);
            return new T[0];
        }
    }

    private void LoadGossipTopic(string fileName)
    {
        string topicName = Path.GetFileNameWithoutExtension(fileName);
        var gossipData = LoadJsonData<GossipSnippet>(fileName);
        if (gossipData.Length > 0)
        {
            AllGossipTopics[topicName] = new List<GossipSnippet>(gossipData);
        }
    }

    private void LoadHangoutEvents(string fileName)
    {
        string companionID = Path.GetFileNameWithoutExtension(fileName);
        var hangoutData = LoadJsonData<HangoutEvent>(fileName);
        if (hangoutData.Length > 0)
        {
            AllHangoutEvents[companionID] = new List<HangoutEvent>(hangoutData);
        }
    }
}
