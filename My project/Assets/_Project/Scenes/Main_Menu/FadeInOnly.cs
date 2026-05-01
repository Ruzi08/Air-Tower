using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOnly : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    
    void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Перетащи FadePanel в поле Fade Image!");
            return;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());
    }
    
    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - (elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
        
        Debug.Log("Fade In завершен!");
    }
}