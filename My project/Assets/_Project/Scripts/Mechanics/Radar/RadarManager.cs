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

    [Header("Angle Display")]
    [SerializeField] private Text angleDisplayText;
    [SerializeField] private GameObject angleDisplayPanel;
    [SerializeField] private Color normalAngleColor = Color.white;
    [SerializeField] private Color warningAngleColor = Color.yellow;
    [SerializeField] private float maxAngleWarning = 45f;

    [Header("Destination Zone")]
    [SerializeField] private Color destinationZoneColor = new Color(1f, 0.5f, 0f, 0.5f);
    [SerializeField] private float destinationZoneWidth = 0.15f;

    [Header("Target Zone")]
    [SerializeField] private Color targetZoneColor = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private float targetZoneWidth = 0.12f;

    // Компоненты
    private Image targetZoneImage;
    private RectTransform targetZoneRect;
    private Image trajectoryLineImage;
    private RectTransform trajectoryLineRect;
    private Image editTrajectoryLineImage;
    private RectTransform editTrajectoryLineRect;

    // Данные
    private List<AircraftController> activeAircrafts = new List<AircraftController>();
    private AircraftController selectedAircraft;
    private float spawnTimer;
    private bool isInitialized = false;

    // Данные редактирования траектории
    private TrajectoryHandle trajectoryHandle;
    private string pendingAircraftID = null;
    private float currentPreviewDeltaAngle = 0f;

    private void Awake()
    {
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

        RectTransform prefabRect = aircraftPrefab.GetComponent<RectTransform>();
        if (prefabRect == null)
        {
            Debug.LogError("Префаб самолета НЕ является UI элементом! Нет RectTransform.");
            enabled = false;
            return;
        }

        CreateTrajectoryLine();
        CreateEditLine();
        CreateZones();
        CreateTrajectoryHandle();

        if (aircraftContainer == null)
        {
            InitializeContainer();
        }

        if (angleDisplayPanel != null)
            angleDisplayPanel.SetActive(false);

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

        RectTransform containerRect = go.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = Vector2.zero;
    }

    private void CreateTrajectoryLine()
    {
        if (trajectoryLinePrefab != null)
        {
            GameObject lineObj = Instantiate(trajectoryLinePrefab, radarArea);
            trajectoryLineImage = lineObj.GetComponent<Image>();
            trajectoryLineRect = lineObj.GetComponent<RectTransform>();

            if (trajectoryLineImage != null)
            {
                trajectoryLineImage.type = Image.Type.Tiled;
                trajectoryLineImage.sprite = CreateDashedSprite();
                trajectoryLineImage.pixelsPerUnitMultiplier = 100f;
                trajectoryLineImage.raycastTarget = false;
            }

            if (trajectoryLineRect != null)
            {
                trajectoryLineRect.anchorMin = Vector2.zero;
                trajectoryLineRect.anchorMax = Vector2.zero;
                trajectoryLineRect.pivot = new Vector2(0, 0.5f);
                trajectoryLineRect.anchoredPosition = Vector2.zero;
                trajectoryLineRect.localPosition = Vector3.zero;
                trajectoryLineRect.localScale = Vector3.one;
            }

            if (trajectoryLineImage != null)
                trajectoryLineImage.gameObject.SetActive(false);
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

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        editTrajectoryLineImage.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        editTrajectoryLineImage.color = editLineColor;
        editTrajectoryLineImage.raycastTarget = false;
        editTrajectoryLineImage.gameObject.SetActive(false);
    }

    private void CreateTrajectoryHandle()
    {
        // Удаляем старую ручку, если есть
        var oldHandle = radarArea.Find("TrajectoryHandle");
        if (oldHandle != null)
            Destroy(oldHandle.gameObject);

        GameObject handleObj = new GameObject("TrajectoryHandle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(radarArea, false);

        // Удаляем возможные лишние компоненты
        var outline = handleObj.GetComponent<Outline>();
        if (outline != null) Destroy(outline);
        var shadow = handleObj.GetComponent<Shadow>();
        if (shadow != null) Destroy(shadow);

        RectTransform handleRect = handleObj.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(0.4f, 0.4f);
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.zero;
        handleRect.pivot = new Vector2(0.5f, 0.5f);

        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.sprite = CreateCircleSprite();
        handleImg.color = Color.yellow;
        handleImg.raycastTarget = true;

        trajectoryHandle = handleObj.AddComponent<TrajectoryHandle>();
        trajectoryHandle.Initialize(this, radarArea);
        trajectoryHandle.Hide();
    }

    private Sprite CreateCircleSprite()
    {
        int size = 24;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;

        int center = size / 2;
        int radius = size / 2 - 2;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                    tex.SetPixel(x, y, white);
                else
                    tex.SetPixel(x, y, transparent);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private Sprite CreateDashedSprite()
    {
        int width = 32;
        int height = 4;

        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isColored = (x / 4) % 2 == 0;
                colors[y * width + x] = isColored ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    private void CreateZones()
    {
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
        if (!isInitialized || aircraftPrefab == null || aircraftContainer == null)
            return;

        GameObject go = Instantiate(aircraftPrefab, aircraftContainer);

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            Destroy(go);
            return;
        }

        AircraftController ac = go.GetComponent<AircraftController>();
        if (ac == null)
        {
            Destroy(go);
            return;
        }

        Vector2 start = GetRandomEdgePoint();
        Vector2 targetZone = GetRandomEdgePoint();
        Vector2 end = GetRandomEdgePoint();

        while (Vector2.Distance(start, end) < 0.3f)
        {
            end = GetRandomEdgePoint();
        }

        float speed = Random.Range(minSpeed, maxSpeed);

        ac.Initialize(radarArea, start, end, targetZone);
        ac.Speed = speed;

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
        for (int i = activeAircrafts.Count - 1; i >= 0; i--)
        {
            if (activeAircrafts[i] == null)
                activeAircrafts.RemoveAt(i);
        }

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

    private void HandleAircraftSelected(AircraftController ac)
    {
        if (selectedAircraft != null && selectedAircraft != ac)
            selectedAircraft.SetSelected(false);

        selectedAircraft = ac;

        ShowZone(targetZoneImage, targetZoneRect, ac.TargetZoneNorm, targetZoneWidth, targetZoneColor);

        if (infoText != null)
            infoText.text = ac.GetDescription();

        if (trajectoryLineImage != null)
        {
            trajectoryLineImage.gameObject.SetActive(true);
            UpdateTrajectoryLine();
        }

        if (trajectoryHandle != null)
            trajectoryHandle.ShowForAircraft(ac);
    }

    private void UpdateTrajectoryLine()
    {
        if (trajectoryLineImage == null || selectedAircraft == null)
        {
            if (trajectoryLineImage != null)
                trajectoryLineImage.gameObject.SetActive(false);
            return;
        }

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

        trajectoryLineRect.anchoredPosition = start;
        trajectoryLineRect.sizeDelta = new Vector2(distance, trajectoryLineWidth);
        trajectoryLineRect.localRotation = Quaternion.Euler(0, 0, angle);

        trajectoryLineImage.color = trajectoryLineColor;
        trajectoryLineImage.gameObject.SetActive(true);
    }

    private void ShowZone(Image zoneImage, RectTransform zoneRect, Vector2 normPos, float width, Color color)
    {
        if (zoneImage == null) return;

        Vector2 size = radarArea.rect.size;
        Vector2 center;
        Vector2 zoneSize;

        if (Mathf.Approximately(normPos.x, 0f))
        {
            center = new Vector2(0, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.15f);
            zoneRect.pivot = new Vector2(0, 0.5f);
        }
        else if (Mathf.Approximately(normPos.x, 1f))
        {
            center = new Vector2(size.x, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.15f);
            zoneRect.pivot = new Vector2(1, 0.5f);
        }
        else if (Mathf.Approximately(normPos.y, 0f))
        {
            center = new Vector2(normPos.x * size.x, 0);
            zoneSize = new Vector2(size.x * 0.15f, width * size.y);
            zoneRect.pivot = new Vector2(0.5f, 0);
        }
        else
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

    private void HideAllZones()
    {
        if (targetZoneImage != null) targetZoneImage.gameObject.SetActive(false);
    }

    private void HandleDestinationReached(AircraftController ac, bool hitTarget)
    {
        if (hitTarget)
            Debug.Log($"САМОЛЁТ {ac.AircraftID} УСПЕШНО ПРИБЫЛ В ЦЕЛЕВУЮ ЗОНУ!");
        else
            Debug.Log($"САМОЛЁТ {ac.AircraftID} ПРОМАХНУЛСЯ! Цель: {ac.TargetZoneNorm}, Прибыл: {ac.EndPosNorm}");
    }

    private void HandleAircraftDestroyed(AircraftController ac)
    {
        if (selectedAircraft == ac)
        {
            selectedAircraft = null;
            HideAllZones();
            if (trajectoryLineImage != null)
                trajectoryLineImage.gameObject.SetActive(false);
            if (trajectoryHandle != null)
                trajectoryHandle.Hide();
            if (infoText != null)
                infoText.text = "Выберите самолет";
        }

        activeAircrafts.Remove(ac);
    }

    public AircraftController GetAircraftByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var aircraft in activeAircrafts)
            if (aircraft != null && aircraft.AircraftID == id)
                return aircraft;
        return null;
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

    public bool IsAircraftExists(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        foreach (var aircraft in activeAircrafts)
            if (aircraft != null && aircraft.AircraftID == id)
                return true;
        return false;
    }

    // ========== МЕТОДЫ ДЛЯ РЕДАКТИРОВАНИЯ ТРАЕКТОРИИ ==========

    public void StartTrajectoryEditing(AircraftController aircraft, float originalAngle)
    {
        if (aircraft == null) return;
        pendingAircraftID = aircraft.AircraftID;
        currentPreviewDeltaAngle = 0f;
        ShowAngleDisplay(originalAngle);
    }

    public void UpdateTrajectoryPreview(Vector2 startPoint, Vector2 newEndPoint, float deltaAngle)
    {
        if (editTrajectoryLineImage == null) return;

        currentPreviewDeltaAngle = deltaAngle;

        Vector2 dir = newEndPoint - startPoint;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        editTrajectoryLineRect.anchoredPosition = startPoint;
        editTrajectoryLineRect.sizeDelta = new Vector2(distance, editLineWidth);
        editTrajectoryLineRect.localRotation = Quaternion.Euler(0, 0, angle);
        editTrajectoryLineImage.gameObject.SetActive(true);

        UpdateAngleDisplay(deltaAngle);
    }

    public void CancelTrajectoryEdit()
    {
        pendingAircraftID = null;
        currentPreviewDeltaAngle = 0f;

        if (editTrajectoryLineImage != null)
            editTrajectoryLineImage.gameObject.SetActive(false);

        HideAngleDisplay();
    }

    public void ShowAngleMessage(string aircraftID, float deltaAngle)
    {
        Debug.Log($"[ТРАЕКТОРИЯ] Самолёт {aircraftID}: угол отклонения {deltaAngle:F1}°");

        if (infoText != null)
        {
            string originalText = infoText.text;
            infoText.text = $"УГОЛ ВЫБРАН: {deltaAngle:F1}°\n(Подтвердите на радио)";
            CancelInvoke(nameof(RestoreInfoText));
            Invoke(nameof(RestoreInfoText), 2f);
        }
    }

    private void RestoreInfoText()
    {
        if (infoText != null && selectedAircraft != null)
            infoText.text = selectedAircraft.GetDescription();
        else if (infoText != null)
            infoText.text = "Выберите самолет";
    }

    private void ShowAngleDisplay(float originalAngle)
    {
        if (angleDisplayPanel != null)
            angleDisplayPanel.SetActive(true);

        if (angleDisplayText != null)
        {
            angleDisplayText.text = $"Отклонение: 0°";
            angleDisplayText.color = normalAngleColor;
        }
    }

    private void UpdateAngleDisplay(float deltaAngle)
    {
        if (angleDisplayText == null) return;

        string sign = deltaAngle >= 0 ? "+" : "";
        angleDisplayText.text = $"Отклонение: {sign}{deltaAngle:F1}°";
        angleDisplayText.color = Mathf.Abs(deltaAngle) > maxAngleWarning ? warningAngleColor : normalAngleColor;
    }

    private void HideAngleDisplay()
    {
        if (angleDisplayPanel != null)
            angleDisplayPanel.SetActive(false);
    }

    public RectTransform GetRadarArea() => radarArea;
    public Vector2 GetRadarSize() => radarArea != null ? radarArea.rect.size : Vector2.zero;

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
}