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
    [Range(0.01f, 0.3f)] public float blinkBlackDuration = 0.04f;
    [Range(0.02f, 0.3f)] public float blinkOpenDuration = 0.06f;
    
    [Header("=== ПАРАМЕТРЫ ЗАСЫПАНИЯ ===")]
    [Range(0.5f, 10f)] public float sleepInterval = 5f;
    [Range(0.5f, 3f)] public float sleepDuration = 1.5f;
    [Range(0, 30)] public float sleepRestoreAmount = 10f;
    
    [Header("=== ПАРАМЕТРЫ ПЬЯНОГО ЭФФЕКТА ===")]
    [Range(0, 10)] public float pivoScaleActive = 5f;   // Значение при <10%
    [Range(0, 10)] public float pivoScaleNormal = 0f;   // Значение при >10%
    
    [Header("=== ССЫЛКИ ===")]
    public URPFullScreenController fullScreenController;
    public GameObject gameOverPanel;
    public Button restartButton;
    
    [Header("=== СОСТОЯНИЯ ===")]
    public bool isOnPhone = false;
    public bool isGameOver = false;
    
    private Coroutine blinkCoroutine;
    private Coroutine sleepCoroutine;
    private string currentState = "normal";
    private bool isBlinking = false;
    
    void Start()
    {
        if (fullScreenController == null)
            fullScreenController = FindObjectOfType<URPFullScreenController>();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        // Выключаем всё
        if (fullScreenController != null)
        {
            fullScreenController.SetBlinkAlpha(0f);
            fullScreenController.SetPivoScale(pivoScaleNormal);
        }
        
        Debug.Log("✅ FatigueManager запущен!");
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
        
        UpdateState();
    }
    
    void UpdateState()
    {
        if (isOnPhone)
        {
            if (currentState != "phone")
            {
                StopAll();
                currentState = "phone";
                if (fullScreenController != null)
                {
                    fullScreenController.SetBlinkAlpha(0f);
                    fullScreenController.SetPivoScale(pivoScaleNormal);
                }
            }
            return;
        }
        
        if (currentWakeness < 10)
        {
            if (currentState != "drunk")
            {
                StopAll();
                currentState = "drunk";
                Debug.Log($"🥴 ПЬЯНЫЙ РЕЖИМ: {currentWakeness:F1}%");
                
                // Включаем Pivo шейдер
                if (fullScreenController != null)
                    fullScreenController.SetPivoScale(pivoScaleActive);
                
                // Засыпания чаще
                sleepCoroutine = StartCoroutine(SleepLoop(sleepInterval * 0.5f));
            }
        }
        else if (currentWakeness < 30)
        {
            if (currentState != "sleep")
            {
                StopAll();
                currentState = "sleep";
                Debug.Log($"😴 ЗАСЫПАНИЕ: {currentWakeness:F1}%");
                
                // Выключаем Pivo
                if (fullScreenController != null)
                    fullScreenController.SetPivoScale(pivoScaleNormal);
                
                sleepCoroutine = StartCoroutine(SleepLoop(sleepInterval));
            }
        }
        else if (currentWakeness < 50)
        {
            if (currentState != "blink")
            {
                StopAll();
                currentState = "blink";
                Debug.Log($"😉 МОРГАНИЕ: {currentWakeness:F1}%");
                
                // Выключаем Pivo
                if (fullScreenController != null)
                    fullScreenController.SetPivoScale(pivoScaleNormal);
                
                blinkCoroutine = StartCoroutine(BlinkLoop());
            }
        }
        else
        {
            if (currentState != "normal")
            {
                StopAll();
                currentState = "normal";
                Debug.Log($"✅ НОРМА: {currentWakeness:F1}%");
                
                if (fullScreenController != null)
                {
                    fullScreenController.SetBlinkAlpha(0f);
                    fullScreenController.SetPivoScale(pivoScaleNormal);
                }
            }
        }
    }
    
    void StopAll()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        if (sleepCoroutine != null) StopCoroutine(sleepCoroutine);
        
        blinkCoroutine = null;
        sleepCoroutine = null;
        
        if (fullScreenController != null)
            fullScreenController.SetBlinkAlpha(0f);
        
        isBlinking = false;
    }
    
    IEnumerator BlinkLoop()
    {
        while (currentState == "blink" && !isOnPhone && !isGameOver)
        {
            yield return new WaitForSeconds(blinkInterval);
            if (currentState == "blink" && !isOnPhone && !isGameOver)
            {
                yield return StartCoroutine(DoBlink());
            }
        }
    }
    
    IEnumerator DoBlink()
    {
        if (isBlinking) yield break;
        isBlinking = true;
        
        if (fullScreenController != null) fullScreenController.SetBlinkAlpha(1f);
        yield return new WaitForSeconds(blinkBlackDuration);
        
        float elapsed = 0;
        while (elapsed < blinkOpenDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / blinkOpenDuration);
            if (fullScreenController != null) fullScreenController.SetBlinkAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (fullScreenController != null) fullScreenController.SetBlinkAlpha(0f);
        isBlinking = false;
    }
    
    IEnumerator SleepLoop(float interval)
    {
        while ((currentState == "sleep" || currentState == "drunk") && !isOnPhone && !isGameOver)
        {
            yield return new WaitForSeconds(interval);
            if ((currentState == "sleep" || currentState == "drunk") && !isOnPhone && !isGameOver)
            {
                yield return StartCoroutine(DoSleep());
            }
        }
    }
    
    IEnumerator DoSleep()
    {
        float elapsed = 0;
        
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / sleepDuration);
            if (fullScreenController != null) fullScreenController.SetBlinkAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (fullScreenController != null) fullScreenController.SetBlinkAlpha(1f);
        
        float oldValue = currentWakeness;
        currentWakeness = Mathf.Min(currentWakeness + sleepRestoreAmount, 100);
        Debug.Log($"💤 ПРОСНУЛСЯ! +{sleepRestoreAmount}% ({oldValue:F1}% → {currentWakeness:F1}%)");
        
        elapsed = 0;
        while (elapsed < sleepDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / sleepDuration);
            if (fullScreenController != null) fullScreenController.SetBlinkAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (fullScreenController != null) fullScreenController.SetBlinkAlpha(0f);
    }
    
    void GameOver()
    {
        isGameOver = true;
        StopAll();
        
        if (fullScreenController != null)
        {
            fullScreenController.SetBlinkAlpha(1f);
            fullScreenController.SetPivoScale(pivoScaleNormal);
        }
        
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
    
    public void RestoreWakeness(float amount)
    {
        if (!isGameOver)
        {
            currentWakeness = Mathf.Min(currentWakeness + amount, 100);
            Debug.Log($"☕ +{amount}% бодрости!");
            UpdateState();
        }
    }
    
    public void SetOnPhone(bool value)
    {
        isOnPhone = value;
        UpdateState();
    }
}