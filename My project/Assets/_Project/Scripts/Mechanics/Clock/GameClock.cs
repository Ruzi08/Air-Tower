using UnityEngine;
using TMPro;
using System.Collections;

public class GameClock : MonoBehaviour
{
    [Header("Настройки времени")]
    [SerializeField] private float realSecondsPerGameHour = 900f; // 1 реальный час
    [SerializeField] private bool autoStart = true;
    
    [Header("Компоненты")]
    [SerializeField] private TextMeshProUGUI clockText;
    
    [Header("Визуальные эффекты")]
    [SerializeField] private Color normalColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color pulseColor = Color.white;
    [SerializeField] private bool enableColonBlink = false; // МИГАНИЕ ОТКЛЮЧЕНО
    [SerializeField] private bool enableMinutePulse = true;
    
    [Header("ЗВУКИ")]
    [SerializeField] private AudioSource audioSource; // Источник звука
    [SerializeField] private AudioClip hourChimeSound; // Звук на каждый час
    [SerializeField] private bool playSoundOnFullHour = true;
    [SerializeField] private bool playSoundOnShiftEnd = true; // Звук на 04:00
    [SerializeField] private AudioClip shiftEndSound; // Отдельный звук на конец смены (опционально)
    
    // Внутренние переменные
    private float currentGameTimeInHours = 0f;
    private bool isShiftActive = true;
    private int lastMinute = -1;
    private int lastPlayedHour = -1; // Для отслеживания, сыграл ли звук на этом часе
    private Coroutine pulseCoroutine;
    
    // Свойства
    public float CurrentTimeInHours => currentGameTimeInHours;
    public bool IsShiftActive => isShiftActive;
    public string CurrentTimeString => GetTimeString();
    
    void Start()
    {
        if (clockText == null)
        {
            Debug.LogError("Clock Text не назначен!");
            return;
        }
        
        // Проверяем AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (playSoundOnFullHour || playSoundOnShiftEnd))
            {
                Debug.LogWarning("AudioSource не найден! Добавь компонент AudioSource на часы");
            }
        }
        
        if (autoStart)
        {
            StartShift();
        }
    }
    
    void Update()
    {
        if (!isShiftActive) return;
        
        // Сохраняем старый час
        int oldHour = Mathf.FloorToInt(currentGameTimeInHours);
        
        // Обновляем время
        currentGameTimeInHours += Time.deltaTime / realSecondsPerGameHour;
        
        // Получаем новый час
        int newHour = Mathf.FloorToInt(currentGameTimeInHours);
        
        // Проверяем, наступил ли новый час (кроме 00:00)
        if (playSoundOnFullHour && newHour > oldHour && newHour >= 1 && newHour <= 3)
        {
            PlayHourChime(newHour);
        }
        
        // Проверяем окончание смены
        if (currentGameTimeInHours >= 4f)
        {
            currentGameTimeInHours = 4f;
            isShiftActive = false;
            OnShiftEnd();
        }
        
        // Обновляем дисплей
        UpdateDisplay();
    }
    
    void PlayHourChime(int hour)
    {
        // Чтобы звук не сыграл дважды за один час
        if (lastPlayedHour == hour) return;
        lastPlayedHour = hour;
        
        Debug.Log($"🕐 Бьют часы! {hour:00}:00");
        
        if (audioSource != null && hourChimeSound != null)
        {
            audioSource.PlayOneShot(hourChimeSound);
        }
        else if (audioSource != null && hourChimeSound == null)
        {
            // Если нет звука, просто пишем в консоль
            Debug.Log("Добавь AudioClip в поле Hour Chime Sound");
        }
    }
    
    void UpdateDisplay()
    {
        int hours = Mathf.FloorToInt(currentGameTimeInHours);
        int minutes = Mathf.FloorToInt((currentGameTimeInHours - hours) * 60f);
        minutes = Mathf.Clamp(minutes, 0, 59);
        
        // Форматируем строку (без мигания)
        string timeString = $"{hours:00}:{minutes:00}";
        
        clockText.text = timeString;
        
        // Эффект пульсации при смене минуты
        if (enableMinutePulse && lastMinute != minutes && minutes != 0)
        {
            lastMinute = minutes;
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseEffect());
        }
    }
    
    IEnumerator PulseEffect()
    {
        clockText.color = pulseColor;
        yield return new WaitForSeconds(0.1f);
        clockText.color = normalColor;
    }
    
    void OnShiftEnd()
    {
        Debug.Log($"=== Смена окончена! Время: {GetTimeString()} ===");
        
        // Звук на конец смены (04:00)
        if (playSoundOnShiftEnd && audioSource != null)
        {
            if (shiftEndSound != null)
            {
                audioSource.PlayOneShot(shiftEndSound);
            }
            else if (hourChimeSound != null)
            {
                audioSource.PlayOneShot(hourChimeSound);
            }
        }
    }
    
    string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentGameTimeInHours);
        int minutes = Mathf.FloorToInt((currentGameTimeInHours - hours) * 60f);
        minutes = Mathf.Clamp(minutes, 0, 59);
        return $"{hours:00}:{minutes:00}";
    }
    
    public void StartShift()
    {
        currentGameTimeInHours = 0f;
        isShiftActive = true;
        lastMinute = -1;
        lastPlayedHour = -1;
        UpdateDisplay();
        Debug.Log("Смена началась! 00:00");
    }
    
    public void ResetShift()
    {
        StartShift();
    }
    
    public void AddTime(float hours)
    {
        if (!isShiftActive) return;
        currentGameTimeInHours += hours;
        if (currentGameTimeInHours >= 4f)
        {
            currentGameTimeInHours = 4f;
            isShiftActive = false;
            OnShiftEnd();
        }
        UpdateDisplay();
    }
}