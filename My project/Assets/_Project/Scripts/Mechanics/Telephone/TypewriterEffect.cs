using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public float charsPerSecond = 40f;
    public float minSkipProgress = 0.3f; // 30% текста должно напечататься
    
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private string fullText;
    private System.Action onCompleteCallback;
    private bool canSkip = false;
    private bool isSkipping = false;
    
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        // 🔥 Скип на ЛЮБУЮ кнопку/клавишу/клик, но только если canSkip = true (>=30%)
        if (IsTyping && canSkip && !isSkipping)
        {
            // Любая кнопка мыши
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                Skip();
                return;
            }
            
            // Любая клавиша на клавиатуре
            if (Input.anyKeyDown)
            {
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
        isSkipping = false;
        typingCoroutine = StartCoroutine(TypeText());
    }
    
    private IEnumerator TypeText()
    {
        textComponent.text = "";
        float delay = 1f / charsPerSecond;
        int minSkipLength = Mathf.CeilToInt(fullText.Length * minSkipProgress);
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            // Если скипнули - выводим весь текст и выходим
            if (isSkipping)
            {
                textComponent.text = fullText;
                break;
            }
            
            textComponent.text = fullText.Substring(0, i);
            
            // 🔥 Разрешаем скип после достижения порога
            if (!canSkip && i >= minSkipLength)
            {
                canSkip = true;
                Debug.Log($"✅ Скип доступен ({i}/{fullText.Length})");
            }
            
            yield return new WaitForSeconds(delay);
        }
        
        // Финальная установка текста
        if (textComponent.text != fullText)
            textComponent.text = fullText;
        
        typingCoroutine = null;
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
        canSkip = false;
        isSkipping = false;
    }
    
    public void Skip()
    {
        if (!IsTyping) return;
        if (!canSkip) return;
        
        isSkipping = true;
        Debug.Log("⏩ Диалог пропущен");
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
        isSkipping = false;
    }
    
    public bool IsTyping => typingCoroutine != null;
}