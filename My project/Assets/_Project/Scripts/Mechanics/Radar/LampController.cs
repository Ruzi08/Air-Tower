using UnityEngine;

public class LampController : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material offMaterial;    // Базовый материал (выключено)
    [SerializeField] private Material onMaterial;     // Материал цвета (включено)

    [Header("Settings")]
    [SerializeField] private LampType lampType = LampType.Green;

    private Renderer lampRenderer;
    private bool isOn = false;

    public enum LampType
    {
        Green,   // Нет опасности
        Yellow,  // Есть предупреждение
        Red      // Критическая опасность
    }

    void Start()
    {
        lampRenderer = GetComponent<Renderer>();
        TurnOff();
    }

    public void TurnOn()
    {
        if (lampRenderer != null && onMaterial != null)
        {
            lampRenderer.material = onMaterial;
            isOn = true;
        }
    }

    public void TurnOff()
    {
        if (lampRenderer != null && offMaterial != null)
        {
            lampRenderer.material = offMaterial;
            isOn = false;
        }
    }

    public void SetState(bool on)
    {
        if (on) TurnOn();
        else TurnOff();
    }

    public LampType GetLampType()
    {
        return lampType;
    }
}