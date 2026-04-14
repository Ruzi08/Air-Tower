using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class AircraftController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, Interactable
{
    [Header("Movement")]
    [SerializeField] private Vector2 startPosNorm; // 0..1
    [SerializeField] private Vector2 endPosNorm;   // 0..1
    [SerializeField] private float moveSpeed = 0.3f;   // Скорость перемещения

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

    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    [SerializeField] private float holdTimeToEdit = 0.3f;

    private static int lastGeneratedNumber = 0;
    private static System.Random random = new System.Random();


    private RectTransform parentRect; // Размер области радара
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
        {
            GenerateNewID();
        }
    }

    public void Initialize(RectTransform radarArea, Vector2 start, Vector2 end,Vector2 target)
    {
        parentRect = radarArea;
        startPosNorm = start;
        endPosNorm = end;
        targetZoneNorm = target;
        progress = 0f;

        // Сбрасываем anchor в Top-Left для правильного позиционирования
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.sizeDelta = new Vector2(20, 20);

        if (aircraftImage != null)
        {
            aircraftImage.color = Color.red; // Яркий цвет для отладки
        }

        UpdatePosition();
        SetSelected(false);
    }

    void Update()
    {
        if (parentRect == null) return;

        progress += moveSpeed * Time.deltaTime;

        if (progress >= 1f)
        {
            bool hitTarget = CheckIfHitTarget();
            OnDestinationReached?.Invoke(this, hitTarget);
            // Самолет достиг точки назначения
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
        if (parentRect == null)
        {
            Debug.LogError("parentRect is null!");
            return Vector2.zero;
        }

        // Получаем размер родительского RectTransform
        Vector2 size = parentRect.rect.size;

        // Конвертируем нормализованные координаты (0..1) в локальные координаты
        // 0,0 - левый нижний угол, 1,1 - правый верхний
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
        return $"ID: {aircraftID}\nТраектория: ({startPosNorm.x:F2}, {startPosNorm.y:F2}) → ({endPosNorm.x:F2}, {endPosNorm.y:F2})\nПрогресс: {(progress * 100f):F1}%";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Вызываем наш метод взаимодействия
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
        return distance < checkRadius * parentRect.rect.width; // Переводим радиус в пиксели
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

    // Для отладки траектории в редакторе
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
        // Только если самолет уже выделен
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


    private bool CheckIfHitTarget()
    {
        // Порог сравнения (допустимая погрешность)
        float threshold = 0.05f;

        // Проверяем, что конечная точка близка к целевой зоне
        return Vector2.Distance(endPosNorm, targetZoneNorm) < threshold;
    }

}
