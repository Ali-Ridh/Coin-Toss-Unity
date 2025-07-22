// PURPOSE: Mendefinisikan struktur lengkap dari satu event hangout.
// NOTE: Ini  data game yang statis, dimuat dari JSON.
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HangoutEvent
{
    public string eventID;
    public int requiredRank; // Rank yang dibutuhkan untuk memulai event ini
    public List<DialogueLine> dialogue;
    public int basePointsAwarded;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerID; // ID karakter yang berbicara (misal "Player", "CompanionA")
    [TextArea(3, 5)]
    public string text;
    public List<DialogueChoice> choices; // Daftar pilihan jika ada
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int pointsValue; // Poin yang didapat dari pilihan ini
    public bool triggersRomance; // Flag khusus untuk keputusan romansa
}
