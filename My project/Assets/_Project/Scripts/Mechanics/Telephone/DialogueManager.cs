using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ScheduledCall
{
    public string callName;
    public TextAsset dialogueJsonFile;
    public float startTime;
    public bool played = false;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    
    [Header("Typewriter")]
    public TypewriterEffect typewriter;
    
    [Header("Настройки блокировки")]
    public bool lockPlayer = true;
    public bool hideCrosshairDuringDialogue = true;
    
    [Header("Расписание звонков")]
    public List<ScheduledCall> scheduledCalls;
    public PhoneDialogueTrigger phone;
    
    private List<DialogueEntry> currentDialogueList;
    private int currentIndex = 0;
    private bool isDialogueActive = false;
    private bool waitingForInput = false;
    private Coroutine autoProgressCoroutine;
    
    private FirstPersonController playerController;
    private PlayerInteractor playerInteractor;
    private CameraHeadBob cameraHeadBob;
    
    private float gameTimer = 0f;
    private bool gameStarted = false;
    private TextAsset pendingDialogue = null;
    
    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        dialoguePanel.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);
        
        playerController = FindObjectOfType<FirstPersonController>();
        playerInteractor = FindObjectOfType<PlayerInteractor>();
        cameraHeadBob = FindObjectOfType<CameraHeadBob>();
        
        gameStarted = true;
        
        StartCoroutine(CheckScheduledCalls());
    }
    
    void Update()
    {
        if (gameStarted)
            gameTimer += Time.deltaTime;
        
        // 🔥 Продолжение диалога на ЛЮБУЮ кнопку/клавишу/клик
        if (isDialogueActive && waitingForInput)
        {
            // Любая кнопка мыши
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                OnContinueClicked();
                return;
            }
            
            // Любая клавиша на клавиатуре
            if (Input.anyKeyDown)
            {
                // Исключаем ESC если нужно (опционально)
                // if (Input.GetKeyDown(KeyCode.Escape)) return;
                
                OnContinueClicked();
                return;
            }
        }
    }
    
    private IEnumerator CheckScheduledCalls()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (isDialogueActive) continue;
            if (phone == null) continue;
            
            foreach (ScheduledCall call in scheduledCalls)
            {
                if (!call.played && gameTimer >= call.startTime)
                {
                    call.played = true;
                    Debug.Log($"📞 ЗВОНОК ПО РАСПИСАНИЮ: {call.callName} на {call.startTime} секунде");
                    
                    pendingDialogue = call.dialogueJsonFile;
                    phone.StartRinging();
                    break;
                }
            }
        }
    }
    
    public void OnPhonePickedUp()
    {
        if (pendingDialogue != null)
        {
            LoadAndStartDialogue(pendingDialogue);
            pendingDialogue = null;
        }
        else
        {
            Debug.LogWarning("Нет диалога для этого звонка");
        }
    }
    
    private void LoadAndStartDialogue(TextAsset jsonFile)
    {
        if (jsonFile == null)
        {
            Debug.LogError("❌ JSON файл диалога не назначен!");
            return;
        }
        
        DialogueCollection collection = JsonUtility.FromJson<DialogueCollection>(jsonFile.text);
        
        if (collection != null && collection.dialogues != null && collection.dialogues.Count > 0)
        {
            StartDialogue(collection.dialogues);
        }
        else
        {
            Debug.LogError($"❌ Ошибка парсинга JSON: {jsonFile.name}");
        }
    }
    
    public void StartDialogue(List<DialogueEntry> dialogues)
    {
        if (isDialogueActive) return;
        
        currentDialogueList = dialogues;
        currentIndex = 0;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        
        LockPlayerControls(true);
        
        OnDialogueStart?.Invoke();
        ShowCurrentLine();
    }
    
    private void ShowCurrentLine()
    {
        if (currentIndex >= currentDialogueList.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueEntry entry = currentDialogueList[currentIndex];
        
        typewriter.StartTyping(entry.text, OnTypingComplete);
        continueButton.interactable = false;
        waitingForInput = false;
        
        if (autoProgressCoroutine != null)
            StopCoroutine(autoProgressCoroutine);
    }
    
    private void OnTypingComplete()
    {
        continueButton.interactable = true;
        waitingForInput = true;
        
        DialogueEntry entry = currentDialogueList[currentIndex];
        if (entry.autoProgressDelay > 0)
        {
            autoProgressCoroutine = StartCoroutine(AutoProgress(entry.autoProgressDelay));
        }
    }
    
    private IEnumerator AutoProgress(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (waitingForInput && isDialogueActive)
        {
            OnContinueClicked();
        }
    }
    
    private void OnContinueClicked()
    {
        // Если текст ещё печатается - скипаем
        if (typewriter.IsTyping)
        {
            typewriter.Skip();
            return;
        }
        
        currentIndex++;
        ShowCurrentLine();
    }
    
    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        
        LockPlayerControls(false);
        
        OnDialogueEnd?.Invoke();
        
        if (phone != null && phone.telephoneSound != null)
        {
            phone.telephoneSound.PlayHangUp();
        }
    }
    
    private void LockPlayerControls(bool locked)
    {
        if (!lockPlayer) return;
        
        if (playerInteractor != null)
            playerInteractor.enabled = !locked;
        
        if (playerController != null)
        {
            if (locked)
                playerController.LockAll();
            else
                playerController.UnlockAll();
        }
        
        if (cameraHeadBob != null)
        {
            cameraHeadBob.enabled = !locked;
        }
        
        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (hideCrosshairDuringDialogue && CrosshairController.Instance != null)
                CrosshairController.Instance.Hide();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (hideCrosshairDuringDialogue && CrosshairController.Instance != null)
                CrosshairController.Instance.Show();
        }
    }
    
    public bool IsDialogueActive => isDialogueActive;
    public float GetGameTime() => gameTimer;
}