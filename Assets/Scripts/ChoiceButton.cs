using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private Button button;
    
    private int choiceIndex;
    
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (choiceText == null)
            choiceText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void Setup(string text, int index)
    {
        choiceIndex = index;
        
        if (choiceText != null)
            choiceText.text = text;
            
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    private void OnButtonClick()
    {
        // Notify the DialogueManager that this choice was selected
        DialogueManager.Instance.SelectChoice(choiceIndex);
    }
}