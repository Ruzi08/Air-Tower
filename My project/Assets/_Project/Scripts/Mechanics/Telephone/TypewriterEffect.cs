using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float charsPerSecond = 40f;
    public float minSkipProgress = 0.3f; // 30% текста должно напечататься чтобы можно было скипнуть
    
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private string fullText;
    private System.Action onCompleteCallback;
    private bool canSkip = false;
    private bool isSkipping = false;
    private bool skipRequested = false;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        // Скип только если можно скипнуть (>=30%) И нажата ЛКМ или Пробел
        if (IsTyping && canSkip && !skipRequested)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                Skip();
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
        isSkipping = false;
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
        
        // 🔥 ВАЖНО: вызываем onComplete ТОЛЬКО если текст допечатался, а не скипнулся
        if (!skipRequested)
        {
            onCompleteCallback?.Invoke();
        }
        else
        {
            // При скипе сразу переходим к следующей реплике
            onCompleteCallback?.Invoke();
        }
        
        onCompleteCallback = null;
        canSkip = false;
        skipRequested = false;
        isSkipping = false;
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
        isSkipping = false;
    }
    
    public bool IsTyping => typingCoroutine != null;
}