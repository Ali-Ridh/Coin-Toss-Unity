using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// DialogueManager is a singleton MonoBehaviour that handles the visual novel dialogue system.
/// It loads JSON dialogue files, tracks conversation state, processes player choices,
/// and executes end actions like scene loading.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // Singleton instance
    private static DialogueManager _instance;
    
    // Event for notifying the UI when a dialogue node changes
    public static event Action<DialogueNode> OnDialogueNodeChanged;
    
    // Event for notifying the UI when choices are available
    public static event Action<List<DialogueChoice>> OnChoicesAvailable;
    
    // Dictionary to store loaded dialogue nodes by their ID
    private Dictionary<string, DialogueNode> dialogueNodes;
    
    // Current dialogue state
    private string currentDialogueId;
    private DialogueNode currentNode;
    private Stack<string> dialogueHistory;
    
    // Path to the StreamingAssets folder
    private string streamingAssetsPath;
    
    /// <summary>
    /// Gets the singleton instance of the DialogueManager.
    /// </summary>
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindObjectOfType<DialogueManager>();
                
                // If not found, create a new GameObject with the DialogueManager component
                if (_instance == null)
                {
                    GameObject managerObject = new GameObject("DialogueManager");
                    _instance = managerObject.AddComponent<DialogueManager>();
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Initializes the DialogueManager.
    /// </summary>
    void Awake()
    {
        // Ensure only one instance exists
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize variables
            dialogueNodes = new Dictionary<string, DialogueNode>();
            dialogueHistory = new Stack<string>();
            streamingAssetsPath = Application.streamingAssetsPath;
        }
        else if (_instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Loads a dialogue from a JSON file in the StreamingAssets folder.
    /// </summary>
    /// <param name="fileName">The name of the JSON file to load.</param>
    public void LoadDialogue(string fileName)
    {
        try
        {
            string path = Path.Combine(streamingAssetsPath, fileName);
            
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                
                // Parse the JSON content
                DialogueNode[] nodes = JsonHelper.FromJson<DialogueNode>(jsonContent);
                
                // Clear existing nodes and populate with new ones
                dialogueNodes.Clear();
                foreach (var node in nodes)
                {
                    if (!string.IsNullOrEmpty(node.nodeId))
                    {
                        dialogueNodes[node.nodeId] = node;
                    }
                    else
                    {
                        Debug.LogWarning("Dialogue node found with empty nodeId in file: " + fileName);
                    }
                }
                
                Debug.Log("Successfully loaded dialogue file: " + fileName);
            }
            else
            {
                Debug.LogError("Dialogue file not found: " + path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading dialogue file: " + e.Message);
        }
    }
    
    /// <summary>
    /// Starts a dialogue sequence from a specific node.
    /// </summary>
    /// <param name="fileName">The JSON file containing the dialogue.</param>
    /// <param name="startNodeId">The ID of the starting dialogue node.</param>
    public void StartDialogue(string fileName, string startNodeId)
    {
        // Load the dialogue file
        LoadDialogue(fileName);
        
        // Check if the start node exists
        if (dialogueNodes.ContainsKey(startNodeId))
        {
            currentDialogueId = startNodeId;
            currentNode = dialogueNodes[startNodeId];
            dialogueHistory.Clear();
            
            // Process the current node
            ProcessCurrentNode();
        }
        else
        {
            Debug.LogError("Start node ID not found in dialogue: " + startNodeId);
        }
    }
    
    /// <summary>
    /// Advances to the next dialogue node in a linear conversation.
    /// </summary>
    public void NextNode()
    {
        // If there are choices, we shouldn't automatically advance
        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            // Present choices to the player
            PresentChoices(currentNode.choices);
            return;
        }
        
        // If there's a next node specified, go to it
        if (!string.IsNullOrEmpty(currentNode.nextNode))
        {
            TransitionToNode(currentNode.nextNode);
        }
        else
        {
            // If no next node and no choices, end the dialogue
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Selects a choice and transitions to the appropriate node.
    /// </summary>
    /// <param name="choiceIndex">The index of the selected choice.</param>
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode.choices != null && choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
        {
            // Record current node in history
            dialogueHistory.Push(currentNode.nodeId);
            
            // Get the next node ID from the choice
            string nextNodeId = currentNode.choices[choiceIndex].nextNode;
            
            // Transition to the next node
            TransitionToNode(nextNodeId);
        }
        else
        {
            Debug.LogError("Invalid choice index: " + choiceIndex);
        }
    }
    
    /// <summary>
    /// Transitions to a specific dialogue node.
    /// </summary>
    /// <param name="nodeId">The ID of the node to transition to.</param>
    private void TransitionToNode(string nodeId)
    {
        if (dialogueNodes.ContainsKey(nodeId))
        {
            currentDialogueId = nodeId;
            currentNode = dialogueNodes[nodeId];
            ProcessCurrentNode();
        }
        else
        {
            Debug.LogError("Node ID not found in dialogue: " + nodeId);
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Processes the current dialogue node and notifies the UI.
    /// </summary>
    private void ProcessCurrentNode()
    {
        // Notify the UI about the new node
        OnDialogueNodeChanged?.Invoke(currentNode);
        
        // Check if this node has immediate actions to execute
        if (currentNode.actions != null && currentNode.actions.Count > 0)
        {
            ExecuteActions(currentNode.actions);
        }
        
        // If the node has choices, present them to the player
        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            PresentChoices(currentNode.choices);
        }
        // If no choices and no next node, end the dialogue
        else if (string.IsNullOrEmpty(currentNode.nextNode))
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Presents choices to the player through the UI.
    /// </summary>
    /// <param name="choices">The list of choices to present.</param>
    private void PresentChoices(List<DialogueChoice> choices)
    {
        OnChoicesAvailable?.Invoke(choices);
    }
    
    /// <summary>
    /// Executes a list of actions.
    /// </summary>
    /// <param name="actions">The actions to execute.</param>
    private void ExecuteActions(List<DialogueAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action.type)
            {
                case "loadScene":
                    SceneManager.LoadScene(action.value);
                    break;
                case "unlockAchievement":
                    // Implementation depends on achievement system
                    Debug.Log("Unlocking achievement: " + action.value);
                    break;
                case "setVariable":
                    // Set a game variable
                    Debug.Log("Setting variable: " + action.value);
                    break;
                case "addItem":
                    // Add item to player inventory
                    Debug.Log("Adding item: " + action.value);
                    break;
                default:
                    Debug.LogWarning("Unknown action type: " + action.type);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Ends the current dialogue.
    /// </summary>
    private void EndDialogue()
    {
        Debug.Log("Dialogue ended");
        currentDialogueId = null;
        currentNode = null;
        // Notify the UI that dialogue has ended
        OnDialogueNodeChanged?.Invoke(null);
    }
    
    /// <summary>
    /// Gets the current dialogue node.
    /// </summary>
    /// <returns>The current dialogue node, or null if no dialogue is active.</returns>
    public DialogueNode GetCurrentNode()
    {
        return currentNode;
    }
    
    /// <summary>
    /// Checks if a dialogue is currently active.
    /// </summary>
    /// <returns>True if a dialogue is active, false otherwise.</returns>
    public bool IsDialogueActive()
    {
        return currentNode != null;
    }
}

/// <summary>
/// Represents a dialogue node in the conversation.
/// </summary>
[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string speaker;
    public string text;
    public string portrait;
    public string background;
    public string nextNode;
    public List<DialogueChoice> choices;
    public List<DialogueAction> actions;
    
    public DialogueNode()
    {
        choices = new List<DialogueChoice>();
        actions = new List<DialogueAction>();
    }
}

