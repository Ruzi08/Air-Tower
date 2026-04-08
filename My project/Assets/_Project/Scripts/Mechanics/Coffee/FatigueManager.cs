using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FatigueManager : MonoBehaviour
{
    [Header("=== ПАРАМЕТРЫ УСТАЛОСТИ ===")]
    [Range(0, 100)] public float currentWakeness = 100f;
    [Range(0, 5)] public float wakenessReducePerSecond = 0.3f;
    
    [Header("=== ПАРАМЕТРЫ МОРГАНИЯ ===")]
    [Range(0.5f, 10f)] public float blinkInterval = 3f;
    [Range(0.05f, 0.5f)] public float blinkDuration = 0.1f;
    
    [Header("=== ПАРАМЕТРЫ ЗАСЫПАНИЯ ===")]
    [Range(0.5f, 10f)] public float sleepInterval = 5f;
    [Range(0.5f, 3f)] public float sleepDuration = 1.5f;
    [Range(0, 30)] public float sleepRestoreAmount = 10f;
    
    [Header("=== ПАРАМЕТРЫ ПЬЯНОГО ЭФФЕКТА ===")]
    [Range(0.5f, 5f)] public float wobbleSpeed = 2f;
    [Range(0, 1)] public float wobbleIntensity = 0.5f;
    
    [Header("=== ССЫЛКИ НА UI ===")]
    public Image fadeImage;
    public GameObject gameOverPanel;
    public Button restartButton;
    public DrunkScreenEffect drunkEffect;
    
    [Header("=== СОСТОЯНИЯ ===")]
    public bool isOnPhone = false;
    public bool isGameOver = false;
    
    // Приватные переменные
    private Coroutine currentEffectCoroutine;
    private string currentEffect = "none";
    
    void Start()
    {
        // Настройка UI
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        Debug.Log($"✅ Система усталости запущена!");
        Debug.Log($"📊 Потеря бодрости: {wakenessReducePerSecond}%/сек");
        Debug.Log($"💤 Восстановление при засыпании: +{sleepRestoreAmount}%");
        
        StartCoroutine(ShowStats());
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        currentWakeness -= wakenessReducePerSecond * Time.deltaTime;
        currentWakeness = Mathf.Clamp(currentWakeness, 0, 100);
        
        if (currentWakeness <= 0)
        {
            GameOver();
            return;
        }
        
        UpdateEffectsByFatigue();
    }
    
    void UpdateEffectsByFatigue()
    {
        if (isOnPhone)
        {
            if (currentEffect != "phone")
            {
                StopAllEffects();
                currentEffect = "phone";
                
                if (drunkEffect != null)
                    drunkEffect.EnableEffect(false);
                
                Debug.Log("📞 Разговор по телефону - все эффекты отключены");
            }
            return;
        }
        
        if (currentWakeness < 10)
        {
            if (currentEffect != "drunk_sleep")
            {
                StopAllEffects();
                currentEffect = "drunk_sleep";
                Debug.Log($"🥴 КРИТИЧЕСКИЙ УРОВЕНЬ: {currentWakeness:F1}% < 10% - Пьяный эффект");
                
                if (drunkEffect != null)
                {
                    drunkEffect.EnableEffect(true);
                    float effectIntensity = 1f - (currentWakeness / 10f);
                    drunkEffect.SetIntensity(Mathf.Clamp01(effectIntensity));
                }
                
                currentEffectCoroutine = StartCoroutine(DrunkEffectUpdater());
                StartCoroutine(SleepLoop(sleepInterval * 0.5f));
            }
        }
        else if (currentWakeness < 30)
        {
            if (currentEffect != "sleep")
            {
                StopAllEffects();
                currentEffect = "sleep";
                Debug.Log($"😴 ВЫСОКИЙ УРОВЕНЬ: {currentWakeness:F1}% - Засыпания");
                
                if (drunkEffect != null)
                    drunkEffect.EnableEffect(false);
                
                StartCoroutine(SleepLoop(sleepInterval));
            }
        }
        else if (currentWakeness < 50)
        {
            if (currentEffect != "blink")
            {
                StopAllEffects();
                currentEffect = "blink";
                Debug.Log($"😉 СРЕДНИЙ УРОВЕНЬ: {currentWakeness:F1}% - Моргания");
                
                if (drunkEffect != null)
                    drunkEffect.EnableEffect(false);
                
                StartCoroutine(BlinkLoop());
            }
        }
        else
        {
            if (currentEffect != "none")
            {
                StopAllEffects();
                currentEffect = "none";
                Debug.Log($"✅ НОРМАЛЬНЫЙ УРОВЕНЬ: {currentWakeness:F1}% - Без эффектов");
                
                if (drunkEffect != null)
                    drunkEffect.EnableEffect(false);
            }
        }
    }
    
    IEnumerator BlinkLoop()
    {
        while (currentEffect == "blink" && !isOnPhone && !isGameOver)
        {
            yield return new WaitForSeconds(blinkInterval);
            if (currentEffect == "blink" && !isOnPhone && !isGameOver)
            {
                StartCoroutine(BlinkEffect());
            }
        }
    }
    
    IEnumerator BlinkEffect()
    {
        float elapsed = 0;
        
        while (elapsed < blinkDuration)
        {
            float alpha = Mathf.Lerp(0, 0.95f, elapsed / blinkDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0;
        while (elapsed < blinkDuration)
        {
            float alpha = Mathf.Lerp(0.95f, 0, elapsed / blinkDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }
    
    IEnumerator SleepLoop(float interval)
    {
        while ((currentEffect == "sleep" || currentEffect == "drunk_sleep") && !isOnPhone && !isGameOver)
        {
            yield return new WaitForSeconds(interval);
            if ((currentEffect == "sleep" || currentEffect == "drunk_sleep") && !isOnPhone && !isGameOver)
            {
                StartCoroutine(SleepEffect());
            }
        }
    }
    
    IEnumerator SleepEffect()
    {
        float elapsed = 0;
        
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(0, 0.98f, elapsed / sleepDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        float oldValue = currentWakeness;
        currentWakeness = Mathf.Min(currentWakeness + sleepRestoreAmount, 100);
        Debug.Log($"💤 ПРОСНУЛСЯ! +{sleepRestoreAmount}% ({oldValue:F1}% → {currentWakeness:F1}%)");
        
        elapsed = 0;
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(0.98f, 0, elapsed / sleepDuration);
            if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }
    
    IEnumerator DrunkEffectUpdater()
    {
        while (currentEffect == "drunk_sleep" && !isGameOver)
        {
            if (drunkEffect != null)
            {
                float intensity = 1f - (currentWakeness / 10f);
                intensity = Mathf.Clamp01(intensity);
                drunkEffect.SetIntensity(intensity);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    void StopAllEffects()
    {
        if (currentEffectCoroutine != null)
            StopCoroutine(currentEffectCoroutine);
        
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);
        
        if (drunkEffect != null)
            drunkEffect.EnableEffect(false);
    }
    
    void GameOver()
    {
        isGameOver = true;
        StopAllEffects();
        
        Debug.Log("💀 GAME OVER! Персонаж уснул");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RestartGame()
    {
        Debug.Log("🔄 Перезапуск игры...");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    IEnumerator ShowStats()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(5f);
            string zone = GetFatigueZone();
            Debug.Log($"📊 Бодрость: {currentWakeness:F1}% | Зона: {zone}");
        }
    }
    
    string GetFatigueZone()
    {
        if (currentWakeness < 10) return "КРИТИЧЕСКАЯ (<10%)";
        if (currentWakeness < 30) return "ВЫСОКАЯ (10-30%)";
        if (currentWakeness < 50) return "СРЕДНЯЯ (30-50%)";
        return "НОРМАЛЬНАЯ (>50%)";
    }
    
    public void RestoreWakeness(float amount)
    {
        if (!isGameOver)
        {
            float oldValue = currentWakeness;
            currentWakeness = Mathf.Min(currentWakeness + amount, 100);
            Debug.Log($"☕ Восстановлено {amount}%! ({oldValue:F1}% → {currentWakeness:F1}%)");
        }
    }
    
    public void SetOnPhone(bool isOnPhoneValue)
    {
        isOnPhone = isOnPhoneValue;
        Debug.Log($"📞 Телефон: {(isOnPhone ? "поднят" : "положен")}");
        
        if (!isOnPhone && !isGameOver)
        {
            UpdateEffectsByFatigue();
        }
    }
}