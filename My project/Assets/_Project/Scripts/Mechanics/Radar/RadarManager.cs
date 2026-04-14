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

    [Header("Trajectory Editing")]
    [SerializeField] private Color editLineColor = Color.yellow;
    [SerializeField] private float editLineWidth = 0.1f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Destination Zone")]
    [SerializeField] private Color destinationZoneColor = new Color(1f, 0.5f, 0f, 0.5f); // Оранжевый полупрозрачный
    [SerializeField] private float destinationZoneWidth = 0.15f;

    [Header("Target Zone (куда НАДО)")]
    [SerializeField] private Color targetZoneColor = new Color(0f, 1f, 0f, 0.4f); // Зелёный
    [SerializeField] private float targetZoneWidth = 0.12f;

    private Image targetZoneImage;
    private RectTransform targetZoneRect;

    private float currentEditAngle = 0f;

    private Image editTrajectoryLineImage;
    private RectTransform editTrajectoryLineRect;
    private bool isEditingMode = false;
    private Vector2? pendingTrajectory = null; // Ожидающая траектория для текущего самолета
    private string pendingAircraftID = null;

    // Внутренние данные
    private List<AircraftController> activeAircrafts = new List<AircraftController>();
    private AircraftController selectedAircraft;
    private Image trajectoryLineImage;
    private RectTransform trajectoryLineRect;
    private float spawnTimer;
    private bool isInitialized = false;

    private Image destinationZoneImage;
    private RectTransform destinationZoneRect;

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
        CreateEditLine();
        CreateZones();
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
        HandleTrajectoryEditing();
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
        Vector2 targetZone = GetRandomEdgePoint();
        Vector2 end = GetRandomEdgePoint();

        while (Vector2.Distance(start, end) < 0.3f)
        {
            end = GetRandomEdgePoint();
        }

        float speed = Random.Range(minSpeed, maxSpeed);

        // Настройка компонента
        ac.Initialize(radarArea, start, end, targetZone);
        ac.Speed = speed;

        // Подписка на события
        ac.OnSelected += HandleAircraftSelected;
        ac.OnDestroyed += HandleAircraftDestroyed;
        ac.OnDestinationReached += HandleDestinationReached;

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
        if (selectedAircraft != ac)
        {
            CancelEditing();
        }

        if (selectedAircraft != null && selectedAircraft != ac)
        {
            selectedAircraft.SetSelected(false);
        }

        selectedAircraft = ac;
        ShowZone(targetZoneImage, targetZoneRect, ac.TargetZoneNorm, targetZoneWidth, targetZoneColor);

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

    private void HandleDestinationReached(AircraftController ac, bool hitTarget)
    {
        if (hitTarget)
        {
            Debug.Log($"САМОЛЁТ {ac.AircraftID} УСПЕШНО ПРИБЫЛ В ЦЕЛЕВУЮ ЗОНУ!");


            // TODO: Добавить очки, звук успеха, эффекты
        }
        else
        {
            Debug.Log($"САМОЛЁТ {ac.AircraftID} ПРОМАХНУЛСЯ! Цель: {ac.TargetZoneNorm}, Прибыл: {ac.EndPosNorm}");


            // TODO: Штраф, звук ошибки, эффекты
        }
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
                return true;
            }
        }

        return false;
    }

    private void HandleAircraftDestroyed(AircraftController ac)
    {
        if (selectedAircraft == ac)
        {
            selectedAircraft = null;
            HideAllZones();
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
                ac.OnDestroyed -= HandleAircraftDestroyed;
            }
        }
    }

    private void CreateEditLine()
    {
        GameObject lineObj = new GameObject("EditTrajectoryLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObj.transform.SetParent(radarArea, false);

        editTrajectoryLineImage = lineObj.GetComponent<Image>();
        editTrajectoryLineRect = lineObj.GetComponent<RectTransform>();

        editTrajectoryLineRect.anchorMin = Vector2.zero;
        editTrajectoryLineRect.anchorMax = Vector2.zero;
        editTrajectoryLineRect.pivot = new Vector2(0, 0.5f);

        // Создаем простой спрайт
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        editTrajectoryLineImage.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);

        editTrajectoryLineImage.color = editLineColor;
        editTrajectoryLineImage.raycastTarget = false;
        editTrajectoryLineImage.gameObject.SetActive(false);

    }

    private void HandleTrajectoryEditing()
    {
        if (!isEditingMode || selectedAircraft == null) return;

        Vector2 startPos = selectedAircraft.CurrentPosition;

        // Поворот колесиком мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            currentEditAngle += scroll * rotationSpeed;
            Debug.Log($"Угол изменён: {currentEditAngle:F1}°");
        }

        // Вычисляем направление по углу
        float angleRad = currentEditAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // Находим точку на границе
        Vector2 edgePoint = GetEdgePoint(startPos, direction);

        // Обновляем линию
        UpdateEditLine(startPos, edgePoint);

        // Сохраняем
        Vector2 size = radarArea.rect.size;
        pendingTrajectory = new Vector2(edgePoint.x / size.x, edgePoint.y / size.y);
        pendingAircraftID = selectedAircraft.AircraftID;

        // Фиксация по ЛКМ или Enter
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log($"Траектория зафиксирована: {pendingTrajectory.Value}");
            StopEditingMode();
        }

        // Отмена по ESC или ПКМ
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CancelEditing();
        }
    }

    private Vector2 GetEdgePoint(Vector2 origin, Vector2 direction)
    {
        Vector2 size = radarArea.rect.size;

        // Если направление нулевое - возвращаем точку справа
        if (direction.magnitude < 0.1f)
        {
            return new Vector2(size.x, origin.y);
        }

        direction.Normalize();

        // Находим пересечение с четырьмя границами
        float tMin = float.MaxValue;

        // Правая граница (x = width)
        if (direction.x > 0)
        {
            float t = (size.x - origin.x) / direction.x;
            if (t > 0 && t < tMin) tMin = t;
        }

        // Левая граница (x = 0)
        if (direction.x < 0)
        {
            float t = -origin.x / direction.x;
            if (t > 0 && t < tMin) tMin = t;
        }

        // Верхняя граница (y = height)
        if (direction.y > 0)
        {
            float t = (size.y - origin.y) / direction.y;
            if (t > 0 && t < tMin) tMin = t;
        }

        // Нижняя граница (y = 0)
        if (direction.y < 0)
        {
            float t = -origin.y / direction.y;
            if (t > 0 && t < tMin) tMin = t;
        }

        Vector2 result = origin + direction * tMin;

        // Ограничиваем размерами (на всякий случай)
        result.x = Mathf.Clamp(result.x, 0, size.x);
        result.y = Mathf.Clamp(result.y, 0, size.y);

        return result;
    }

    private void UpdateEditLine(Vector2 start, Vector2 end)
    {
        if (editTrajectoryLineImage == null || editTrajectoryLineRect == null) return;

        Vector2 dir = end - start;
        float dist = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        editTrajectoryLineRect.anchoredPosition = start;
        editTrajectoryLineRect.sizeDelta = new Vector2(dist, editLineWidth);
        editTrajectoryLineRect.localRotation = Quaternion.Euler(0, 0, angle);

        editTrajectoryLineImage.color = editLineColor;
        editTrajectoryLineImage.gameObject.SetActive(true);
    }

    private void StopEditingMode()
    {
        isEditingMode = false;
        editTrajectoryLineImage.gameObject.SetActive(false);
    }

    private void CancelEditing()
    {
        pendingTrajectory = null;
        pendingAircraftID = null;
        StopEditingMode();
    }

    public void StartEditMode()
    {
        if (selectedAircraft == null) return;

        isEditingMode = true;
        pendingTrajectory = null;

        // Инициализируем угол текущим направлением самолёта
        Vector2 currentDir = selectedAircraft.GetDirection();
        currentEditAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;

        editTrajectoryLineImage.gameObject.SetActive(true);

        // Показываем начальную линию
        Vector2 start = selectedAircraft.CurrentPosition;
        Vector2 edgePoint = GetEdgePoint(start, currentDir);
        UpdateEditLine(start, edgePoint);

        Debug.Log($"Начальный угол: {currentEditAngle:F1}°");
    }

    public void ApplyPendingTrajectory(string aircraftID)
    {
        if (pendingTrajectory.HasValue && pendingAircraftID == aircraftID)
        {
            AircraftController ac = GetAircraftByID(aircraftID);
            if (ac != null)
            {
                ac.SetNewDestination(pendingTrajectory.Value);
                pendingTrajectory = null;
                pendingAircraftID = null;
                Debug.Log($"Траектория применена для {aircraftID}");
            }
        }
    }

    public AircraftController GetAircraftByID(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (var aircraft in activeAircrafts)
        {
            if (aircraft != null && aircraft.AircraftID == id)
                return aircraft;
        }

        return null;
    }

    private void ShowZone(Image zoneImage, RectTransform zoneRect, Vector2 normPos, float width, Color color)
    {
        if (zoneImage == null) return;

        Vector2 size = radarArea.rect.size;
        Vector2 center;
        Vector2 zoneSize;

        // Определяем сторону
        if (Mathf.Approximately(normPos.x, 0f)) // Левая
        {
            center = new Vector2(0, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.15f);
            zoneRect.pivot = new Vector2(0, 0.5f);
        }
        else if (Mathf.Approximately(normPos.x, 1f)) // Правая
        {
            center = new Vector2(size.x, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.15f);
            zoneRect.pivot = new Vector2(1, 0.5f);
        }
        else if (Mathf.Approximately(normPos.y, 0f)) // Нижняя
        {
            center = new Vector2(normPos.x * size.x, 0);
            zoneSize = new Vector2(size.x * 0.15f, width * size.y);
            zoneRect.pivot = new Vector2(0.5f, 0);
        }
        else // Верхняя
        {
            center = new Vector2(normPos.x * size.x, size.y);
            zoneSize = new Vector2(size.x * 0.15f, width * size.y);
            zoneRect.pivot = new Vector2(0.5f, 1);
        }

        zoneRect.anchoredPosition = center;
        zoneRect.sizeDelta = zoneSize;
        zoneImage.color = color;
        zoneImage.gameObject.SetActive(true);
    }

    private string GetEdgeName(Vector2 norm)
    {
        if (Mathf.Approximately(norm.x, 0f)) return "ЛЕВО";
        if (Mathf.Approximately(norm.x, 1f)) return "ПРАВО";
        if (Mathf.Approximately(norm.y, 0f)) return "НИЗ";
        if (Mathf.Approximately(norm.y, 1f)) return "ВЕРХ";
        return "НЕИЗВЕСТНО";
    }


    private void CreateZones()
    {
        // Целевая зона (зелёная)
        GameObject targetObj = new GameObject("TargetZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        targetObj.transform.SetParent(radarArea, false);
        targetZoneImage = targetObj.GetComponent<Image>();
        targetZoneRect = targetObj.GetComponent<RectTransform>();
        SetupZone(targetZoneImage, targetZoneRect, targetZoneColor);

    }

    private void SetupZone(Image img, RectTransform rect, Color color)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        img.color = color;
        img.raycastTarget = false;
        img.gameObject.SetActive(false);
    }

    private void HideAllZones()
    {
        if (targetZoneImage != null) targetZoneImage.gameObject.SetActive(false);
    }
}