using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScheduledEvent
{
    public string eventName;           // Название события (для удобства)
    public float startTime;            // Время от запуска игры (в секундах)
    public UnityEvent onTrigger;       // Что произойдёт
    public bool played = false;        // Уже выполнилось?
}

public class EventScheduler : MonoBehaviour
{
    [Header("Расписание событий")]
    public List<ScheduledEvent> scheduledEvents;
    public bool autoStart = true;
    
    private float gameTimer = 0f;
    private bool isRunning = false;
    
    void Start()
    {
        if (autoStart)
            StartScheduler();
    }
    
    void Update()
    {
        if (!isRunning) return;
        
        gameTimer += Time.deltaTime;
        
        foreach (ScheduledEvent evt in scheduledEvents)
        {
            if (!evt.played && gameTimer >= evt.startTime)
            {
                evt.played = true;
                Debug.Log($"⚡ СОБЫТИЕ: {evt.eventName} на {evt.startTime} секунде");
                evt.onTrigger?.Invoke();
            }
        }
    }
    
    public void StartScheduler()
    {
        isRunning = true;
        gameTimer = 0f;
        Debug.Log("EventScheduler запущен");
    }
    
    public void StopScheduler()
    {
        isRunning = false;
        Debug.Log("EventScheduler остановлен");
    }
    
    public void ResetScheduler()
    {
        gameTimer = 0f;
        foreach (ScheduledEvent evt in scheduledEvents)
            evt.played = false;
        Debug.Log("EventScheduler сброшен");
    }
    
    public float GetGameTime() => gameTimer;
}