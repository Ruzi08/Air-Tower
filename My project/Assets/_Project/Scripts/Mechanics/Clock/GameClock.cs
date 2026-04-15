using UnityEngine;
using TMPro;
using System.Collections;

public class GameClock : MonoBehaviour
{
    [Header("Настройки времени")]
    [SerializeField] private float realSecondsPerGameHour = 900f;
    [SerializeField] private bool autoStart = true;
    
    [Header("Компоненты")]
    [SerializeField] private TextMeshProUGUI clockText;
    
    [Header("Визуальные эффекты")]
    [SerializeField] private Color normalColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color pulseColor = Color.white;
    [SerializeField] private bool enableColonBlink = false;
    [SerializeField] private bool enableMinutePulse = true;
    
    [Header("ЗВУКИ (3D)")]
    [SerializeField] private SimpleSound hourChimeSound;   // Звук на каждый час (с 1 до 3)
    [SerializeField] private SimpleSound shiftEndSound;    // Звук на конец смены (04:00)
    [SerializeField] private bool playSoundOnFullHour = true;
    [SerializeField] private bool playSoundOnShiftEnd = true;
    
    // Внутренние переменные
    private float currentGameTimeInHours = 0f;
    private bool isShiftActive = true;
    private int lastMinute = -1;
    private int lastPlayedHour = -1;
    private Coroutine pulseCoroutine;
    
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
        
        if (autoStart)
        {
            StartShift();
        }
    }
    
    void Update()
    {
        if (!isShiftActive) return;
        
        int oldHour = Mathf.FloorToInt(currentGameTimeInHours);
        
        currentGameTimeInHours += Time.deltaTime / realSecondsPerGameHour;
        
        int newHour = Mathf.FloorToInt(currentGameTimeInHours);
        
        // Проверяем, наступил ли новый час (с 1 до 3)
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
        
        UpdateDisplay();
    }
    
    void PlayHourChime(int hour)
    {
        if (lastPlayedHour == hour) return;
        lastPlayedHour = hour;
        
        Debug.Log($"🕐 Бьют часы! {hour:00}:00");
        
        if (hourChimeSound != null)
        {
            hourChimeSound.Play();
        }
    }
    
    void UpdateDisplay()
    {
        int hours = Mathf.FloorToInt(currentGameTimeInHours);
        int minutes = Mathf.FloorToInt((currentGameTimeInHours - hours) * 60f);
        minutes = Mathf.Clamp(minutes, 0, 59);
        
        string timeString = $"{hours:00}:{minutes:00}";
        clockText.text = timeString;
        
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
        
        if (playSoundOnShiftEnd && shiftEndSound != null)
        {
            shiftEndSound.Play();
        }
        else if (playSoundOnShiftEnd && hourChimeSound != null)
        {
            hourChimeSound.Play();
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