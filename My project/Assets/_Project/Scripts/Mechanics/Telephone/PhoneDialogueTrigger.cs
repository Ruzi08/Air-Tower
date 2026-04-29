using UnityEngine;
using System.Collections;

public class PhoneDialogueTrigger : MonoBehaviour, Interactable
{
    [Header("Телефон настройки")]
    public float answerTimeLimit = 8f;
    
    [Header("Анимация телефона (подлёт к камере)")]
    public Transform phoneMesh;
    public Transform phoneTargetAnchor;
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
    
    private FirstPersonController playerController;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        playerController = FindObjectOfType<FirstPersonController>();
        
        if (phoneMesh != null)
        {
            originalParent = phoneMesh.parent;
            originalPhonePos = phoneMesh.position;
            originalPhoneRot = phoneMesh.rotation;
        }
        
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
        if (playerController != null)
        {
            playerController.LockAll();
            Debug.Log("🔒 Управление заблокировано");
        }
        
        phoneMesh.SetParent(null);
        
        Vector3 startPos = phoneMesh.position;
        Vector3 targetPos = phoneTargetAnchor.position;
        Quaternion targetRot = phoneTargetAnchor.rotation;
        
        // Мгновенный поворот в правильное положение
        phoneMesh.rotation = targetRot;
        
        Debug.Log("📱 Телефон мгновенно повёрнут в правильное положение");
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * phoneAnimationSpeed;
            phoneMesh.position = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }
        
        phoneMesh.position = targetPos;
        
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
        
        // 🔥 Мгновенно поворачиваем обратно в исходную ориентацию
        phoneMesh.rotation = originalPhoneRot;
        
        Debug.Log("📱 Телефон мгновенно повёрнут обратно");
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * phoneAnimationSpeed;
            phoneMesh.position = Vector3.Lerp(startPos, originalPhonePos, progress);
            yield return null;
        }
        
        phoneMesh.position = originalPhonePos;
        
        phoneMesh.SetParent(originalParent);
        
        if (playerController != null)
        {
            playerController.UnlockAll();
            Debug.Log("🔓 Управление разблокировано");
        }
        
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
        
        if (playerController != null)
            playerController.UnlockAll();
    }

    public string GetDescription()
    {
        return isRinging ? "📞 Взять трубку" : "📱 Телефон";
    }
}