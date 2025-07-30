using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Typewriter Effect")]
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Portrait Fade Settings")]
    [SerializeField] private float portraitFadeDuration = 0.5f;

    private Dictionary<string, Sprite> portraitSprites = new Dictionary<string, Sprite>();

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

    private void Start()
    {
        // Initially hide the dialogue panel
        HideDialoguePanel();
        
        // Load portrait sprites (in a real implementation, you would load these from resources)
        LoadPortraitSprites();
        
        // Initialize typewriter effect if not assigned
        if (typewriterEffect == null && dialogueText != null)
        {
            typewriterEffect = dialogueText.GetComponent<TypewriterEffect>();
            if (typewriterEffect == null)
            {
                typewriterEffect = dialogueText.gameObject.AddComponent<TypewriterEffect>();
            }
        }
        
        if (typewriterEffect != null)
        {
            typewriterEffect.typingSpeed = typingSpeed;
        }
    }

    private void LoadPortraitSprites()
    {
        // In a real implementation, you would load portrait sprites from Resources or Addressables
        // For now, we'll just create a placeholder dictionary
        // portraitSprites["mayor_happy"] = Resources.Load<Sprite>("Portraits/mayor_happy");
        // portraitSprites["mayor_concerned"] = Resources.Load<Sprite>("Portraits/mayor_concerned");
        // etc.
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
        
        // Start typing the dialogue text using TypewriterEffect
        if (typewriterEffect != null && dialogueText != null)
        {
            typewriterEffect.StartTyping(node.text);
        }
        else if (dialogueText != null)
        {
            dialogueText.text = node.text;
        }
    }

    private void OnChoicesAvailable(List<DialogueChoice> choices)
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
        if (string.IsNullOrEmpty(portraitId))
        {
            // Hide both portraits if no portrait is specified
            if (leftPortrait != null) leftPortrait.gameObject.SetActive(false);
            if (rightPortrait != null) rightPortrait.gameObject.SetActive(false);
            return;
        }

        // For simplicity, we'll always show portraits on the left side
        // In a more complex implementation, you might have logic to determine left/right placement
        if (leftPortrait != null)
        {
            // Try to get the sprite from our dictionary
            if (portraitSprites.ContainsKey(portraitId) && portraitSprites[portraitId] != null)
            {
                leftPortrait.sprite = portraitSprites[portraitId];
            }
            else
            {
                // Placeholder if sprite not found
                leftPortrait.sprite = null;
            }
            
            leftPortrait.gameObject.SetActive(true);
            
            // Start fade animation
            StartCoroutine(FadeImage(leftPortrait, 0f, 1f, portraitFadeDuration));
        }
        
        // Hide the right portrait
        if (rightPortrait != null)
        {
            rightPortrait.gameObject.SetActive(false);
        }
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            
            foreach (char letter in text.ToCharArray())
            {
                dialogueText.text += letter;
                
                // Check for player input to skip typing
                if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit"))
                {
                    dialogueText.text = text;
                    yield break;
                }
                
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }

    private void CreateChoiceButtons(List<DialogueChoice> choices)
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
                // Fallback to the old method if ChoiceButton component is not found
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
                    button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                }
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        // Notify DialogueManager of choice selection
        DialogueManager.Instance.SelectChoice(choiceIndex);
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        if (image == null) yield break;

        Color originalColor = image.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                                   Mathf.Lerp(startAlpha, endAlpha, normalizedTime));
            yield return null;
        }
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, endAlpha);
    }
}