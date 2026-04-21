using UnityEngine;

public class LightSwitch : Sound, Interactable
{
    [Header("Настройки выключателя")]
    public Lamp[] lamps;
    public bool isOn = true;
    
    [Header("Визуальные эффекты")]
    public GameObject switchOnVisual;
    public GameObject switchOffVisual;
    
    private bool hasPower = true;
    
    protected override void Start()
    {
        AudioSrc = GetComponent<AudioSource>();
        if (AudioSrc == null)
            AudioSrc = gameObject.AddComponent<AudioSource>();
        
        volume = maxVolume;
        CurrentVolume = minVolume;
        TargetVolume = minVolume;
        
        SetLampsState(isOn);
        UpdateVisual();
        
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
        
        // ✅ Синхронизируем isOn с реальностью: лампы выключены, значит isOn = false
        isOn = false;
        
        UpdateVisual();
        
        foreach (Lamp lamp in lamps)
            if (lamp != null) lamp.TurnOff();
        
        Debug.Log($"Выключатель: электричество пропало, isOn сброшен на false");
    }
    
    private void HandlePowerRestored()
    {
        hasPower = true;
        UpdateVisual();
        
        // ✅ НЕ включаем лампы автоматически, isOn остаётся false
        Debug.Log($"Выключатель: электричество вернулось, isOn={isOn}, нужно нажать чтобы включить");
    }
    
    public void Interact()
    {
        if (!hasPower)
        {
            Debug.Log("Нет электричества! Выключатель не работает.");
            return;
        }
        
        isOn = !isOn;
        SetLampsState(isOn);
        UpdateVisual();
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
        {
            PlaySnd(sounds[0], volume: maxVolume, p1: minPitch, p2: maxPitch);
        }
        
        Debug.Log($"Свет {(isOn ? "включён" : "выключен")}");
    }
    
    private void SetLampsState(bool state)
    {
        foreach (Lamp lamp in lamps)
        {
            if (lamp != null)
            {
                if (state) lamp.TurnOn();
                else lamp.TurnOff();
            }
        }
    }
    
    private void UpdateVisual()
    {
        if (switchOnVisual != null)
            switchOnVisual.SetActive(isOn && hasPower);
        
        if (switchOffVisual != null)
            switchOffVisual.SetActive(!isOn || !hasPower);
    }
    
    public void UpdatePowerState(bool powerAvailable)
    {
        hasPower = powerAvailable;
        UpdateVisual();
    }
    
    public string GetDescription()
    {
        if (!hasPower) return "💀 Нет электричества...";
        return isOn ? "Нажмите, чтобы выключить свет" : "Нажмите, чтобы включить свет";
    }
}