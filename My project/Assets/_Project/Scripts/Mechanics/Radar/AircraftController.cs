using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Interactable
{
    [Header("Movement")]
    [SerializeField] private Vector2 startPosNorm; // 0..1
    [SerializeField] private Vector2 endPosNorm;   // 0..1
    [SerializeField] private float moveSpeed = 120f;   // UI units per second

    [Header("Components")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image aircraftImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    [Header("Identification")]
    [SerializeField] private string aircraftID;
    [SerializeField] private bool generateIDOnAwake = true;

    [Header("Target Zone")]
    [SerializeField] private Vector2 targetZoneNorm;

    [Header("Audio")]
    public AudioClip aircraftSelectSound;
    private AudioSource audioSource;

    [Header("ID Label")]
    [SerializeField] private GameObject idLabelPrefab;
    [SerializeField] private Color idLabelColor = Color.white;
    [SerializeField] private Vector2 idLabelOffset = new Vector2(0, -15f);
    [SerializeField] private float idLabelFontSize = 1f;

    private TextMeshProUGUI idLabelText;
    private RectTransform idLabelRect;

    public Vector2 TargetZoneNorm => targetZoneNorm;
    public Vector2 TargetZoneWorld => NormToWorld(targetZoneNorm);

    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    [SerializeField] private float holdTimeToEdit = 0.3f;
    private bool suppressClickAfterHold = false;

    private static int lastGeneratedNumber = 0;
    private static System.Random random = new System.Random();

    private RectTransform parentRect;
    private float progress = 0f;
    private bool isSelected = false;

    public System.Action<AircraftController, bool> OnDestinationReached;

    public bool IsPointerDown => isPointerDown;
    public bool IsSelected => isSelected;
    public float RemainingFlightTime
    {
        get
        {
            float speed = Mathf.Max(0f, moveSpeed);
            if (speed <= 0f)
                return float.PositiveInfinity;

            return GetRemainingDistanceWorld() / speed;
        }
    }

    public float Speed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    public string AircraftID => aircraftID;
    public Vector2 EndPosNorm => endPosNorm;

    public System.Action<string> OnIDGenerated;
    public System.Action<AircraftController> OnSelected;
    public System.Action<AircraftController> OnReachedDestination;
    public System.Action<AircraftController> OnDestroyed;

    public Vector2 StartPositionWorld => NormToWorld(startPosNorm);
    public Vector2 EndPositionWorld => NormToWorld(endPosNorm);
    public Vector2 CurrentPosition => rectTransform.anchoredPosition;
    public Vector2 VelocityWorld
    {
        get
        {
            float speed = Mathf.Max(0f, moveSpeed);
            if (speed <= 0f)
                return Vector2.zero;

            Vector2 direction = EndPositionWorld - CurrentPosition;
            if (direction.sqrMagnitude <= 0.0001f)
                return Vector2.zero;

            return direction.normalized * speed;
        }
    }

    private void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (aircraftImage == null) aircraftImage = GetComponent<Image>();

        if (generateIDOnAwake && string.IsNullOrEmpty(aircraftID))
        {
            GenerateNewID();
        }
    }

    public void Initialize(RectTransform radarArea, Vector2 start, Vector2 end, Vector2 target)
    {
        parentRect = radarArea;
        startPosNorm = start;
        endPosNorm = end;
        targetZoneNorm = target;
        progress = 0f;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(60, 60);

        if (aircraftImage != null)
        {
            aircraftImage.color = Color.red;
        }

        CreateIDLabel();
        UpdatePosition();
        SetSelected(false);
    }

    private void Update()
    {
        if (parentRect == null) return;

        float pathLength = GetPathLengthWorld();
        if (pathLength <= 0.0001f)
        {
            progress = 1f;
        }
        else
        {
            progress += (Mathf.Max(0f, moveSpeed) * Time.deltaTime) / pathLength;
        }

        if (progress >= 1f)
        {
            bool hitTarget = CheckIfHitTarget();
            OnDestinationReached?.Invoke(this, hitTarget);
            OnReachedDestination?.Invoke(this);
            Destroy(gameObject);
        }
        else
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector2 currentNorm = Vector2.Lerp(startPosNorm, endPosNorm, progress);
        Vector2 newPos = NormToWorld(currentNorm);
        rectTransform.anchoredPosition = newPos;
        UpdateIDLabelPosition();
    }

    private Vector2 NormToWorld(Vector2 norm)
    {
        if (parentRect == null)
        {
            Debug.LogError("parentRect is null!");
            return Vector2.zero;
        }

        Vector2 size = parentRect.rect.size;
        float x = norm.x * size.x;
        float y = norm.y * size.y;
        return new Vector2(x, y);
    }

    public void Interact()
    {
        ToggleSelection();
    }

    public string GetDescription()
    {
        return $"ID: {aircraftID}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (suppressClickAfterHold)
        {
            suppressClickAfterHold = false;
            return;
        }

        Interact();
    }

    private void ToggleSelection()
    {
        if (aircraftSelectSound != null && audioSource != null)
            audioSource.PlayOneShot(aircraftSelectSound);

        SetSelected(!isSelected);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        aircraftImage.color = isSelected ? selectedColor : normalColor;

        if (isSelected)
        {
            OnSelected?.Invoke(this);
        }
    }

    public Vector2 GetDirection()
    {
        return (endPosNorm - startPosNorm).normalized;
    }

    public bool WillCollideWith(AircraftController other, float checkRadius = 0.05f)
    {
        if (other == null) return false;

        float distance = Vector2.Distance(CurrentPosition, other.CurrentPosition);
        return distance < checkRadius * parentRect.rect.width;
    }

    public void GenerateNewID()
    {
        aircraftID = GenerateUniqueAircraftID();
        gameObject.name = $"Aircraft_{aircraftID}";
        OnIDGenerated?.Invoke(aircraftID);
    }

    public void SetID(string newID)
    {
        aircraftID = newID;
        gameObject.name = $"Aircraft_{aircraftID}";
        OnIDGenerated?.Invoke(aircraftID);
    }

    private void CreateIDLabel()
    {
        if (idLabelPrefab != null)
        {
            GameObject labelObj = Instantiate(idLabelPrefab, transform);
            idLabelText = labelObj.GetComponent<TextMeshProUGUI>();
            idLabelRect = labelObj.GetComponent<RectTransform>();
        }
        else
        {
            GameObject labelObj = new GameObject("IDLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(transform, false);

            idLabelText = labelObj.GetComponent<TextMeshProUGUI>();
            idLabelRect = labelObj.GetComponent<RectTransform>();

            idLabelText.fontSize = idLabelFontSize;
            idLabelText.alignment = TextAlignmentOptions.Center;
            idLabelText.raycastTarget = false;
            idLabelText.fontStyle = FontStyles.Bold;

            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont != null)
                idLabelText.font = defaultFont;
        }

        idLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        idLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        idLabelRect.pivot = new Vector2(0.5f, 0f);
        idLabelRect.sizeDelta = new Vector2(60, 20);

        idLabelText.text = aircraftID;
        idLabelText.color = idLabelColor;
        idLabelText.outlineWidth = 0.2f;
        idLabelText.outlineColor = Color.black;
    }

    private void UpdateIDLabelPosition()
    {
        if (idLabelRect != null)
        {
            idLabelRect.anchoredPosition = idLabelOffset;
        }
    }

    private static string GenerateUniqueAircraftID()
    {
        char letter1 = (char)random.Next('A', 'K' + 1);
        char letter2 = (char)random.Next('A', 'K' + 1);

        int randomNumber = random.Next(0, 50);
        string number = randomNumber.ToString("D2");

        return $"{letter1}{letter2}{number}";
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    private void OnDrawGizmosSelected()
    {
        if (parentRect == null) return;

        Gizmos.color = Color.yellow;
        Vector3 startWorld = transform.parent.TransformPoint(NormToWorld(startPosNorm));
        Vector3 endWorld = transform.parent.TransformPoint(NormToWorld(endPosNorm));
        Gizmos.DrawLine(startWorld, endWorld);
        Gizmos.DrawSphere(startWorld, 5f);
        Gizmos.DrawSphere(endWorld, 5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isSelected) return;

        isPointerDown = true;
        pointerDownTime = Time.time;
        CancelInvoke(nameof(StartEditMode));
        Invoke(nameof(StartEditMode), holdTimeToEdit);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        CancelInvoke(nameof(StartEditMode));
    }

    private void StartEditMode()
    {
        if (isPointerDown && isSelected)
        {
            suppressClickAfterHold = true;
            RadarManager radar = FindFirstObjectByType<RadarManager>();
            if (radar != null)
            {
                radar.StartEditMode();
            }
        }
    }

    public void SetNewDestination(Vector2 newEndNorm)
    {
        Vector2 currentNorm = Vector2.Lerp(startPosNorm, endPosNorm, progress);
        startPosNorm = currentNorm;
        endPosNorm = newEndNorm;
        progress = 0f;
    }

    private float GetPathLengthWorld()
    {
        return Vector2.Distance(StartPositionWorld, EndPositionWorld);
    }

    private float GetRemainingDistanceWorld()
    {
        return Vector2.Distance(CurrentPosition, EndPositionWorld);
    }

    private bool CheckIfHitTarget()
    {
        float threshold = 0.05f;
        return Vector2.Distance(endPosNorm, targetZoneNorm) < threshold;
    }
}
