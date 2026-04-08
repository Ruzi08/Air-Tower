using UnityEngine;
using System.Collections;

public class FatigueManager : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ===")]
    public float currentWakeness = 100f;
    public float wakenessReducePerSecond = 0.3f;
    
    [Header("=== ССЫЛКИ НА UI ===")]
    public GameObject gameOverPanel;  // Перетащи сюда GameOverPanel
    public UnityEngine.UI.Button restartButton;  // Перетащи сюда RestartButton
    
    private bool isGameOver = false;
    
    void Start()
    {
        Debug.Log("✅ Система усталости запущена!");
        
        // Скрываем панель Game Over при старте
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        else
            Debug.LogWarning("⚠️ GameOverPanel не назначен в FatigueManager!");
        
        // Назначаем обработчик для кнопки рестарта
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        else
            Debug.LogWarning("⚠️ RestartButton не назначен в FatigueManager!");
        
        StartCoroutine(ShowStats());
    }
    
    void Update()
    {
        if (isGameOver) return; // Если игра закончена - не уменьшаем усталость
        
        currentWakeness -= wakenessReducePerSecond * Time.deltaTime;
        currentWakeness = Mathf.Clamp(currentWakeness, 0, 100);
        
        if (currentWakeness <= 0 && !isGameOver)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        isGameOver = true;
        Debug.Log("💀 GAME OVER! Ты уснул от усталости!");
        
        // Показываем панель Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("📺 Панель Game Over показана");
        }
        
        // Останавливаем время в игре
        Time.timeScale = 0f;
        
        // Разблокируем курсор чтобы можно было нажать на кнопку
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RestartGame()
    {
        Debug.Log("🔄 Перезапуск игры...");
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        // Перезагружаем текущую сцену
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    IEnumerator ShowStats()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(5f);
            Debug.Log($"📊 Бодрость: {currentWakeness:F1}% | Потеря: {wakenessReducePerSecond:F1}%/сек");
        }
    }
    
    public void RestoreWakeness(float amount)
    {
        if (!isGameOver)
        {
            currentWakeness = Mathf.Min(currentWakeness + amount, 100);
            Debug.Log($"☕ Восстановлено {amount}% бодрости! Текущая: {currentWakeness}%");
        }
    }
}