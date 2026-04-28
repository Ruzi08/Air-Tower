using UnityEngine;
using System.Collections;

public class PhoneDialogueTrigger : MonoBehaviour, Interactable
{
    [Header("Телефон настройки")]
    public float answerTimeLimit = 8f;
    
    [Header("Анимация телефона (подлёт к камере)")]
    public Transform phoneMesh;                // Перетащи сюда модель телефона
    public Transform phoneTargetAnchor;        // 🔥 Якорь (пустой объект перед камерой)
    public float phoneAnimationSpeed = 8f;

    [Header("Компоненты")]
    public TelephoneSound telephoneSound;
    public Animator phoneAnimator;
    public GameObject phoneLight;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private bool isRinging = false;
    private Coroutine answerTimerCoroutine;
    private Coroutine phoneMoveCoroutine;
    
    private Vector3 originalPhonePos;
    private Quaternion originalPhoneRot;
    private Transform originalParent;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (phoneMesh != null)
        {
            originalParent = phoneMesh.parent;
            originalPhonePos = phoneMesh.position;
            originalPhoneRot = phoneMesh.rotation;
        }
        
        // Если якорь не назначен, создаём его автоматически
        if (phoneTargetAnchor == null)
        {
            GameObject anchor = new GameObject("PhoneAnchor");
            phoneTargetAnchor = anchor.transform;
            phoneTargetAnchor.SetParent(Camera.main.transform);
            phoneTargetAnchor.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            phoneTargetAnchor.localRotation = Quaternion.Euler(15f, -20f, 5f);
            Debug.Log("📱 Якорь для телефона создан автоматически");
        }
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
            telephoneSound.StopRing();

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

        if (telephoneSound != null)
            telephoneSound.PlayPickUp();
        
        StopRinging();
        
        StartPhonePickupAnimation();
    }
    
    private void StartPhonePickupAnimation()
    {
        if (phoneMesh == null || phoneTargetAnchor == null)
        {
            DialogueManager.Instance.OnPhonePickedUp();
            return;
        }
        
        if (phoneMoveCoroutine != null)
            StopCoroutine(phoneMoveCoroutine);
        
        phoneMoveCoroutine = StartCoroutine(AnimatePhoneToAnchor());
    }
    
    private IEnumerator AnimatePhoneToAnchor()
    {
        FirstPersonController fps = FindObjectOfType<FirstPersonController>();
        if (fps != null)
            fps.LockAll();
        
        // Открепляем телефон и делаем его независимым
        phoneMesh.SetParent(null);
        
        Vector3 startPos = phoneMesh.position;
        Quaternion startRot = phoneMesh.rotation;
        
        // 🔥 Летим к якорю
        Vector3 targetPos = phoneTargetAnchor.position;
        Quaternion targetRot = phoneTargetAnchor.rotation;
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * phoneAnimationSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            phoneMesh.position = Vector3.Lerp(startPos, targetPos, smoothProgress);
            phoneMesh.rotation = Quaternion.Slerp(startRot, targetRot, smoothProgress);
            
            yield return null;
        }
        
        phoneMesh.position = targetPos;
        phoneMesh.rotation = targetRot;
        
        DialogueManager.Instance.OnPhonePickedUp();
        
        StartCoroutine(WaitForDialogueAndReturn());
    }
    
    private IEnumerator WaitForDialogueAndReturn()
    {
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }
        
        StartCoroutine(ReturnPhoneToOriginal());
    }
    
    private IEnumerator ReturnPhoneToOriginal()
    {
        Vector3 startPos = phoneMesh.position;
        Quaternion startRot = phoneMesh.rotation;
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * phoneAnimationSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            phoneMesh.position = Vector3.Lerp(startPos, originalPhonePos, smoothProgress);
            phoneMesh.rotation = Quaternion.Slerp(startRot, originalPhoneRot, smoothProgress);
            
            yield return null;
        }
        
        phoneMesh.position = originalPhonePos;
        phoneMesh.rotation = originalPhoneRot;
        
        phoneMesh.SetParent(originalParent);
        
        FirstPersonController fps = FindObjectOfType<FirstPersonController>();
        if (fps != null)
            fps.UnlockAll();
        
        Debug.Log("📱 Телефон вернулся на место");
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