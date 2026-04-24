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
    
    public void PowerOut()
    {
        if (!hasPower) return;
        
        hasPower = false;
        Debug.Log("⚡ Электричество отключено!");
        
        OnPowerOut?.Invoke();
    }
    
    public void RestorePower()
    {
        if (hasPower) return;
        
        hasPower = true;
        Debug.Log("🔌 Электричество восстановлено!");
        
        OnPowerRestored?.Invoke();
    }
    
    public bool HasPower() => hasPower;
}