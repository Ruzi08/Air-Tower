using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhoneDialogueTrigger : MonoBehaviour, Interactable
{
    [Header("Телефон настройки")]
    public float answerTimeLimit = 8f;
    
    [Header("Компоненты")]
    public SimpleSound ringSound;
    public Animator phoneAnimator;
    public GameObject phoneLight;
    
    [Header("Звуки трубки")]
    public AudioClip pickUpSound;
    public AudioClip hangUpSound;
    [Range(0, 1)] public float pickUpVolume = 0.7f;
    [Range(0, 1)] public float hangUpVolume = 0.7f;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    
    private bool isRinging = false;
    private Coroutine answerTimerCoroutine;
    
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    // Вызывается из DialogueManager по расписанию
    public void StartRinging()
    {
        Debug.Log("🔔🔔🔔 ТЕЛЕФОН ЗВОНИТ! 🔔🔔🔔");
        isRinging = true;
        
        if (ringSound != null)
            ringSound.Play();
        
        if (phoneAnimator != null)
            phoneAnimator.SetBool("IsRinging", true);
        
        if (phoneLight != null)
            phoneLight.SetActive(true);
        
        if (answerTimerCoroutine != null)
            StopCoroutine(answerTimerCoroutine);
        answerTimerCoroutine = StartCoroutine(AnswerTimer());
    }
    
    private IEnumerator AnswerTimer()
    {
        float timer = 0f;
        Debug.Log($"⏱️ Нужно ответить за {answerTimeLimit} секунд");
        
        while (timer < answerTimeLimit && isRinging)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (isRinging)
        {
            Debug.Log("💀 GAME OVER: Не успели ответить на звонок!");
            GameOver();
        }
    }
    
    private void StopRinging()
    {
        Debug.Log("🔇 ОСТАНОВКА ЗВОНКА");
        
        if (ringSound != null)
            ringSound.Stop();
        
        if (phoneAnimator != null)
            phoneAnimator.SetBool("IsRinging", false);
        
        if (phoneLight != null)
            phoneLight.SetActive(false);
        
        if (answerTimerCoroutine != null)
            StopCoroutine(answerTimerCoroutine);
        
        isRinging = false;
    }
    
    public void Interact()
    {
        if (!isRinging) 
        {
            Debug.Log("❌ Телефон не звонит");
            return;
        }
        
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("❌ DialogueManager.Instance = null");
            return;
        }
        
        if (DialogueManager.Instance.IsDialogueActive) 
        {
            Debug.Log("❌ Диалог уже активен");
            return;
        }
        
        Debug.Log("✅ Беру трубку!");
        
        // Звук поднятия трубки
        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position, pickUpVolume);
        }
        
        StopRinging();
        
        // Сообщаем DialogueManager что трубку взяли
        DialogueManager.Instance.OnPhonePickedUp();
    }
    
    private void GameOver()
    {
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (CrosshairController.Instance != null)
            CrosshairController.Instance.Hide();
    }
    
    public string GetDescription()
    {
        return isRinging ? "📞 Взять трубку" : "📱 Телефон";
    }
}