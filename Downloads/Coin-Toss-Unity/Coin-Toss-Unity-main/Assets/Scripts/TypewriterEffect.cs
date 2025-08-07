using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private float _typingSpeed = 0.05f;
    
    public float typingSpeed
    {
        get { return _typingSpeed; }
        set { _typingSpeed = value; }
    }
    
    private Coroutine typeCoroutine;
    
    private void Awake()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponent<TextMeshProUGUI>();
    }
    
    public void StartTyping(string text)
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);
            
        typeCoroutine = StartCoroutine(TypeText(text));
    }
    
    public void StopTyping()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }
    }
    
    public void SkipTyping(string fullText)
    {
        StopTyping();
        if (textMeshPro != null)
            textMeshPro.text = fullText;
    }
    
    private IEnumerator TypeText(string text)
    {
        if (textMeshPro == null) yield break;
        
        textMeshPro.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            textMeshPro.text += letter;
            
            // Check for player input to skip typing
            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Submit"))
            {
                textMeshPro.text = text;
                yield break;
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}