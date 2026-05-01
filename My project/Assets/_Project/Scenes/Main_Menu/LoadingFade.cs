using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingFade : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FADE IMAGE НЕ ПРИВЯЗАН!");
            return;
        }
        
        // Начинаем с черного
        fadeImage.color = new Color(0, 0, 0, 1);
        
        // Просто плавно появляемся (не загружаем ничего)
        StartCoroutine(FadeIn());
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }
}