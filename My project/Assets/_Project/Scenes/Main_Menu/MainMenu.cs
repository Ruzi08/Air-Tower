using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Укажите в инспекторе название сцены для игры
    public string gameSceneName = "GameScene";
    
    // Метод для кнопки Play
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    // Метод для кнопки Options
    public void OpenOptions()
    {
        // Здесь можно открыть панель настроек
        Debug.Log("Открываем настройки");
        // Например: optionsPanel.SetActive(true);
    }
    
    // Метод для кнопки Exit
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}