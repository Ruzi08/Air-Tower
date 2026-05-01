using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    private void Start()
    {
        Debug.Log("=== FADE START ===");
        
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image не привязан в инспекторе!");
            return;
        }
        
        // Начинаем с черного
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        
        Debug.Log($"Начальный Alpha: {fadeImage.color.a}");
        
        // Запускаем затемнение
        StartCoroutine(FadeIn());
    }
    
    private IEnumerator FadeIn()
    {
        Debug.Log("Корутина запущена");
        
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            
            Debug.Log($"Alpha: {alpha}");
            yield return null;
        }
        
        // Финальный прозрачный
        color.a = 0f;
        fadeImage.color = color;
        
        Debug.Log("Затемнение закончено");
        
        // Отключаем объект (опционально)
        // fadeImage.gameObject.SetActive(false);
    }
}