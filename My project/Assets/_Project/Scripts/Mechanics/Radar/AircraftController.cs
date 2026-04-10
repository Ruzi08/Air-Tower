using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AircraftController : MonoBehaviour, IPointerClickHandler, Interactable
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

    private RectTransform parentRect; // Размер области радара
    private float progress = 0f;
    private bool isSelected = false;

    public float Speed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

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
    }

    public void Initialize(RectTransform radarArea, Vector2 start, Vector2 end)
    {
        parentRect = radarArea;
        startPosNorm = start;
        endPosNorm = end;
        progress = 0f;
        Debug.Log($"Initialize: parentRect size = {parentRect.rect.size}");
        Debug.Log($"Canvas scale: {parentRect.lossyScale}");

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

        if (progress < 0.1f) // Выведем только первые 10% пути
        {
            Debug.Log($"Позиция самолета: {newPos}, Размер родителя: {parentRect.rect.size}");
        }
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

        Debug.Log($"NormToWorld: norm={norm}, size={size}, result=({x}, {y})");

        return new Vector2(x, y);
    }

    public void Interact()
    {
        ToggleSelection();
    }

    public string GetDescription()
    {
        return $"Самолет\nТраектория: {startPosNorm} -> {endPosNorm}\nПрогресс: {(progress * 100f):F1}%";
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
}
