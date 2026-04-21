using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float charsPerSecond = 40f;
    
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private string fullText;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    public void StartTyping(string text, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        fullText = text;
        typingCoroutine = StartCoroutine(TypeText(onComplete));
    }
    
    private IEnumerator TypeText(System.Action onComplete)
    {
        textComponent.text = "";
        float delay = 1f / charsPerSecond;
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }
        
        onComplete?.Invoke();
        typingCoroutine = null;
    }
    
    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = fullText;
            typingCoroutine = null;
        }
    }
    
    public bool IsTyping => typingCoroutine != null;
}