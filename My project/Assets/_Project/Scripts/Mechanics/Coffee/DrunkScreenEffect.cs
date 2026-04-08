using UnityEngine;
using UnityEngine.UI;

public class DrunkScreenEffect : MonoBehaviour
{
    [Header("Параметры эффекта")]
    public float waveSpeed = 2f;
    public float waveAmount = 8f;
    public float colorShiftSpeed = 1f;
    [Range(0, 1)] public float intensity = 0.5f;
    
    private Image effectImage;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private float time;
    private bool isActive = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        effectImage = GetComponent<Image>();
        
        if (effectImage == null)
            effectImage = gameObject.AddComponent<Image>();
        
        effectImage.raycastTarget = false;
        effectImage.color = new Color(1, 1, 1, 0);
        originalPosition = rectTransform.localPosition;
    }
    
    void Update()
    {
        if (!isActive) return;
        
        time += Time.deltaTime * waveSpeed;
        
        float offsetX = Mathf.Sin(time) * waveAmount * intensity;
        float offsetY = Mathf.Cos(time * 1.3f) * waveAmount * intensity;
        rectTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
        
        float alpha = 0.2f * intensity;
        float colorR = 1 + Mathf.Sin(time * colorShiftSpeed) * 0.1f * intensity;
        float colorG = 1 - Mathf.Sin(time * colorShiftSpeed * 1.3f) * 0.1f * intensity;
        effectImage.color = new Color(colorR, colorG, 1, alpha);
        
        float rotation = Mathf.Sin(time * 1.5f) * 2f * intensity;
        rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
    }
    
    public void EnableEffect(bool enable)
    {
        isActive = enable;
        if (!enable)
        {
            rectTransform.localPosition = originalPosition;
            rectTransform.localRotation = Quaternion.identity;
            effectImage.color = new Color(1, 1, 1, 0);
            time = 0;
        }
    }
    
    public void SetIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp01(newIntensity);
    }
}