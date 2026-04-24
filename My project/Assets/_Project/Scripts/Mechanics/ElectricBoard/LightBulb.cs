using UnityEngine;

public class LightBulb : MonoBehaviour
{
    [Header("Настройки")]
    public int materialIndex = 0;         // Какой материал менять (0 = первый)
    public Material onMaterial;           // Материал когда свет есть
    public Material offMaterial;          // Материал когда света нет
    
    [Header("Опционально")]
    public Light lightSource;             // Реальный источник света
    
    private Renderer bulbRenderer;
    private Material[] originalMaterials;
    private bool hasPower = true;
    
    void Start()
    {
        bulbRenderer = GetComponent<Renderer>();
        
        if (bulbRenderer == null)
            bulbRenderer = GetComponentInChildren<Renderer>();
        
        if (bulbRenderer != null)
        {
            // Сохраняем оригинальные материалы
            originalMaterials = bulbRenderer.materials;
            
            // Применяем начальное состояние
            UpdateVisual();
        }
        
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += HandlePowerOut;
            PowerManager.Instance.OnPowerRestored += HandlePowerRestored;
        }
    }
    
    void OnDestroy()
    {
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= HandlePowerOut;
            PowerManager.Instance.OnPowerRestored -= HandlePowerRestored;
        }
    }
    
    private void HandlePowerOut()
    {
        hasPower = false;
        UpdateVisual();
    }
    
    private void HandlePowerRestored()
    {
        hasPower = true;
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        if (bulbRenderer == null) return;
        if (originalMaterials == null) return;
        
        // Создаём копию материалов
        Material[] newMaterials = (Material[])originalMaterials.Clone();
        
        // Меняем только нужный индекс
        if (materialIndex >= 0 && materialIndex < newMaterials.Length)
        {
            newMaterials[materialIndex] = hasPower ? onMaterial : offMaterial;
        }
        
        // Применяем
        bulbRenderer.materials = newMaterials;
        
        // Обновляем реальный свет
        if (lightSource != null)
        {
            lightSource.enabled = hasPower;
        }
    }
}