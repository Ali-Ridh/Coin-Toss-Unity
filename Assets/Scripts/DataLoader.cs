using UnityEngine;
using System.Collections.Generic;
using System.IO; // Diperlukan untuk membaca file

// Ini adalah kelas pembantu (helper class) kecil yang diperlukan
// agar JsonUtility bisa membaca array dari file JSON.
[System.Serializable]
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
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

    // Properti publik untuk menyimpan semua data yang sudah di-load.
    // Manager lain bisa mengakses data ini melalui DataLoader.Instance
    public List<MenuItem> AllMenuItems { get; private set; }
    public List<UpgradeData> AllUpgrades { get; private set; }
    public List<RankData> AllRankData { get; private set; }
    public List<DailyScheduleEntry> FullSchedule { get; private set; }
    
    // Kita akan menggunakan Dictionary untuk menyimpan data gosip dan hangout
    // agar mudah diakses berdasarkan nama topiknya atau ID companion.
    public Dictionary<string, List<GossipSnippet>> AllGossipTopics { get; private set; }
    public Dictionary<string, List<HangoutEvent>> AllHangoutEvents { get; private set; }


    void Awake()
    {
        // Setup Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Pastikan DataLoader tidak hancur saat ganti scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inisialisasi semua list dan dictionary
        AllMenuItems = new List<MenuItem>();
        AllUpgrades = new List<UpgradeData>();
        AllRankData = new List<RankData>();
        FullSchedule = new List<DailyScheduleEntry>();
        AllGossipTopics = new Dictionary<string, List<GossipSnippet>>();
        AllHangoutEvents = new Dictionary<string, List<HangoutEvent>>();

        // Mulai proses loading data
        LoadAllGameData();
    }

    private void LoadAllGameData()
    {
        Debug.Log("Starting to load all game data...");

        // Load data yang sederhana
        AllMenuItems.AddRange(LoadJsonData<MenuItem>("MenuItems.json"));
        AllUpgrades.AddRange(LoadJsonData<UpgradeData>("Upgrades.json"));
        AllRankData.AddRange(LoadJsonData<RankData>("RankData.json"));
        FullSchedule.AddRange(LoadJsonData<DailyScheduleEntry>("Schedule.json"));

        // --- Contoh cara me-load data yang lebih kompleks (seperti gosip) ---
        // Anda perlu mengulangi proses ini untuk semua file gosip dan hangout Anda.
        // Ini bisa diotomatisasi lebih lanjut nanti.
        LoadGossipTopic("Gossip_Topic_Template.json");
        LoadHangoutEvents("Companion_Hangouts_Template.json");

        Debug.Log("All game data loaded successfully!");
    }

    private T[] LoadJsonData<T>(string fileName)
    {
        // Path ke folder StreamingAssets
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        
        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            // Gunakan JsonHelper untuk mengubah array JSON menjadi objek C#
            T[] data = JsonHelper.FromJson<T>(jsonContent);
            Debug.Log("Successfully loaded " + data.Length + " entries from " + fileName);
            return data;
        }
        else
        {
            Debug.LogError("Cannot find file: " + fileName + " at path: " + path);
            return new T[0]; // Kembalikan array kosong jika file tidak ditemukan
        }
    }

    // Fungsi khusus untuk me-load file gosip dan menyimpannya di Dictionary
    private void LoadGossipTopic(string fileName)
    {
        string topicName = Path.GetFileNameWithoutExtension(fileName); // misal: "Gossip_Topic_Template"
        var gossipData = LoadJsonData<GossipSnippet>(fileName);
        if (gossipData.Length > 0)
        {
            AllGossipTopics[topicName] = new List<GossipSnippet>(gossipData);
        }
    }

    // Fungsi khusus untuk me-load file hangout dan menyimpannya di Dictionary
    private void LoadHangoutEvents(string fileName)
    {
        string companionID = Path.GetFileNameWithoutExtension(fileName); // misal: "Companion_Hangouts_Template"
        var hangoutData = LoadJsonData<HangoutEvent>(fileName);
        if (hangoutData.Length > 0)
        {
            AllHangoutEvents[companionID] = new List<HangoutEvent>(hangoutData);
        }
    }
}
