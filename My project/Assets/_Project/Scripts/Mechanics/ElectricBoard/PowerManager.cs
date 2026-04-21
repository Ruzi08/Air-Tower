using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public static PowerManager Instance { get; private set; }
    
    [Header("Состояние электричества")]
    public bool hasPower = true;
    
    // События для других объектов
    public System.Action OnPowerOut;
    public System.Action OnPowerRestored;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // При старте уведомляем все объекты о состоянии света
        NotifyAllObjects();
    }
    
    public void PowerOut()
    {
        if (!hasPower) return;
        
        hasPower = false;
        Debug.Log("⚡ Электричество отключено!");
        
        // Вызываем событие
        OnPowerOut?.Invoke();
    }
    
    public void RestorePower()
    {
        if (hasPower) return;
        
        hasPower = true;
        Debug.Log("🔌 Электричество восстановлено!");
        
        // Вызываем событие
        OnPowerRestored?.Invoke();
    }
    
    // Принудительно уведомить все объекты
    private void NotifyAllObjects()
    {
        Lamp[] lamps = FindObjectsOfType<Lamp>(true);
        foreach (Lamp lamp in lamps)
            lamp.UpdatePowerState(hasPower);
        
        LightSwitch[] switches = FindObjectsOfType<LightSwitch>(true);
        foreach (LightSwitch sw in switches)
            sw.UpdatePowerState(hasPower);
    }
    
    public bool HasPower() => hasPower;
}