using UnityEngine;

public class Lamp : MonoBehaviour
{
    [Header("Настройки лампы")]
    public Light lightSource;
    public bool startOn = true;
    
    private bool isOn;
    private bool hasPower = true;
    
    void Start()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();
        
        isOn = startOn;
        UpdateLight();
        
        // ПОДПИСЫВАЕМСЯ НА СОБЫТИЯ PowerManager
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += HandlePowerOut;
            PowerManager.Instance.OnPowerRestored += HandlePowerRestored;
        }
    }
    
    void OnDestroy()
    {
        // Отписываемся при уничтожении
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= HandlePowerOut;
            PowerManager.Instance.OnPowerRestored -= HandlePowerRestored;
        }
    }
    
    private void HandlePowerOut()
    {
        hasPower = false;
        UpdateLight();
        Debug.Log($"Лампа: свет пропал");
    }
    
    private void HandlePowerRestored()
    {
        hasPower = true;
        UpdateLight();
        Debug.Log($"Лампа: свет вернулся");
    }
    
    public void TurnOn()
    {
        if (!hasPower) return;
        isOn = true;
        UpdateLight();
    }
    
    public void TurnOff()
    {
        isOn = false;
        UpdateLight();
    }
    
    public void Toggle()
    {
        if (isOn) TurnOff();
        else TurnOn();
    }
    
    private void UpdateLight()
    {
        if (lightSource != null)
            lightSource.enabled = isOn && hasPower;
    }
    
    public void UpdatePowerState(bool power)
    {
        hasPower = power;
        UpdateLight();
    }
    
    public bool IsOn() => isOn && hasPower;
}