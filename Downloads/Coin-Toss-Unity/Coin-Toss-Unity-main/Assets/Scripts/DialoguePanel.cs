using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    private TypewriterEffect typewriterEffect;
    private ChoiceButton[] choiceButtons;
    
    private void Awake()
    {
        // Get or add TypewriterEffect component to dialogueText
        if (dialogueText != null)
        {
            typewriterEffect = dialogueText.GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                typewriterEffect = dialogueText.gameObject.AddComponent<TypewriterEffect>();
            }
            typewriterEffect.typingSpeed = typingSpeed;
        }
        
        // Initially hide the dialogue panel
        HideDialoguePanel();
    }
    
    private void OnEnable()
    {
        // Subscribe to dialogue events
        DialogueManager.OnDialogueNodeChanged += OnDialogueNodeChanged;
        DialogueManager.OnChoicesAvailable += OnChoicesAvailable;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from dialogue events
        DialogueManager.OnDialogueNodeChanged -= OnDialogueNodeChanged;
        DialogueManager.OnChoicesAvailable -= OnChoicesAvailable;
    }
    
    private void OnDialogueNodeChanged(DialogueNode node)
    {
        if (node == null)
        {
            HideDialoguePanel();
            return;
        }
        
        ShowDialoguePanel();
        UpdateSpeakerName(node.speaker);
        UpdatePortrait(node.portrait);
        
        // Start typing the dialogue text
        if (typewriterEffect != null)
        {
            typewriterEffect.StartTyping(node.text);
        }
        else if (dialogueText != null)
        {
            dialogueText.text = node.text;
        }
    }
    
    private void OnChoicesAvailable(System.Collections.Generic.List<DialogueChoice> choices)
    {
        CreateChoiceButtons(choices);
    }
    
    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }
    
    private void HideDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    private void UpdateSpeakerName(string speakerName)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }
    }
    
    private void UpdatePortrait(string portraitId)
    {
        // For simplicity, we'll always show portraits on the left side
        if (leftPortrait != null)
        {
            // In a real implementation, you would load the appropriate sprite based on portraitId
            // For now, we'll just show/hide the portrait image
            leftPortrait.gameObject.SetActive(!string.IsNullOrEmpty(portraitId));
        }
        
        // Hide the right portrait
        if (rightPortrait != null)
        {
            rightPortrait.gameObject.SetActive(false);
        }
    }
    
    private void CreateChoiceButtons(System.Collections.Generic.List<DialogueChoice> choices)
    {
        // Clear existing buttons
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new buttons
        for (int i = 0; i < choices.Count; i++)
        {
            int choiceIndex = i; // Capture for closure
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            ChoiceButton choiceButton = buttonObj.GetComponent<ChoiceButton>();
            
            if (choiceButton != null)
            {
                choiceButton.Setup(choices[i].text, choiceIndex);
            }
            else
            {
                // Fallback if ChoiceButton component is not found
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText != null)
                {
                    buttonText.text = choices[i].text;
                }
                
                if (button != null)
                {
                    // Remove any existing listeners
                    button.onClick.RemoveAllListeners();
                    // Add listener for this specific choice
                    int index = choiceIndex; // Capture for closure
                    button.onClick.AddListener(() => DialogueManager.Instance.SelectChoice(index));
                }
            }
        }
    }
}