using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    public string gameSceneName = "GameScene";
    
    public void PlayGame()
    {
        StartCoroutine(FadeOutAndLoad());
    }
    
    private IEnumerator FadeOutAndLoad()
    {
        // Включаем панель и делаем прозрачной
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        
        // Затемняем
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadeImage.color = color;
        
        // Загружаем сцену
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}