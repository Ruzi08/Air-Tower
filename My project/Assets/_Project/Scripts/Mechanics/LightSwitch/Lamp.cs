using UnityEngine;

public class Lamp : MonoBehaviour
{
    [Header("Настройки лампы")]
    public Light lightSource;      // Компонент света
    public bool startOn = true;    // Включена ли при старте
    
    private bool isOn;
    
    void Start()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();
        
        isOn = startOn;
        lightSource.enabled = isOn;
    }
    
    public void TurnOn()
    {
        isOn = true;
        lightSource.enabled = true;
    }
    
    public void TurnOff()
    {
        isOn = false;
        lightSource.enabled = false;
    }
    
    public void Toggle()
    {
        if (isOn) TurnOff();
        else TurnOn();
    }
    
    public bool IsOn() => isOn;
}