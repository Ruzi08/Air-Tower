using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarManager : MonoBehaviour
{
    [Header("Radar Settings")]
    [SerializeField] private RectTransform radarArea;
    [SerializeField] private GameObject aircraftPrefab;
    [SerializeField] private Transform aircraftContainer;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAircrafts = 10;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float maxSpeed = 0.5f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.04f;

    [Header("UI - Selection Display")]
    [SerializeField] private Text infoText;
    [SerializeField] private LineRenderer trajectoryLinePrefab;

    // Внутренние данные
    private List<AircraftController> activeAircrafts = new List<AircraftController>();
    private AircraftController selectedAircraft;
    private LineRenderer activeTrajectoryLine;
    private float spawnTimer;
    private bool isInitialized = false;

    private void Awake()
    {
        // Инициализируем контейнер ДО всего остального
        InitializeContainer();
    }

    private void Start()
    {
        Debug.Log("RadarManager Start");



        if (radarArea == null)
        {
            Debug.LogError("RadarManager: Radar Area не назначен!");
            enabled = false;
            return;
        }

        if (aircraftPrefab == null)
        {
            Debug.LogError("RadarManager: Aircraft Prefab не назначен!");
            enabled = false;
            return;
        }

        // Проверяем, что префаб содержит RectTransform (UI элемент)
        RectTransform prefabRect = aircraftPrefab.GetComponent<RectTransform>();
        if (prefabRect == null)
        {
            Debug.LogError("Префаб самолета НЕ является UI элементом! Нет RectTransform. Пересоздайте префаб как UI → Image.");
            enabled = false;
            return;
        }

        // Создаем линию траектории
        if (trajectoryLinePrefab != null)
        {
            activeTrajectoryLine = Instantiate(trajectoryLinePrefab, radarArea);
            activeTrajectoryLine.gameObject.SetActive(false);

            SetupLineRendererForUI();
        }

        // Убеждаемся, что контейнер существует
        if (aircraftContainer == null)
        {
            InitializeContainer();
        }

        isInitialized = true;

        if (trajectoryLinePrefab == null)
        {
            Debug.Log("pizdesc");
        }
    }

    private void InitializeContainer()
    {
        if (aircraftContainer != null) return;

        if (radarArea == null)
        {
            Debug.LogError("Не могу создать контейнер: radarArea не назначен");
            return;
        }

        var go = new GameObject("AircraftContainer", typeof(RectTransform));
        go.transform.SetParent(radarArea, false);
        aircraftContainer = go.transform;

        // Настраиваем RectTransform
        RectTransform containerRect = go.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
    }

    private void Update()
    {
        if (!isInitialized) return;

        HandleSpawning();
        CheckCollisions();
        UpdateTrajectoryLine();
    }

    private void HandleSpawning()
    {
        if (activeAircrafts.Count >= maxAircrafts) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnAircraft();
        }
    }

    private void SpawnAircraft()
    {
        Debug.Log("SpawnAircraft вызван");

        // ПРОВЕРЯЕМ ВСЕ УСЛОВИЯ
        if (!isInitialized)
        {
            Debug.LogError("RadarManager не инициализирован!");
            return;
        }

        if (aircraftPrefab == null)
        {
            Debug.LogError("Невозможно создать самолет: префаб не назначен");
            return;
        }

        if (aircraftContainer == null)
        {
            Debug.LogError("Невозможно создать самолет: контейнер не создан. Пытаюсь создать...");
            InitializeContainer();
            if (aircraftContainer == null) return;
        }

        // СОЗДАЕМ САМОЛЕТ
        GameObject go = Instantiate(aircraftPrefab, aircraftContainer);

        // Проверяем RectTransform
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Созданный объект не имеет RectTransform! Это не UI элемент!");
            Destroy(go);
            return;
        }

        AircraftController ac = go.GetComponent<AircraftController>();

        if (ac == null)
        {
            Debug.LogError("Префаб самолета не содержит компонент AircraftController!");
            Destroy(go);
            return;
        }

        // Генерируем случайные точки
        Vector2 start = GetRandomEdgePoint();
        Vector2 end = GetRandomEdgePoint();

        while (Vector2.Distance(start, end) < 0.3f)
        {
            end = GetRandomEdgePoint();
        }

        float speed = Random.Range(minSpeed, maxSpeed);

        // Настройка компонента
        ac.Initialize(radarArea, start, end);
        ac.Speed = speed;

        // Подписка на события
        ac.OnSelected += HandleAircraftSelected;
        ac.OnReachedDestination += HandleAircraftReachedDestination;
        ac.OnDestroyed += HandleAircraftDestroyed;

        activeAircrafts.Add(ac);
    }

    private Vector2 GetRandomEdgePoint()
    {
        int side = Random.Range(0, 4);
        float rand = Random.value;

        switch (side)
        {
            case 0: return new Vector2(0, rand);
            case 1: return new Vector2(1, rand);
            case 2: return new Vector2(rand, 0);
            default: return new Vector2(rand, 1);
        }
    }

    private void CheckCollisions()
    {
        // Очистка null элементов
        for (int i = activeAircrafts.Count - 1; i >= 0; i--)
        {
            if (activeAircrafts[i] == null)
            {
                activeAircrafts.RemoveAt(i);
            }
        }

        // Проверяем попарно
        for (int i = 0; i < activeAircrafts.Count; i++)
        {
            for (int j = i + 1; j < activeAircrafts.Count; j++)
            {
                var a1 = activeAircrafts[i];
                var a2 = activeAircrafts[j];

                if (a1 == null || a2 == null) continue;

                if (a1.WillCollideWith(a2, collisionRadius))
                {
                    Debug.Log($"Столкновение! {a1.name} и {a2.name}");
                    Destroy(a1.gameObject);
                    Destroy(a2.gameObject);
                }
            }
        }
    }

    private void SetupLineRendererForUI()
    {
        if (activeTrajectoryLine == null) return;

        // Важно для UI: линия должна быть в том же слое/порядке
        activeTrajectoryLine.sortingOrder = 1; // Поверх самолетов
        activeTrajectoryLine.useWorldSpace = false; // Используем локальные координаты родителя

        // Настройка ширины
        activeTrajectoryLine.startWidth = 2f;
        activeTrajectoryLine.endWidth = 2f;
    }

    private void HandleAircraftSelected(AircraftController ac)
    {
        Debug.Log($"=== ВЫБРАН САМОЛЕТ: {ac.name} ===");
        Debug.Log($"activeTrajectoryLine существует: {activeTrajectoryLine != null}");
        if (selectedAircraft != null && selectedAircraft != ac)
        {
            selectedAircraft.SetSelected(false);
        }

        selectedAircraft = ac;

        if (infoText != null)
        {
            infoText.text = ac.GetDescription();
        }

        if (activeTrajectoryLine != null)
        {
            activeTrajectoryLine.gameObject.SetActive(true);
            UpdateTrajectoryLine();
        }
    }

    private void UpdateTrajectoryLine()
    {
        if (activeTrajectoryLine == null || selectedAircraft == null)
        {
            if (activeTrajectoryLine != null) activeTrajectoryLine.gameObject.SetActive(false);
            return;
        }

        Vector3 startPos = selectedAircraft.StartPositionWorld;
        Vector3 endPos = selectedAircraft.EndPositionWorld;
        Vector3 currentPos = selectedAircraft.CurrentPosition;

        activeTrajectoryLine.positionCount = 2;
        activeTrajectoryLine.SetPosition(0, currentPos);
        activeTrajectoryLine.SetPosition(1, endPos);
    }

    private void HandleAircraftReachedDestination(AircraftController ac)
    {
    }

    private void HandleAircraftDestroyed(AircraftController ac)
    {
        if (selectedAircraft == ac)
        {
            selectedAircraft = null;
            if (activeTrajectoryLine != null) activeTrajectoryLine.gameObject.SetActive(false);
            if (infoText != null) infoText.text = "Выберите самолет";
        }

        activeAircrafts.Remove(ac);
    }

    private void OnDestroy()
    {
        foreach (var ac in activeAircrafts)
        {
            if (ac != null)
            {
                ac.OnSelected -= HandleAircraftSelected;
                ac.OnReachedDestination -= HandleAircraftReachedDestination;
                ac.OnDestroyed -= HandleAircraftDestroyed;
            }
        }
    }
}