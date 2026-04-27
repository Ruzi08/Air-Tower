using UnityEngine;
using System.Collections;

public class PhoneDialogueTrigger : MonoBehaviour, Interactable
{
    [Header("Телефон настройки")]
    public float answerTimeLimit = 8f;

    [Header("Компоненты")]
    public TelephoneSound telephoneSound;   // теперь полностью управляет звуками
    public Animator phoneAnimator;
    public GameObject phoneLight;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private bool isRinging = false;
    private Coroutine answerTimerCoroutine;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void StartRinging()
    {
        Debug.Log("🔔 Телефон звонит!");
        isRinging = true;

        if (telephoneSound != null)
            telephoneSound.StartRing();

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
        while (timer < answerTimeLimit && isRinging)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (isRinging)
        {
            Debug.Log("💀 GAME OVER: не успели ответить!");
            GameOver();
        }
    }

    private void StopRinging()
    {

        if (telephoneSound != null)
        {
            telephoneSound.StopRing();
        }

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
            Debug.Log("Телефон не звонит");
            return;
        }

        if (DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive)
            return;

        Debug.Log("✅ Беру трубку!");

        // Воспроизводим звук через TelephoneSound
        if (telephoneSound != null)
            telephoneSound.PlayPickUp();
        
        StopRinging();

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