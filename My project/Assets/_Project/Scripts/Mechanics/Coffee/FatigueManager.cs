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
    [Range(0.01f, 0.3f)] public float blinkBlackDuration = 0.04f;  // ← ДЛИТЕЛЬНОСТЬ ЧЁРНОГО ЭКРАНА
    [Range(0.02f, 0.3f)] public float blinkOpenDuration = 0.06f;    // ← ДЛИТЕЛЬНОСТЬ ОТКРЫТИЯ
    
    [Header("=== ПАРАМЕТРЫ ЗАСЫПАНИЯ ===")]
    [Range(0.5f, 10f)] public float sleepInterval = 5f;
    [Range(0.5f, 3f)] public float sleepDuration = 1.5f;
    [Range(0, 30)] public float sleepRestoreAmount = 10f;
    
    [Header("=== ПАРАМЕТРЫ ПЬЯНОГО ЭФФЕКТА ===")]
    [Range(0.5f, 5f)] public float wobbleSpeed = 2f;
    [Range(0, 1)] public float wobbleIntensity = 0.5f;
    
    [Header("=== ССЫЛКИ ===")]
    public BlinkCameraEffect blinkEffect;
    public GameObject gameOverPanel;
    public Button restartButton;
    public DrunkScreenEffect drunkEffect;
    
    [Header("=== СОСТОЯНИЯ ===")]
    public bool isOnPhone = false;
    public bool isGameOver = false;
    
    private Coroutine currentEffectCoroutine;
    private string currentEffect = "none";
    private bool isBlinking = false;
    
    void Start()
    {
        if (blinkEffect == null)
        {
            blinkEffect = Camera.main.GetComponent<BlinkCameraEffect>();
            if (blinkEffect == null)
            {
                Debug.LogError("❌ Нужно добавить BlinkCameraEffect на Main Camera!");
            }
        }
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        Debug.Log($"✅ Система усталости запущена!");
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
                if (drunkEffect != null) drunkEffect.EnableEffect(false);
                if (blinkEffect != null) blinkEffect.SetAlpha(0f);
            }
            return;
        }
        
        if (currentWakeness < 10)
        {
            if (currentEffect != "drunk_sleep")
            {
                StopAllEffects();
                currentEffect = "drunk_sleep";
                Debug.Log($"🥴 КРИТИЧЕСКИЙ: {currentWakeness:F1}%");
                
                if (drunkEffect != null)
                {
                    drunkEffect.EnableEffect(true);
                    float intensity = 1f - (currentWakeness / 10f);
                    drunkEffect.SetIntensity(Mathf.Clamp01(intensity));
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
                Debug.Log($"😴 ЗАСЫПАНИЕ: {currentWakeness:F1}%");
                
                if (drunkEffect != null) drunkEffect.EnableEffect(false);
                StartCoroutine(SleepLoop(sleepInterval));
            }
        }
        else if (currentWakeness < 50)
        {
            if (currentEffect != "blink")
            {
                StopAllEffects();
                currentEffect = "blink";
                Debug.Log($"😉 МОРГАНИЕ: {currentWakeness:F1}%");
                
                if (drunkEffect != null) drunkEffect.EnableEffect(false);
                StartCoroutine(BlinkLoop());
            }
        }
        else
        {
            if (currentEffect != "none")
            {
                StopAllEffects();
                currentEffect = "none";
                Debug.Log($"✅ НОРМА: {currentWakeness:F1}%");
                
                if (drunkEffect != null) drunkEffect.EnableEffect(false);
                if (blinkEffect != null) blinkEffect.SetAlpha(0f);
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
                yield return StartCoroutine(BlinkEffect());
            }
        }
    }
    
    IEnumerator BlinkEffect()
    {
        if (isBlinking) yield break;
        isBlinking = true;
        
        // МГНОВЕННО чёрный экран
        if (blinkEffect != null) blinkEffect.SetAlpha(1f);
        
        // Короткая задержка (только чёрный экран)
        yield return new WaitForSeconds(blinkBlackDuration);
        
        // Плавное открытие
        float elapsed = 0;
        while (elapsed < blinkOpenDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / blinkOpenDuration);
            if (blinkEffect != null) blinkEffect.SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Финальная очистка
        if (blinkEffect != null) blinkEffect.SetAlpha(0f);
        
        isBlinking = false;
    }
    
    IEnumerator SleepLoop(float interval)
    {
        while ((currentEffect == "sleep" || currentEffect == "drunk_sleep") && !isOnPhone && !isGameOver)
        {
            yield return new WaitForSeconds(interval);
            if ((currentEffect == "sleep" || currentEffect == "drunk_sleep") && !isOnPhone && !isGameOver)
            {
                yield return StartCoroutine(SleepEffect());
            }
        }
    }
    
    IEnumerator SleepEffect()
    {
        float elapsed = 0;
        
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / sleepDuration);
            if (blinkEffect != null) blinkEffect.SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (blinkEffect != null) blinkEffect.SetAlpha(1f);
        
        float oldValue = currentWakeness;
        currentWakeness = Mathf.Min(currentWakeness + sleepRestoreAmount, 100);
        Debug.Log($"💤 ПРОСНУЛСЯ! +{sleepRestoreAmount}% ({oldValue:F1}% → {currentWakeness:F1}%)");
        
        elapsed = 0;
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / sleepDuration);
            if (blinkEffect != null) blinkEffect.SetAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (blinkEffect != null) blinkEffect.SetAlpha(0f);
    }
    
    IEnumerator DrunkEffectUpdater()
    {
        while (currentEffect == "drunk_sleep" && !isGameOver)
        {
            if (drunkEffect != null)
            {
                float intensity = 1f - (currentWakeness / 10f);
                drunkEffect.SetIntensity(Mathf.Clamp01(intensity));
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    void StopAllEffects()
    {
        if (currentEffectCoroutine != null)
            StopCoroutine(currentEffectCoroutine);
        
        if (blinkEffect != null) blinkEffect.SetAlpha(0f);
        if (drunkEffect != null) drunkEffect.EnableEffect(false);
    }
    
    void GameOver()
    {
        isGameOver = true;
        StopAllEffects();
        if (blinkEffect != null) blinkEffect.SetAlpha(1f);
        
        Debug.Log("💀 GAME OVER!");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    void RestartGame()
    {
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
            Debug.Log($"📊 Бодрость: {currentWakeness:F1}%");
        }
    }
    
    public void RestoreWakeness(float amount)
    {
        if (!isGameOver)
        {
            currentWakeness = Mathf.Min(currentWakeness + amount, 100);
            Debug.Log($"☕ +{amount}% бодрости! Текущая: {currentWakeness}%");
        }
    }
    
    public void SetOnPhone(bool value)
    {
        isOnPhone = value;
        Debug.Log($"📞 Телефон: {(isOnPhone ? "поднят" : "положен")}");
        if (!isOnPhone && !isGameOver) UpdateEffectsByFatigue();
    }
}