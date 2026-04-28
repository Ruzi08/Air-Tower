using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class RadarManager : MonoBehaviour
{
    private enum RadarEdgeSide
    {
        Left = 0,
        Right = 1,
        Bottom = 2,
        Top = 3
    }

    private class AircraftTrajectoryVisual
    {
        public Image Image;
        public RectTransform Rect;
    }

    [Header("Radar Settings")]
    [SerializeField] private RectTransform radarArea;
    [SerializeField] private GameObject aircraftPrefab;
    [SerializeField] private Transform aircraftContainer;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxAircrafts = 10;
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float maxSpeed = 0.5f;
    [SerializeField] private float spawnInset = 0.05f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.04f;
    [SerializeField] private bool predictCollisionForWholeRoute = true;
    [SerializeField] private float collisionPredictionTime = 5f;
    [SerializeField] private Color collisionWarningTrajectoryColor = new Color(1f, 0.6f, 0f, 1f);
    [SerializeField] private float criticalCollisionTime = 1.5f;
    [SerializeField] private Color criticalCollisionTrajectoryColor = Color.red;

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
    [SerializeField] private Color destinationZoneColor = new Color(1f, 0.5f, 0f, 0.5f);
    [SerializeField] private float destinationZoneWidth = 0.15f;

    [Header("Target Zone (РєСѓРґР° РќРђР”Рћ)")]
    [SerializeField] private Color targetZoneColor = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private float targetZoneWidth = 0.12f;
    [SerializeField] private float edgeDetectionEpsilon = 0.001f;

    private Image targetZoneImage;
    private RectTransform targetZoneRect;

    private float currentEditAngle = 0f;
    private float lastMouseX;
    private bool isDraggingTrajectory = false;

    private Image editTrajectoryLineImage;
    private RectTransform editTrajectoryLineRect;
    private bool isEditingMode = false;
    private bool isPendingTrajectoryVisible = false;
    private Vector2? pendingTrajectory = null;
    private string pendingAircraftID = null;

    private readonly List<AircraftController> activeAircrafts = new List<AircraftController>();
    private readonly Dictionary<AircraftController, AircraftTrajectoryVisual> aircraftTrajectoryLines = new Dictionary<AircraftController, AircraftTrajectoryVisual>();
    private readonly HashSet<AircraftController> collisionWarningAircrafts = new HashSet<AircraftController>();
    private readonly HashSet<AircraftController> criticalCollisionAircrafts = new HashSet<AircraftController>();
    private AircraftController selectedAircraft;
    private float spawnTimer;
    private bool isInitialized = false;

    private Image destinationZoneImage;
    private RectTransform destinationZoneRect;

    private void Awake()
    {
        InitializeContainer();
    }

    private void Start()
    {
        if (radarArea == null)
        {
            Debug.LogError("RadarManager: Radar Area РЅРµ РЅР°Р·РЅР°С‡РµРЅ!");
            enabled = false;
            return;
        }

        if (aircraftPrefab == null)
        {
            Debug.LogError("RadarManager: Aircraft Prefab РЅРµ РЅР°Р·РЅР°С‡РµРЅ!");
            enabled = false;
            return;
        }

        RectTransform prefabRect = aircraftPrefab.GetComponent<RectTransform>();
        if (prefabRect == null)
        {
            Debug.LogError("РџСЂРµС„Р°Р± СЃР°РјРѕР»РµС‚Р° РќР• СЏРІР»СЏРµС‚СЃСЏ UI СЌР»РµРјРµРЅС‚РѕРј! РќРµС‚ RectTransform. РџРµСЂРµСЃРѕР·РґР°Р№С‚Рµ РїСЂРµС„Р°Р± РєР°Рє UI в†’ Image.");
            enabled = false;
            return;
        }

        CreateEditLine();
        CreateZones();

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
            Debug.LogError("РќРµ РјРѕРіСѓ СЃРѕР·РґР°С‚СЊ РєРѕРЅС‚РµР№РЅРµСЂ: radarArea РЅРµ РЅР°Р·РЅР°С‡РµРЅ");
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
        if (!isInitialized)
        {
            return;
        }

        if (aircraftPrefab == null)
        {
            return;
        }

        if (aircraftContainer == null)
        {
            InitializeContainer();
            if (aircraftContainer == null) return;
        }

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

        int startSide = Random.Range(0, 4);
        Vector2 start = GetEdgePointOnSide(startSide, spawnInset);

        Vector2 targetZone = GetRandomEdgePoint();
        while ((int)GetEdgeSide(targetZone) == startSide) targetZone = GetRandomEdgePoint();

        Vector2 end = GetRandomEdgePoint();
        while ((int)GetEdgeSide(end) == startSide) end = GetRandomEdgePoint();

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
        CreateTrajectoryLineForAircraft(ac);
    }

    private Vector2 GetRandomEdgePoint()
    {
        int side = Random.Range(0, 4);
        return GetEdgePointOnSide(side, 0f);
    }

    private Vector2 GetEdgePointOnSide(int side, float inset)
    {
        inset = Mathf.Clamp01(inset);
        float rand = Random.value;
        float clampedRand = Mathf.Lerp(inset, 1f - inset, rand);

        switch (side)
        {
            case 0: return new Vector2(inset, clampedRand);
            case 1: return new Vector2(1f - inset, clampedRand);
            case 2: return new Vector2(clampedRand, inset);
            default: return new Vector2(clampedRand, 1f - inset);
        }
    }

    private void CheckCollisions()
    {
        collisionWarningAircrafts.Clear();
        criticalCollisionAircrafts.Clear();

        for (int i = activeAircrafts.Count - 1; i >= 0; i--)
        {
            if (activeAircrafts[i] == null)
            {
                activeAircrafts.RemoveAt(i);
            }
        }

        for (int i = 0; i < activeAircrafts.Count; i++)
        {
            for (int j = i + 1; j < activeAircrafts.Count; j++)
            {
                var a1 = activeAircrafts[i];
                var a2 = activeAircrafts[j];

                if (a1 == null || a2 == null) continue;

                if (WillAircraftsCollideSoon(a1, a2))
                {
                    collisionWarningAircrafts.Add(a1);
                    collisionWarningAircrafts.Add(a2);


                    if (WillAircraftsCollideWithinTime(a1, a2, criticalCollisionTime))
                    {
                        criticalCollisionAircrafts.Add(a1);
                        criticalCollisionAircrafts.Add(a2);
                    }
                }

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

    }

    private void HandleAircraftSelected(AircraftController ac)
    {
        if (selectedAircraft != ac)
        {
            isPendingTrajectoryVisible = false;
            CancelEditing();
        }

        if (selectedAircraft != null && selectedAircraft != ac)
        {
            selectedAircraft.SetSelected(false);
        }

        selectedAircraft = ac;
        ShowZoneFixed(targetZoneImage, targetZoneRect, ac.TargetZoneNorm, targetZoneWidth, targetZoneColor);

        if (infoText != null)
        {
            infoText.text = ac.GetDescription();
        }

        SetTrajectoryLineVisible(ac, true);
        UpdateTrajectoryLine(ac);
    }

    private void UpdateTrajectoryLine()
    {
        if (selectedAircraft != null && !selectedAircraft.IsSelected)
        {
            selectedAircraft = null;
            HideAllZones();

            if (infoText != null)
                infoText.text = "Choose an aircraft";

        }

        for (int i = 0; i < activeAircrafts.Count; i++)
        {
            AircraftController aircraft = activeAircrafts[i];
            if (aircraft != null)
            {
                UpdateTrajectoryLine(aircraft);
            }
        }
    }

    private void UpdateTrajectoryLine(AircraftController aircraft)
    {
        if (aircraft == null) return;

        if (!aircraftTrajectoryLines.TryGetValue(aircraft, out AircraftTrajectoryVisual trajectoryVisual))
            return;

        if (trajectoryVisual?.Image == null || trajectoryVisual.Rect == null)
            return;

        if (!ShouldShowTrajectory(aircraft))
        {
            trajectoryVisual.Image.gameObject.SetActive(false);
            return;
        }

        Vector2 start = aircraft.CurrentPosition;
        Vector2 end = aircraft.EndPositionWorld;
        Vector2 lineDirection = end - start;
        float distance = lineDirection.magnitude;

        if (distance < 1f)
        {
            trajectoryVisual.Image.gameObject.SetActive(false);
            return;
        }

        float angle = Mathf.Atan2(lineDirection.y, lineDirection.x) * Mathf.Rad2Deg;

        trajectoryVisual.Rect.anchoredPosition = start;
        trajectoryVisual.Rect.sizeDelta = new Vector2(distance, trajectoryLineWidth);
        trajectoryVisual.Rect.localRotation = Quaternion.Euler(0, 0, angle);

        trajectoryVisual.Image.color = GetTrajectoryColor(aircraft);
        trajectoryVisual.Image.gameObject.SetActive(true);
    }

    private void CreateTrajectoryLineForAircraft(AircraftController aircraft)
    {
        if (trajectoryLinePrefab == null || aircraft == null || aircraftTrajectoryLines.ContainsKey(aircraft))
            return;

        GameObject lineObj = Instantiate(trajectoryLinePrefab, radarArea);
        Image lineImage = lineObj.GetComponent<Image>();
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        if (lineImage == null || lineRect == null)
        {
            Debug.LogError("Trajectory line prefab must contain Image and RectTransform components.");
            Destroy(lineObj);
            return;
        }

        lineImage.type = Image.Type.Tiled;
        lineImage.sprite = CreateDashedSprite();
        lineImage.pixelsPerUnitMultiplier = 100f;
        lineImage.color = trajectoryLineColor;
        lineImage.raycastTarget = false;

        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.zero;
        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.localPosition = Vector3.zero;
        lineRect.localScale = Vector3.one;

        lineObj.SetActive(false);

        aircraftTrajectoryLines[aircraft] = new AircraftTrajectoryVisual
        {
            Image = lineImage,
            Rect = lineRect
        };
    }

    private void SetTrajectoryLineVisible(AircraftController aircraft, bool isVisible)
    {
        if (aircraft == null) return;

        if (aircraftTrajectoryLines.TryGetValue(aircraft, out AircraftTrajectoryVisual trajectoryVisual) && trajectoryVisual?.Image != null)
        {
            trajectoryVisual.Image.gameObject.SetActive(isVisible);
        }
    }

    private bool ShouldShowTrajectory(AircraftController aircraft)
    {
        if (aircraft == null) return false;
        return collisionWarningAircrafts.Contains(aircraft) || (selectedAircraft == aircraft && aircraft.IsSelected);
    }

    private Color GetTrajectoryColor(AircraftController aircraft)
    {
        if (criticalCollisionAircrafts.Contains(aircraft))
            return criticalCollisionTrajectoryColor;

        if (collisionWarningAircrafts.Contains(aircraft))
            return collisionWarningTrajectoryColor;

        return trajectoryLineColor;
    }

    private bool WillAircraftsCollideSoon(AircraftController first, AircraftController second)
    {
        if (first == null || second == null || radarArea == null)
            return false;

        float maxPredictionTime = Mathf.Min(first.RemainingFlightTime, second.RemainingFlightTime);
        if (!predictCollisionForWholeRoute)
        {
            maxPredictionTime = Mathf.Min(maxPredictionTime, collisionPredictionTime);
        }

        if (maxPredictionTime <= 0f)
            return false;

        Vector2 relativePosition = second.CurrentPosition - first.CurrentPosition;
        Vector2 relativeVelocity = second.VelocityWorld - first.VelocityWorld;
        float velocitySqrMagnitude = relativeVelocity.sqrMagnitude;
        float collisionDistance = collisionRadius * radarArea.rect.width;

        if (velocitySqrMagnitude < 0.0001f)
            return relativePosition.magnitude <= collisionDistance;

        float timeToClosestApproach = -Vector2.Dot(relativePosition, relativeVelocity) / velocitySqrMagnitude;
        timeToClosestApproach = Mathf.Clamp(timeToClosestApproach, 0f, maxPredictionTime);

        Vector2 closestSeparation = relativePosition + relativeVelocity * timeToClosestApproach;
        return closestSeparation.magnitude <= collisionDistance;
    }

    private bool WillAircraftsCollideWithinTime(AircraftController first, AircraftController second, float predictionTime)
    {
        if (first == null || second == null || radarArea == null || predictionTime <= 0f)
            return false;

        float maxPredictionTime = Mathf.Min(predictionTime, first.RemainingFlightTime, second.RemainingFlightTime);
        if (maxPredictionTime <= 0f)
            return false;

        Vector2 relativePosition = second.CurrentPosition - first.CurrentPosition;
        Vector2 relativeVelocity = second.VelocityWorld - first.VelocityWorld;
        float velocitySqrMagnitude = relativeVelocity.sqrMagnitude;
        float collisionDistance = collisionRadius * radarArea.rect.width;

        if (velocitySqrMagnitude < 0.0001f)
            return relativePosition.magnitude <= collisionDistance;

        float timeToClosestApproach = -Vector2.Dot(relativePosition, relativeVelocity) / velocitySqrMagnitude;
        timeToClosestApproach = Mathf.Clamp(timeToClosestApproach, 0f, maxPredictionTime);

        Vector2 closestSeparation = relativePosition + relativeVelocity * timeToClosestApproach;
        return closestSeparation.magnitude <= collisionDistance;
    }

    private void RemoveTrajectoryLine(AircraftController aircraft)
    {
        if (aircraft == null) return;

        if (aircraftTrajectoryLines.TryGetValue(aircraft, out AircraftTrajectoryVisual trajectoryVisual))
        {
            if (trajectoryVisual?.Image != null)
            {
                Destroy(trajectoryVisual.Image.gameObject);
            }

            aircraftTrajectoryLines.Remove(aircraft);
        }
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

    private void HandleDestinationReached(AircraftController ac, bool hitTarget)
    {
        editTrajectoryLineImage.gameObject.SetActive(false);

        if (hitTarget)
        {
        }
        else
        {
        }
    }

    public bool IsAircraftExists(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("IsAircraftExists: РїРµСЂРµРґР°РЅ РїСѓСЃС‚РѕР№ ID");
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
            editTrajectoryLineImage.gameObject.SetActive(false);
            selectedAircraft = null;
            HideAllZones();

            if (infoText != null)
                infoText.text = "Select aircraft";
        }

        RemoveTrajectoryLine(ac);
        collisionWarningAircrafts.Remove(ac);
        criticalCollisionAircrafts.Remove(ac);
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
                ac.OnDestinationReached -= HandleDestinationReached;
            }
        }

        foreach (var trajectoryLine in aircraftTrajectoryLines.Values)
        {
            if (trajectoryLine?.Image != null)
            {
                Destroy(trajectoryLine.Image.gameObject);
            }
        }

        aircraftTrajectoryLines.Clear();
        collisionWarningAircrafts.Clear();
        criticalCollisionAircrafts.Clear();
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

    private void HandleTrajectoryEditing()
    {
        if (!isEditingMode && !isPendingTrajectoryVisible) return;
        if (selectedAircraft == null) return;

        Vector2 startPos = selectedAircraft.CurrentPosition;

        if (isEditingMode)
        {
            if (!isDraggingTrajectory)
            {
                isDraggingTrajectory = true;
                lastMouseX = Input.mousePosition.x;
            }
            else
            {
                float currentMouseX = Input.mousePosition.x;
                float deltaX = currentMouseX - lastMouseX;

                if (Mathf.Abs(deltaX) > 0.1f)
                {
                    float angleChange = -deltaX * 0.3f;
                    currentEditAngle += angleChange;
                    lastMouseX = currentMouseX;
                }
            }

            float angleRad = currentEditAngle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            Vector2 edgePoint = GetEdgePoint(startPos, direction);
            UpdateEditLine(startPos, edgePoint);

            if (!Input.GetMouseButton(0))
            {
                isEditingMode = false;
                isPendingTrajectoryVisible = true;
                isDraggingTrajectory = false;

                Vector2 size = radarArea.rect.size;
                pendingTrajectory = new Vector2(edgePoint.x / size.x, edgePoint.y / size.y);
                pendingAircraftID = selectedAircraft.AircraftID;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                isEditingMode = false;
                isPendingTrajectoryVisible = false;
                isDraggingTrajectory = false;
                editTrajectoryLineImage.gameObject.SetActive(false);
                CancelEditing();
            }
        }
        else if (isPendingTrajectoryVisible && pendingTrajectory.HasValue)
        {
            isDraggingTrajectory = false;

            Vector2 size = radarArea.rect.size;
            Vector2 targetPos = new Vector2(pendingTrajectory.Value.x * size.x, pendingTrajectory.Value.y * size.y);
            UpdateEditLine(startPos, targetPos);
            editTrajectoryLineImage.gameObject.SetActive(true);
        }
    }

    private Vector2 GetEdgePoint(Vector2 origin, Vector2 direction)
    {
        Vector2 size = radarArea.rect.size;

        if (direction.magnitude < 0.1f)
        {
            return new Vector2(size.x, origin.y);
        }

        direction.Normalize();

        float tMin = float.MaxValue;

        if (direction.x > 0)
        {
            float t = (size.x - origin.x) / direction.x;
            if (t > 0 && t < tMin) tMin = t;
        }

        if (direction.x < 0)
        {
            float t = -origin.x / direction.x;
            if (t > 0 && t < tMin) tMin = t;
        }

        if (direction.y > 0)
        {
            float t = (size.y - origin.y) / direction.y;
            if (t > 0 && t < tMin) tMin = t;
        }

        if (direction.y < 0)
        {
            float t = -origin.y / direction.y;
            if (t > 0 && t < tMin) tMin = t;
        }

        Vector2 result = origin + direction * tMin;
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

    private void CancelEditing()
    {
        isEditingMode = false;
        isPendingTrajectoryVisible = false;
        isDraggingTrajectory = false;
        editTrajectoryLineImage.gameObject.SetActive(false);
        pendingTrajectory = null;
        pendingAircraftID = null;
    }

    public void StartEditMode()
    {
        if (selectedAircraft == null) return;

        isEditingMode = true;
        pendingTrajectory = null;
        isPendingTrajectoryVisible = false;
        isDraggingTrajectory = false;

        Vector2 currentDir = selectedAircraft.GetDirection();
        currentEditAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;

        editTrajectoryLineImage.gameObject.SetActive(true);

        Vector2 start = selectedAircraft.CurrentPosition;
        Vector2 edgePoint = GetEdgePoint(start, currentDir);
        UpdateEditLine(start, edgePoint);

    }

    public void ApplyPendingTrajectory(string aircraftID)
    {
        if (pendingTrajectory.HasValue && pendingAircraftID == aircraftID)
        {
            AircraftController ac = GetAircraftByID(aircraftID);
            if (ac != null)
            {
                ac.SetNewDestination(pendingTrajectory.Value);
                UpdateTrajectoryLine(ac);
                isPendingTrajectoryVisible = false;
                editTrajectoryLineImage.gameObject.SetActive(false);
                pendingTrajectory = null;
                pendingAircraftID = null;
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
        bool horizontal;
        bool invertGradient;
        float rotation = 0f;

        if (Mathf.Approximately(normPos.x, 0f)) // Левая граница
        {
            center = new Vector2(0f, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.3f);
            zoneRect.pivot = new Vector2(0f, 0.5f);
            horizontal = true;
            invertGradient = false;
        }
        else if (Mathf.Approximately(normPos.x, 1f)) // Правая граница
        {
            center = new Vector2(size.x, normPos.y * size.y);
            zoneSize = new Vector2(width * size.x, size.y * 0.3f);
            zoneRect.pivot = new Vector2(1f, 0.5f);
            horizontal = true;
            rotation = 180f; // Разворачиваем градиент
        }
        else if (Mathf.Approximately(normPos.y, 0f)) // Нижняя граница
        {
            center = new Vector2(normPos.x * size.x, 0);
            zoneSize = new Vector2(size.x * 0.3f, width * size.y);
            zoneRect.pivot = new Vector2(0.5f, 0);
            horizontal = false;
            rotation = 0f;
        }
        else // Верхняя граница
        {
            center = new Vector2(normPos.x * size.x, size.y);
            zoneSize = new Vector2(size.x * 0.3f, width * size.y);
            zoneRect.pivot = new Vector2(0.5f, 1);
            horizontal = false;
            rotation = 180f; // Разворачиваем градиент
        }

        zoneRect.anchoredPosition = center;
        zoneRect.sizeDelta = zoneSize;

        // Обновляем спрайт с правильным направлением градиента
        int textureSize = 128;
        Sprite gradientSprite = CreateGradientSprite(textureSize, 8, horizontal);
        zoneImage.sprite = gradientSprite;
        zoneImage.color = color;

        // Поворачиваем для нужного направления
        zoneRect.localRotation = Quaternion.Euler(0, 0, rotation);

        zoneImage.gameObject.SetActive(true);
    }

    private string GetEdgeName(Vector2 norm)
    {
        if (Mathf.Approximately(norm.x, 0f)) return "Р›Р•Р’Рћ";
        if (Mathf.Approximately(norm.x, 1f)) return "РџР РђР’Рћ";
        if (Mathf.Approximately(norm.y, 0f)) return "РќРР—";
        if (Mathf.Approximately(norm.y, 1f)) return "Р’Р•Р РҐ";
        return "РќР•РР—Р’Р•РЎРўРќРћ";
    }

    private void ShowZoneFixed(Image zoneImage, RectTransform zoneRect, Vector2 normPos, float width, Color color)
    {
        if (zoneImage == null || zoneRect == null || radarArea == null) return;

        Vector2 size = radarArea.rect.size;
        Vector2 center;
        Vector2 zoneSize;
        bool horizontal;
        bool invertGradient;

        switch (GetEdgeSide(normPos))
        {
            case RadarEdgeSide.Left:
                center = new Vector2(0f, normPos.y * size.y);
                zoneSize = new Vector2(width * size.x, size.y * 0.3f);
                zoneRect.pivot = new Vector2(0f, 0.5f);
                horizontal = true;
                invertGradient = false;
                break;
            case RadarEdgeSide.Right:
                center = new Vector2(size.x, normPos.y * size.y);
                zoneSize = new Vector2(width * size.x, size.y * 0.3f);
                zoneRect.pivot = new Vector2(1f, 0.5f);
                horizontal = true;
                invertGradient = true;
                break;
            case RadarEdgeSide.Bottom:
                center = new Vector2(normPos.x * size.x, 0f);
                zoneSize = new Vector2(size.x * 0.3f, width * size.y);
                zoneRect.pivot = new Vector2(0.5f, 0f);
                horizontal = false;
                invertGradient = false;
                break;
            default:
                center = new Vector2(normPos.x * size.x, size.y);
                zoneSize = new Vector2(size.x * 0.3f, width * size.y);
                zoneRect.pivot = new Vector2(0.5f, 1f);
                horizontal = false;
                invertGradient = true;
                break;
        }

        zoneRect.anchoredPosition = center;
        zoneRect.sizeDelta = zoneSize;
        zoneRect.localRotation = Quaternion.identity;

        int textureSize = 128;
        zoneImage.sprite = CreateGradientSpriteFixed(textureSize, 8, horizontal, invertGradient);
        zoneImage.color = color;
        zoneImage.gameObject.SetActive(true);
    }

    private Sprite CreateGradientSpriteFixed(int width, int height, bool horizontal, bool invert)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float t = horizontal ? (float)x / width : (float)y / height;
                if (invert)
                {
                    t = 1f - t;
                }

                float alpha = Mathf.Pow(1f - t, 2f);
                colors[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0f, 0.5f));
    }

    private void CreateZones()
    {
        // Целевая зона (зелёная) - куда НАДО
        GameObject targetObj = new GameObject("TargetZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        targetObj.transform.SetParent(radarArea, false);
        targetZoneImage = targetObj.GetComponent<Image>();
        targetZoneRect = targetObj.GetComponent<RectTransform>();
        SetupZone(targetZoneImage, targetZoneRect, targetZoneColor, targetZoneWidth, true);

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

    private RadarEdgeSide GetEdgeSide(Vector2 normPoint)
    {
        float leftDistance = Mathf.Abs(normPoint.x);
        float rightDistance = Mathf.Abs(1f - normPoint.x);
        float bottomDistance = Mathf.Abs(normPoint.y);
        float topDistance = Mathf.Abs(1f - normPoint.y);

        float minDistance = leftDistance;
        RadarEdgeSide closestSide = RadarEdgeSide.Left;

        if (rightDistance < minDistance)
        {
            minDistance = rightDistance;
            closestSide = RadarEdgeSide.Right;
        }

        if (bottomDistance < minDistance)
        {
            minDistance = bottomDistance;
            closestSide = RadarEdgeSide.Bottom;
        }

        if (topDistance < minDistance)
        {
            minDistance = topDistance;
            closestSide = RadarEdgeSide.Top;
        }

        return minDistance <= edgeDetectionEpsilon ? closestSide : closestSide;
    }

    private Sprite CreateGradientSprite(int width, int height, bool horizontal, bool invert = false)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float t;
                if (horizontal)
                {
                    // Горизонтальный градиент (от левого края к центру)
                    t = (float)x / width;
                }
                else
                {
                    // Вертикальный градиент (от верхнего/нижнего края к центру)
                    t = (float)y / height;
                }

                // Градиент: яркий у края, прозрачный внутри
                float alpha = 1f - t; // Затухание
                alpha = Mathf.Pow(alpha, 2f); // Квадратичное затухание для более резкого перехода

                colors[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0, 0.5f));
    }

    private void SetupZone(Image img, RectTransform rect, Color color, float width, bool isTargetZone)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        // Размер текстуры для градиента
        int textureSize = 128;

        // Создаём градиентный спрайт (горизонтальный по умолчанию)
        Sprite gradientSprite = CreateGradientSprite(textureSize, 8, true);
        img.sprite = gradientSprite;
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        img.gameObject.SetActive(false);
    }

    public bool HasCollisionWarning()
    {
        return collisionWarningAircrafts.Count > 0;
    }

    public bool HasCriticalCollision()
    {
        return criticalCollisionAircrafts.Count > 0;
    }
}
