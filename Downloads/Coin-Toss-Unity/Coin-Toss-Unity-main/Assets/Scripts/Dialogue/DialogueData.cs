// FILE: DialogueData.cs
// PURPOSE: Defines the data structures to match the dialogue JSON format.
// NOTE: This is not a MonoBehaviour and does not get attached to any GameObjects.
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string dialogueText;
    public int emotionIndex;
    public string background;
    public string voiceClip;
    public string leftPortrait; // Path to left portrait sprite in Resources
    public string rightPortrait; // Path to right portrait sprite in Resources
    public string speakerPosition; // "left" or "right"
    public List<Choice> choices;
    public int actionType;
}

// PortraitPosition class removed

[System.Serializable]
public class Choice
{
    public string choiceText;
    public string nextDialogue;
    public int nextLineIndex;
}
