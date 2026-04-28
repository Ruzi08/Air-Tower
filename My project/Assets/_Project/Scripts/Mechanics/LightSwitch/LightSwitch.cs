using UnityEngine;
using System.Collections;

public class LightSwitch : Sound, Interactable
{
    [Header("Настройки выключателя")]
    public Lamp[] lamps;
    public bool isOn = true;
    
    [Header("Визуальные эффекты")]
    public GameObject switchOnVisual;
    public GameObject switchOffVisual;
    
    [Header("Анимация поворота")]
    public Vector3 pressedRotation = new Vector3(15f, 0, 0);
    public float animationSpeed = 15f;
    
    private bool hasPower = true;
    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private bool isAnimating = false;
    
    protected override void Start()
    {
        AudioSrc = GetComponent<AudioSource>();
        if (AudioSrc == null)
            AudioSrc = gameObject.AddComponent<AudioSource>();
        
        volume = maxVolume;
        CurrentVolume = minVolume;
        TargetVolume = minVolume;
        
        originalRotation = transform.localRotation;
        
        if (isOn)
        {
            targetRotation = originalRotation;
            transform.localRotation = originalRotation;
        }
        else
        {
            targetRotation = originalRotation * Quaternion.Euler(pressedRotation);
            transform.localRotation = targetRotation;
        }
        
        if (PowerManager.Instance != null)
        {
            hasPower = PowerManager.Instance.HasPower();
        }
        
        UpdateLampsState();
        UpdateVisual();
        
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += HandlePowerOut;
            PowerManager.Instance.OnPowerRestored += HandlePowerRestored;
        }
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
        isOn = false;
        
        targetRotation = originalRotation * Quaternion.Euler(pressedRotation);
        isAnimating = true;
        
        UpdateLampsState();
        UpdateVisual();
        
        Debug.Log($"{name}: электричество пропало -> выключатель сброшен в OFF");
    }
    
    private void HandlePowerRestored()
    {
        hasPower = true;
        
        UpdateLampsState();
        UpdateVisual();
        
        Debug.Log($"{name}: электричество вернулось, isOn={isOn}, лампы должны {(hasPower && isOn ? "гореть" : "не гореть")}");
        
        StartCoroutine(DelayedLampUpdate());
    }
    
    private IEnumerator DelayedLampUpdate()
    {
        yield return null;
        UpdateLampsState();
        Debug.Log($"{name}: повторное обновление ламп, shouldBeOn={hasPower && isOn}");
    }
    
    public void Interact()
    {
        if (isAnimating) return;
        
        isOn = !isOn;
        
        if (isOn)
        {
            targetRotation = originalRotation;
        }
        else
        {
            targetRotation = originalRotation * Quaternion.Euler(pressedRotation);
        }
        isAnimating = true;
        
        UpdateLampsState();
        UpdateVisual();
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
        {
            PlaySnd(sounds[0], volume: maxVolume, p1: minPitch, p2: maxPitch);
        }
        
        Debug.Log($"Выключатель {(isOn ? "включён" : "выключен")}, свет: {(hasPower && isOn ? "горит" : "не горит")}");
    }
    
    public void UpdateLampsState()
    {
        bool shouldBeOn = hasPower && isOn;
        
        foreach (Lamp lamp in lamps)
        {
            if (lamp != null)
            {
                if (shouldBeOn) 
                {
                    lamp.TurnOn();
                    Debug.Log($"{name}: включаю лампу {lamp.name}");
                }
                else 
                {
                    lamp.TurnOff();
                    Debug.Log($"{name}: выключаю лампу {lamp.name}");
                }
            }
        }
    }
    
    public void UpdateVisual()
    {
        if (switchOnVisual != null)
            switchOnVisual.SetActive(isOn);
        
        if (switchOffVisual != null)
            switchOffVisual.SetActive(!isOn);
    }
    
    public void UpdatePowerState(bool powerAvailable)
    {
        hasPower = powerAvailable;
        UpdateLampsState();
        UpdateVisual();
    }
    
    public void ResetToDefaultState()
    {
        isOn = true;
        targetRotation = originalRotation;
        isAnimating = true;
        UpdateLampsState();
        UpdateVisual();
    }
    
    // 🔥 НОВЫЙ МЕТОД для принудительного выключения с анимацией
    public void SetOffState()
    {
        if (isAnimating) return;
        
        isOn = false;
        targetRotation = originalRotation * Quaternion.Euler(pressedRotation);
        isAnimating = true;
        
        UpdateVisual();
        UpdateLampsState();
        
        Debug.Log($"{name}: принудительно выключен через SetOffState()");
    }
    
    public string GetDescription()
    {
        if (!hasPower) return "💀 Нет электричества...";
        return isOn ? "Нажмите, чтобы выключить свет" : "Нажмите, чтобы включить свет";
    }
}