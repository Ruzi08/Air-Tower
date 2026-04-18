using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftController : MonoBehaviour, IPointerClickHandler, Interactable
{
    [Header("Movement")]
    [SerializeField] private Vector2 startPosNorm; // 0..1
    [SerializeField] private Vector2 endPosNorm;   // 0..1
    [SerializeField] private float moveSpeed = 0.3f;

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

    public Vector2 TargetZoneNorm => targetZoneNorm;
    public Vector2 TargetZoneWorld => NormToWorld(targetZoneNorm);

    private static int lastGeneratedNumber = 0;
    private static System.Random random = new System.Random();

    private RectTransform parentRect;
    private float progress = 0f;
    private bool isSelected = false;

    public System.Action<AircraftController, bool> OnDestinationReached;

    public float Speed
    {
        get => moveSpeed;
        set => moveSpeed = value;
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

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (aircraftImage == null) aircraftImage = GetComponent<Image>();
        if (generateIDOnAwake && string.IsNullOrEmpty(aircraftID))
            GenerateNewID();
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
        rectTransform.sizeDelta = new Vector2(20, 20);

        if (aircraftImage != null)
            aircraftImage.color = normalColor;

        UpdatePosition();
        SetSelected(false);
    }

    private void Update()
    {
        if (parentRect == null) return;

        progress += moveSpeed * Time.deltaTime;

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
    }

    private Vector2 NormToWorld(Vector2 norm)
    {
        if (parentRect == null) return Vector2.zero;
        Vector2 size = parentRect.rect.size;
        return new Vector2(norm.x * size.x, norm.y * size.y);
    }

    public void Interact()
    {
        ToggleSelection();
    }

    public string GetDescription()
    {
        return $"ID: {aircraftID}\nТраектория: ({startPosNorm.x:F2}, {startPosNorm.y:F2}) → ({endPosNorm.x:F2}, {endPosNorm.y:F2})\nПрогресс: {(progress * 100f):F1}%";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Interact();
    }

    private void ToggleSelection()
    {
        SetSelected(!isSelected);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        aircraftImage.color = isSelected ? selectedColor : normalColor;

        if (isSelected)
            OnSelected?.Invoke(this);
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

    private static string GenerateUniqueAircraftID()
    {
        char letter1 = (char)random.Next('A', 'Z' + 1);
        char letter2 = (char)random.Next('A', 'Z' + 1);
        lastGeneratedNumber = (lastGeneratedNumber + 1) % 100;
        string number = lastGeneratedNumber.ToString("D2");
        return $"{letter1}{letter2}{number}";
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this);
    }

    public void SetNewDestination(Vector2 newEndNorm)
    {
        Vector2 currentNorm = Vector2.Lerp(startPosNorm, endPosNorm, progress);
        startPosNorm = currentNorm;
        endPosNorm = newEndNorm;
        progress = 0f;
    }

    private bool CheckIfHitTarget()
    {
        float threshold = 0.05f;
        return Vector2.Distance(endPosNorm, targetZoneNorm) < threshold;
    }
}