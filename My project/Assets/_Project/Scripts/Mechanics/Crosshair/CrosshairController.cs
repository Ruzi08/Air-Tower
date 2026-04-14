using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Обычное состояние")]
    public Vector2 normalSize = new Vector2(5, 5);
    public Color normalColor = Color.white;
    public float normalAlpha = 0.98f;
    
    [Header("Наведение на Interactable")]
    public Vector2 highlightSize = new Vector2(6, 6);
    public Color highlightColor = Color.red;
    public float highlightAlpha = 1f;
    
    [Header("Плавность")]
    public float lerpSpeed = 15f;
    
    private Image crosshairImage;
    private RectTransform rectTransform;
    
    private Vector2 currentSize;
    private Color currentColor;
    private float currentAlpha;
    
    void Start()
    {
        crosshairImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (crosshairImage == null)
        {
            Debug.LogError("CrosshairController нужно повесить на Image!");
            return;
        }
        
        // Устанавливаем начальные значения
        SetCrosshairNormal();
    }
    
    void Update()
    {
        // Плавное изменение размера
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, currentSize, Time.deltaTime * lerpSpeed);
        
        // Плавное изменение цвета и прозрачности
        crosshairImage.color = Color.Lerp(crosshairImage.color, currentColor, Time.deltaTime * lerpSpeed);
    }
    
    public void SetCrosshairNormal()
    {
        currentSize = normalSize;
        currentColor = new Color(normalColor.r, normalColor.g, normalColor.b, normalAlpha);
    }
    
    public void SetCrosshairHighlight()
    {
        currentSize = highlightSize;
        currentColor = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightAlpha);
    }
}