// Defines a single piece of gossip the player can collect.
using UnityEngine;

[System.Serializable]
public class GossipSnippet
{
    public string gossipID;
    [TextArea(3, 5)] // Makes the text box bigger in the Inspector
    public string gossipText;
    public string characterWhoSaysIt; // Optional: for specific characters
    public int infoValue; // How much it fills the information-meter
}
