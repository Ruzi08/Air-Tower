using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float charsPerSecond = 40f;
    public float minSkipProgress = 0.3f;
    
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private string fullText;
    private System.Action onCompleteCallback;
    private bool canSkip = false;
    private bool skipRequested = false;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        // 🔥 Скип на ЛКМ ИЛИ на любую клавишу клавиатуры
        if (IsTyping && canSkip && !skipRequested)
        {
            // Левая кнопка мыши
            if (Input.GetMouseButtonDown(0))
            {
                Skip();
                return;
            }
            
            // Любая клавиша на клавиатуре (кроме модификаторов)
            if (Input.anyKeyDown)
            {
                // Игнорируем служебные клавиши
                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) return;
                if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) return;
                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) return;
                
                Skip();
                return;
            }
        }
    }
    
    public void StartTyping(string text, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        fullText = text;
        onCompleteCallback = onComplete;
        canSkip = false;
        skipRequested = false;
        typingCoroutine = StartCoroutine(TypeText());
    }
    
    private IEnumerator TypeText()
    {
        textComponent.text = "";
        float delay = 1f / charsPerSecond;
        int minSkipLength = Mathf.CeilToInt(fullText.Length * minSkipProgress);
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (skipRequested)
            {
                textComponent.text = fullText;
                break;
            }
            
            textComponent.text = fullText.Substring(0, i);
            
            if (!canSkip && i >= minSkipLength)
            {
                canSkip = true;
                Debug.Log($"✅ Скип доступен ({i}/{fullText.Length})");
            }
            
            yield return new WaitForSeconds(delay);
        }
        
        if (textComponent.text != fullText)
            textComponent.text = fullText;
        
        typingCoroutine = null;
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
        canSkip = false;
        skipRequested = false;
    }
    
    public void Skip()
    {
        if (!IsTyping) return;
        if (!canSkip) return;
        if (skipRequested) return;
        
        skipRequested = true;
        Debug.Log("⏩ Анимация печати пропущена");
    }
    
    public void ForceComplete()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = fullText;
            typingCoroutine = null;
            onCompleteCallback?.Invoke();
            onCompleteCallback = null;
        }
        canSkip = false;
        skipRequested = false;
    }
    
    public bool IsTyping => typingCoroutine != null;
}