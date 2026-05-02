using UnityEngine;
using System.Collections;

public class EventAircraftSpawner : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private float spawnTimeSeconds = 1000f; // Время в секундах, когда появится самолёт
    [SerializeField] private string aircraftID = "BA04";     // ID самолёта
    [SerializeField] private Vector2 targetPoint = new Vector2(0f, 0.5f);
    [SerializeField] private float targetZoneRadius = 0.05f;// Целевая точка на радаре (0..1)

    [Header("Aircraft Settings")]
    [SerializeField] private float aircraftSpeed = 120f;
    [SerializeField] private Vector2 customStartPoint = new Vector2(1f,1f); // Опциональная стартовая точка

    [Header("References")]
    [SerializeField] private RadarManager radarManager;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool hasTriggered = false;

    private float timer = 0f;
    private bool isActive = false;

    private void Start()
    {
        // Ищем RadarManager если не назначен
        if (radarManager == null)
        {
            radarManager = FindFirstObjectByType<RadarManager>();
            if (radarManager == null)
            {
                Debug.LogError("EventAircraftSpawner: RadarManager не найден!");
                enabled = false;
                return;
            }
        }

        // Активируем таймер
        isActive = true;
        timer = 0f;
        hasTriggered = false;

        if (showDebugLogs)
            Debug.Log($"✈️ EventAircraftSpawner: Ивент активирован, самолёт появится через {spawnTimeSeconds} секунд");
    }

    private void Update()
    {
        if (!isActive || hasTriggered) return;

        timer += Time.deltaTime;

        if (timer >= spawnTimeSeconds)
        {
            TriggerSpawnEvent();
        }
    }

    private void TriggerSpawnEvent()
    {
        hasTriggered = true;

        if (showDebugLogs)
            Debug.Log($"🚀 EventAircraftSpawner: Спавним самолёт {aircraftID} в точке {targetPoint}");

        if (radarManager != null)
        {
            radarManager.SpawnEventAircraft(aircraftID, targetPoint, targetZoneRadius, aircraftSpeed, customStartPoint);
        }
        else
        {
            Debug.LogError("EventAircraftSpawner: RadarManager не назначен!");
        }
    }

    // Ручной вызов для тестирования
    [ContextMenu("Test Spawn Now")]
    public void TestSpawnNow()
    {
        TriggerSpawnEvent();
    }

    // Сброс ивента для повторного использования
    public void ResetEvent()
    {
        timer = 0f;
        hasTriggered = false;
        if (showDebugLogs)
            Debug.Log("EventAircraftSpawner: Ивент сброшен");
    }
}