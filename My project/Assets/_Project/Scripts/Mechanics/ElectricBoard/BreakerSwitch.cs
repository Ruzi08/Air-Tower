using UnityEngine;

public class BreakerSwitch : MonoBehaviour, Interactable
{
    [Header("Состояние")]
    public bool isBroken = false;     // Выбит?
    public bool isFixed = false;      // Включён?

    [Header("Лампочка (свой выключатель)")]
    public Renderer bulbRenderer;           // Рендерер лампочки
    public int bulbMaterialIndex = 1;       // Индекс материала лампы (обычно 1)
    public Material bulbOnMaterial;         // Лампочка горит (рычажок включён)
    public Material bulbOffMaterial;        // Лампочка не горит (рычажок выбит)

    [Header("Ссылки")]
    public BreakerPanel panel;
    
    [Header("Анимация поворота")]
    public Vector3 brokenRotation = new Vector3(0, -22.205f, 0); // Положение когда выбит
    public float animationSpeed = 45f; // Скорость анимации

    private Quaternion originalRotation;
    private Quaternion brokenRotationQuat;
    private Quaternion targetRotation;
    private bool isAnimating = false;

    private Material[] originalBulbMaterials;

    void Start()
    {
        // Запоминаем повороты
        originalRotation = transform.localRotation;
        brokenRotationQuat = originalRotation * Quaternion.Euler(brokenRotation);
        
        // Устанавливаем правильный поворот в зависимости от состояния
        if (isBroken)
        {
            targetRotation = brokenRotationQuat;
            transform.localRotation = brokenRotationQuat;
        }
        else
        {
            targetRotation = originalRotation;
            transform.localRotation = originalRotation;
        }
        
        if (bulbRenderer != null)
        {
            originalBulbMaterials = bulbRenderer.materials;
        }

        UpdateBulbVisual();
    }
    
    void Update()
    {
        if (isAnimating)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, 
                targetRotation, 
                animationSpeed * Time.deltaTime
            );
            
            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
            }
        }
    }

    public void SetBroken()
    {
        if (isFixed) return;
        isBroken = true;
        isFixed = false;
        UpdateBulbVisual();
        
        // Анимация поворота в выбитое положение
        targetRotation = brokenRotationQuat;
        isAnimating = true;
        
        Debug.Log($"💥 Рычажок {gameObject.name} выбито! Поворот на {brokenRotation}");
    }

    public void Interact()
    {
        if (!isBroken) 
        {
            Debug.Log("Этот рычажок не выбит");
            return;
        }
        
        if (isFixed) return;

        FixSwitch();
    }

    private void FixSwitch()
    {
        isFixed = true;
        isBroken = false;
        UpdateBulbVisual();
        
        // Анимация поворота обратно в исходное положение
        targetRotation = originalRotation;
        isAnimating = true;

        Debug.Log($"✅ Рычажок {gameObject.name} включён. Возврат в исходное положение");

        if (panel != null)
            panel.OnSwitchFixed();
    }

    private void UpdateBulbVisual()
    {
        if (bulbRenderer == null) return;
        if (originalBulbMaterials == null) return;

        Material[] newMaterials = (Material[])originalBulbMaterials.Clone();

        // Лампочка горит только когда рычажок ВКЛЮЧЁН (не выбит)
        bool shouldGlow = !isBroken;

        if (bulbMaterialIndex >= 0 && bulbMaterialIndex < newMaterials.Length)
        {
            newMaterials[bulbMaterialIndex] = shouldGlow ? bulbOnMaterial : bulbOffMaterial;
        }

        bulbRenderer.materials = newMaterials;
    }

    public void ResetSwitch()
    {
        isBroken = false;
        isFixed = false;
        UpdateBulbVisual();
        
        // Сбрасываем поворот в исходное положение
        targetRotation = originalRotation;
        transform.localRotation = originalRotation;
        isAnimating = false;
    }
    
    // Метод для принудительного сброса анимации (если нужно)
    public void ResetRotation()
    {
        transform.localRotation = originalRotation;
        targetRotation = originalRotation;
        isAnimating = false;
    }

    public string GetDescription()
    {
        if (isBroken) return "🔧 Включить рычажок (выбит)";
        return "✅ Включён";
    }
}