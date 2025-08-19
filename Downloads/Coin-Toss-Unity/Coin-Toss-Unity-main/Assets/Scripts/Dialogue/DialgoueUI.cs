// FILE: DialogueUI.cs
// PURPOSE: Controls all the visual elements of the dialogue system.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to use TextMeshPro

public class DialogueUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Image leftPortrait;
    public Image rightPortrait;
    public Image backgroundImage; // Add this in the inspector and assign a UI Image for background

    [Header("Buttons")]
    public Button nextButton;
    public Button autoButton;
    public Button skipButton;

    [Header("Choices")]
    public GameObject choiceButtonContainer;
    public GameObject choiceButtonPrefab;

    // --- Subscribing to Events ---

    private void OnEnable()
    {
        // Listen for when the DialogueManager sends a new line of dialogue
        DialogueManager.OnDialogueNodeChanged += UpdateDialogueDisplay;
        // Listen for when the DialogueManager sends a list of choices
        DialogueManager.OnChoicesAvailable += ShowChoices;
    }

    private void OnDisable()
    {
        // Stop listening when this UI is disabled to prevent errors
        DialogueManager.OnDialogueNodeChanged -= UpdateDialogueDisplay;
        DialogueManager.OnChoicesAvailable -= ShowChoices;
    }

    void Start()
    {
        // Make sure the dialogue is hidden when the game starts
        dialoguePanel.SetActive(false);
        
        // Connect the UI buttons to the DialogueManager functions
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(() => DialogueManager.Instance.NextLine());
        }
        if (autoButton != null)
        {
            autoButton.onClick.AddListener(() => DialogueManager.Instance.ToggleAutoMode());
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(() => DialogueManager.Instance.SkipToEnd());
        }
    }

    // --- Event Handler Functions ---

    private void UpdateDialogueDisplay(DialogueLine line)
    {
        // If the line is null, it means the conversation has ended
        if (line == null)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        // Show the panel if it's hidden
        if (!dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(true);
        }

        // Load and display background image from Resources
        if (backgroundImage != null && !string.IsNullOrEmpty(line.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>(line.background);
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
                backgroundImage.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }
        }
        else if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }

        // Update the text and speaker name
        speakerNameText.text = line.speaker;
        dialogueText.text = line.dialogueText; // We can add a typewriter effect here later

        // Load and display left/right portrait sprites and highlight the speaker
        Sprite leftSprite = !string.IsNullOrEmpty(line.leftPortrait) ? Resources.Load<Sprite>(line.leftPortrait) : null;
        Sprite rightSprite = !string.IsNullOrEmpty(line.rightPortrait) ? Resources.Load<Sprite>(line.rightPortrait) : null;

        if (leftSprite != null)
        {
            leftPortrait.sprite = leftSprite;
            leftPortrait.gameObject.SetActive(true);
        }
        else
        {
            leftPortrait.gameObject.SetActive(false);
        }

        if (rightSprite != null)
        {
            rightPortrait.sprite = rightSprite;
            rightPortrait.gameObject.SetActive(true);
        }
        else
        {
            rightPortrait.gameObject.SetActive(false);
        }

        // Highlight the speaker
        if (line.speakerPosition == "left")
        {
            leftPortrait.color = Color.white;
            rightPortrait.color = new Color(1,1,1,0.3f); // Dim right
        }
        else if (line.speakerPosition == "right")
        {
            rightPortrait.color = Color.white;
            leftPortrait.color = new Color(1,1,1,0.3f); // Dim left
        }
        else
        {
            leftPortrait.color = new Color(1,1,1,0.3f);
            rightPortrait.color = new Color(1,1,1,0.3f);
        }
    }

    private void ShowChoices(List<Choice> choices)
    {
        // Hide the "Next" button when choices are available
        nextButton.gameObject.SetActive(false);
        
        // Clear any old choice buttons
        foreach (Transform child in choiceButtonContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Create a new button for each choice
        foreach (Choice choice in choices)
        {
            GameObject choiceButtonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);
            
            // Set the button's text
            TextMeshProUGUI buttonText = choiceButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            // Set the button's click event
            Button button = choiceButtonObj.GetComponent<Button>();
            if (button != null)
            {
                // When this button is clicked, it will call MakeChoice in the DialogueManager
                // and pass in this specific choice data.
                button.onClick.AddListener(() => {
                    DialogueManager.Instance.MakeChoice(choice);
                    // After making a choice, re-enable the "Next" button and clear the choices
                    nextButton.gameObject.SetActive(true);
                    foreach (Transform child in choiceButtonContainer.transform)
                    {
                        Destroy(child.gameObject);
                    }
                });
            }
        }
    }
}
