using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TrajectoryHandle : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RadarManager radarManager;
    private RectTransform radarArea;
    private RectTransform handleRect;
    private Image handleImage;

    private AircraftController currentAircraft;
    private bool isDragging = false;
    private float startMouseX;
    private float originalAngle;
    private float currentDeltaAngle;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.5f;      // градусов на пиксель
    [SerializeField] private float maxAngle = 180f;
    [SerializeField] private float minAngle = -180f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color draggingColor = Color.yellow;

    // Событие, которое будет вызвано при отпускании ручки (передаёт самолёт и выбранный угол)
    public System.Action<AircraftController, float> OnAngleSelected;

    public void Initialize(RadarManager manager, RectTransform radar)
    {
        radarManager = manager;
        radarArea = radar;
        handleRect = GetComponent<RectTransform>();
        handleImage = GetComponent<Image>();

        if (handleImage != null)
            handleImage.color = normalColor;
    }

    public void ShowForAircraft(AircraftController aircraft)
    {
        currentAircraft = aircraft;
        UpdateHandlePosition();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        currentAircraft = null;
        isDragging = false;

        if (handleImage != null)
            handleImage.color = normalColor;
    }

    public void UpdateHandlePosition()
    {
        if (currentAircraft == null || handleRect == null) return;

        Vector2 endPoint = currentAircraft.EndPositionWorld;
        handleRect.anchoredPosition = endPoint;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentAircraft == null || radarManager == null) return;

        isDragging = true;
        startMouseX = Input.mousePosition.x;

        Vector2 startPoint = currentAircraft.CurrentPosition;
        Vector2 endPoint = currentAircraft.EndPositionWorld;
        Vector2 originalDir = endPoint - startPoint;
        originalAngle = Mathf.Atan2(originalDir.y, originalDir.x) * Mathf.Rad2Deg;
        currentDeltaAngle = 0f;

        if (handleImage != null)
            handleImage.color = draggingColor;

        radarManager.StartTrajectoryEditing(currentAircraft, originalAngle);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || currentAircraft == null || radarManager == null) return;

        float currentMouseX = Input.mousePosition.x;
        float deltaX = currentMouseX - startMouseX;

        currentDeltaAngle = deltaX * sensitivity;
        currentDeltaAngle = Mathf.Clamp(currentDeltaAngle, minAngle, maxAngle);

        float newAngle = originalAngle + currentDeltaAngle;
        float angleRad = newAngle * Mathf.Deg2Rad;
        Vector2 newDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        Vector2 startPoint = currentAircraft.CurrentPosition;
        Vector2 newEndPoint = GetEdgePoint(startPoint, newDirection);

        radarManager.UpdateTrajectoryPreview(startPoint, newEndPoint, currentDeltaAngle);

        handleRect.anchoredPosition = newEndPoint;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        if (handleImage != null)
            handleImage.color = normalColor;

        // Вызываем событие для радио (или другого кода)
        OnAngleSelected?.Invoke(currentAircraft, currentDeltaAngle);

        // Показываем сообщение об угле через RadarManager
        radarManager.ShowAngleMessage(currentAircraft.AircraftID, currentDeltaAngle);

        // Скрываем временную линию и возвращаем ручку на исходное место
        radarManager.CancelTrajectoryEdit();
        UpdateHandlePosition();
    }

    private Vector2 GetEdgePoint(Vector2 origin, Vector2 direction)
    {
        Vector2 size = radarArea.rect.size;

        if (direction.magnitude < 0.1f)
            return new Vector2(size.x, origin.y);

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
}