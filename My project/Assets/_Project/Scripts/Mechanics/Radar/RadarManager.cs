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
    [SerializeField] private GameObject trajectoryLinePrefab;
    [SerializeField] private float trajectoryLineWidth = 2f;
    [SerializeField] private Color trajectoryLineColor = Color.green;

    // Внутренние данные
    private List<AircraftController> activeAircrafts = new List<AircraftController>();
    private AircraftController selectedAircraft;
    private Image trajectoryLineImage;
    private RectTransform trajectoryLineRect;
    private float spawnTimer;
    private bool isInitialized = false;

    private void Awake()
    {
        // Инициализируем контейнер ДО всего остального
        InitializeContainer();
    }

    private void Start()
    { 


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
            GameObject lineObj = Instantiate(trajectoryLinePrefab, radarArea);

            trajectoryLineImage = lineObj.GetComponent<Image>();
            trajectoryLineRect = lineObj.GetComponent<RectTransform>();

            trajectoryLineImage.type = Image.Type.Tiled;
            trajectoryLineImage.sprite = CreateDashedSprite();
            trajectoryLineImage.pixelsPerUnitMultiplier = 100f;

            // ВАЖНО: Сбрасываем всё на нули
            trajectoryLineRect.anchorMin = Vector2.zero;
            trajectoryLineRect.anchorMax = Vector2.zero;
            trajectoryLineRect.pivot = new Vector2(0, 0.5f);
            trajectoryLineRect.anchoredPosition = Vector2.zero;
            trajectoryLineRect.localPosition = Vector3.zero;
            trajectoryLineRect.localScale = Vector3.one;

            trajectoryLineImage.color = Color.green;
            trajectoryLineImage.raycastTarget = false;

            trajectoryLineImage.gameObject.SetActive(false);
        }

        // Убеждаемся, что контейнер существует
        if (aircraftContainer == null)
        {
            InitializeContainer();
        }

        isInitialized = true;
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
                    Destroy(a1.gameObject);
                    Destroy(a2.gameObject);
                }
            }
        }
    }

    public void SelectAircraftByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("SelectAircraftByID: передан пустой ID");
            return;
        }

        foreach (var aircraft in activeAircrafts)
        {
            if (aircraft != null && aircraft.AircraftID == id)
            {
                HandleAircraftSelected(aircraft);
                return;
            }
        }

        Debug.LogWarning($"SelectAircraftByID: самолет с ID {id} не найден");
    }

    private void HandleAircraftSelected(AircraftController ac)
    {
        if (selectedAircraft != null && selectedAircraft != ac)
        {
            selectedAircraft.SetSelected(false);
        }

        selectedAircraft = ac;

        if (infoText != null)
        {
            infoText.text = ac.GetDescription();
        }

        if (trajectoryLineImage != null)
        {
            trajectoryLineImage.gameObject.SetActive(true);
            UpdateTrajectoryLine();
        }
    }

    private void UpdateTrajectoryLine()
    {
        if (trajectoryLineImage == null || selectedAircraft == null)
        {
            if (trajectoryLineImage != null)
                trajectoryLineImage.gameObject.SetActive(false);
            return;
        }

        // Получаем позиции в UI координатах
        Vector2 start = selectedAircraft.CurrentPosition;
        Vector2 end = selectedAircraft.EndPositionWorld;


        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance < 1f)
        {
            trajectoryLineImage.gameObject.SetActive(false);
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // ВАЖНО: Используем anchoredPosition (UI координаты)
        trajectoryLineRect.anchoredPosition = start;
        trajectoryLineRect.sizeDelta = new Vector2(distance, trajectoryLineWidth);
        trajectoryLineRect.localRotation = Quaternion.Euler(0, 0, angle);

        trajectoryLineImage.color = trajectoryLineColor;
        trajectoryLineImage.gameObject.SetActive(true);

    }

    private Sprite CreateDashedSprite()
    {
        int width = 32;
        int height = 4;

        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        // Создаём паттерн: 4 пикселя цвет, 4 пикселя прозрачный
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Каждые 8 пикселей меняем цвет
                bool isColored = (x / 4) % 2 == 0;
                colors[y * width + x] = isColored ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    private void HandleAircraftReachedDestination(AircraftController ac)
    {
    }

    public bool IsAircraftExists(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("IsAircraftExists: передан пустой ID");
            return false;
        }

        foreach (var aircraft in activeAircrafts)
        {
            if (aircraft != null && aircraft.AircraftID == id)
            {
                Debug.Log($"Самолет с ID {id} найден на радаре");
                return true;
            }
        }

        Debug.Log($"Самолет с ID {id} не найден. Активных самолетов: {activeAircrafts.Count}");
        return false;
    }

    private void HandleAircraftDestroyed(AircraftController ac)
    {
        if (selectedAircraft == ac)
        {
            selectedAircraft = null;
            if (trajectoryLineImage != null)
                trajectoryLineImage.gameObject.SetActive(false);
            if (infoText != null)
                infoText.text = "Выберите самолет";
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