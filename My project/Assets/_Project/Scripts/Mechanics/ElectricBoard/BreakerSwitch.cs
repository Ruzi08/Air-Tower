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

    private Material[] originalBulbMaterials;

    void Start()
    {
        if (bulbRenderer != null)
        {
            originalBulbMaterials = bulbRenderer.materials;
        }

        UpdateBulbVisual();
    }

    public void SetBroken()
    {
        if (isFixed) return;
        isBroken = true;
        isFixed = false;
        UpdateBulbVisual();
        Debug.Log($"💥 Рычажок {gameObject.name} выбило!");
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

        Debug.Log($"✅ Рычажок {gameObject.name} включён");

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
    }

    public string GetDescription()
    {
        if (isBroken) return "🔧 Включить рычажок (выбит)";
        return "✅ Включён";
    }
}