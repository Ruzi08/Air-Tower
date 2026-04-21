using UnityEngine;

public class ElectricalPanel : MonoBehaviour, Interactable
{
    [Header("Визуал (опционально)")]
    public GameObject brokenVisual;   // Сломанный вид щитка (искры, дым)
    public GameObject fixedVisual;    // Починенный вид
    
    private bool isRepaired = false;
    
    public void Interact()
    {
        if (isRepaired)
        {
            Debug.Log("Щиток уже починен");
            return;
        }
        
        if (PowerManager.Instance.HasPower())
        {
            Debug.Log("Электричество уже работает");
            return;
        }
        
        // Чиним!
        isRepaired = true;
        PowerManager.Instance.RestorePower();
        UpdateVisual();
        
        Debug.Log("✅ Электричество восстановлено через щиток!");
    }
    
    private void UpdateVisual()
    {
        if (brokenVisual != null)
            brokenVisual.SetActive(!isRepaired);
        
        if (fixedVisual != null)
            fixedVisual.SetActive(isRepaired);
    }
    
    public string GetDescription()
    {
        if (PowerManager.Instance.HasPower())
            return "⚡ Электричество работает";
        return "🔧 Починить щиток (ЛКМ)";
    }
}