using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EventTriggerType
{
    Time,           // По таймеру
    OnLookAt,       // Когда игрок смотрит на объект
    OnEnterZone,    // Когда игрок входит в зону
    OnExitZone,     // Когда игрок выходит из зоны
    OnPowerOut,     // Когда отключили электричество
    OnPowerRestored // Когда включили электричество
}

[System.Serializable]
public class ScheduledEvent
{
    public string eventName;
    public EventTriggerType triggerType = EventTriggerType.Time;
    
    // Для Time
    public float startTime = 0f;
    public float activationDelay = 0f;
    
    // Для OnLookAt
    public GameObject targetObject;
    public float lookAngle = 15f;
    public float lookDuration = 1f;
    public bool checkObstacle = true;
    public LayerMask obstacleLayer = -1;
    public float maxLookDistance = 5f; // Максимальная дистанция для срабатывания
    
    // Для OnEnterZone / OnExitZone
    public Collider zoneCollider;
    
    public bool played = false;
    public bool isActive = false;
    
    public UnityEvent onTrigger;
}

public class EventScheduler : MonoBehaviour
{
    [Header("Расписание событий")]
    public List<ScheduledEvent> scheduledEvents;
    public bool autoStart = true;
    
    private float gameTimer = 0f;
    private bool isRunning = false;
    private Camera playerCamera;
    private Transform playerTransform;
    
    private Dictionary<GameObject, float> lookTimers = new Dictionary<GameObject, float>();
    
    void Start()
    {
        playerCamera = Camera.main;
        playerTransform = playerCamera?.transform;
        
        if (autoStart)
            StartScheduler();
    }
    
    void Update()
    {
        if (!isRunning) return;
        
        gameTimer += Time.deltaTime;
        
        foreach (ScheduledEvent evt in scheduledEvents)
        {
            if (evt.played) continue;
            
            if (!evt.isActive && gameTimer >= evt.activationDelay)
            {
                evt.isActive = true;
                Debug.Log($"✅ Ивент '{evt.eventName}' активирован на {evt.activationDelay} секунде");
            }
            
            if (!evt.isActive) continue;
            
            bool shouldTrigger = false;
            
            switch (evt.triggerType)
            {
                case EventTriggerType.Time:
                    shouldTrigger = gameTimer >= evt.startTime;
                    break;
                    
                case EventTriggerType.OnLookAt:
                    shouldTrigger = CheckLookAt(evt);
                    break;
                    
                case EventTriggerType.OnEnterZone:
                    shouldTrigger = CheckEnterZone(evt);
                    break;
                    
                case EventTriggerType.OnExitZone:
                    shouldTrigger = CheckExitZone(evt);
                    break;
                    
                case EventTriggerType.OnPowerOut:
                    shouldTrigger = (PowerManager.Instance != null && !PowerManager.Instance.HasPower());
                    break;
                    
                case EventTriggerType.OnPowerRestored:
                    shouldTrigger = (PowerManager.Instance != null && PowerManager.Instance.HasPower());
                    break;
            }
            
            if (shouldTrigger)
            {
                evt.played = true;
                Debug.Log($"⚡ СОБЫТИЕ: {evt.eventName} (Тип: {evt.triggerType}) на {gameTimer:F1} секунде");
                evt.onTrigger?.Invoke();
            }
        }
    }
    
    private bool CheckLookAt(ScheduledEvent evt)
    {
        if (evt.targetObject == null || playerCamera == null) return false;
        
        // Проверка расстояния
        float distanceToTarget = Vector3.Distance(playerCamera.transform.position, evt.targetObject.transform.position);
        
        // Если задана максимальная дистанция и игрок дальше - не триггерим
        if (evt.maxLookDistance > 0 && distanceToTarget > evt.maxLookDistance)
        {
            if (lookTimers.ContainsKey(evt.targetObject))
                lookTimers[evt.targetObject] = 0f;
            return false;
        }
        
        Vector3 directionToTarget = (evt.targetObject.transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
        
        if (angle < evt.lookAngle)
        {
            // Проверка препятствия
            if (evt.checkObstacle && IsObstacleBetween(evt.targetObject, evt.obstacleLayer))
            {
                if (lookTimers.ContainsKey(evt.targetObject))
                    lookTimers[evt.targetObject] = 0f;
                return false;
            }
            
            if (!lookTimers.ContainsKey(evt.targetObject))
                lookTimers[evt.targetObject] = 0f;
            
            lookTimers[evt.targetObject] += Time.deltaTime;
            
            if (lookTimers[evt.targetObject] >= evt.lookDuration)
            {
                lookTimers[evt.targetObject] = 0f;
                Debug.Log($"👀 Смотрю на {evt.targetObject.name} (расстояние: {distanceToTarget:F1})");
                return true;
            }
        }
        else
        {
            if (lookTimers.ContainsKey(evt.targetObject))
                lookTimers[evt.targetObject] = 0f;
        }
        
        return false;
    }
    
    private bool IsObstacleBetween(GameObject target, LayerMask obstacleLayer)
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = target.transform.position - origin;
        float distance = direction.magnitude;
        
        LayerMask mask = obstacleLayer.value != -1 ? obstacleLayer : ~LayerMask.GetMask("Interactable");
        
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distance, mask))
        {
            if (hit.collider.gameObject != target)
            {
                Debug.DrawRay(origin, direction, Color.red, 0.5f);
                return true;
            }
        }
        
        Debug.DrawRay(origin, direction, Color.green, 0.5f);
        return false;
    }
    
    private bool CheckEnterZone(ScheduledEvent evt)
    {
        if (evt.zoneCollider == null || playerTransform == null) return false;
        return evt.zoneCollider.bounds.Contains(playerTransform.position);
    }
    
    private bool CheckExitZone(ScheduledEvent evt)
    {
        if (evt.zoneCollider == null || playerTransform == null) return false;
        return !evt.zoneCollider.bounds.Contains(playerTransform.position);
    }
    
    public void StartScheduler()
    {
        isRunning = true;
        gameTimer = 0f;
        
        foreach (ScheduledEvent evt in scheduledEvents)
        {
            evt.played = false;
            evt.isActive = (evt.activationDelay == 0f);
        }
        
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
        {
            evt.played = false;
            evt.isActive = (evt.activationDelay == 0f);
        }
        lookTimers.Clear();
        Debug.Log("EventScheduler сброшен");
    }
    
    public float GetGameTime() => gameTimer;
}