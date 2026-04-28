using UnityEngine;

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
        
        // Устанавливаем правильный поворот
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
        
        SetLampsState(isOn);
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
        
        // Выключаем свет и поворачиваем в нажатое положение
        isOn = false;
        SetLampsState(false);
        UpdateVisual();
        
        targetRotation = originalRotation * Quaternion.Euler(pressedRotation);
        isAnimating = true;
    }
    
    private void HandlePowerRestored()
    {
        hasPower = true;
        UpdateVisual();
        // Не включаем автоматически, просто даём возможность включить
    }
    
    public void Interact()
    {
        if (!hasPower)
        {
            Debug.Log("Нет электричества!");
            return;
        }
        
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
        
        SetLampsState(isOn);
        UpdateVisual();
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
        {
            PlaySnd(sounds[0], volume: maxVolume, p1: minPitch, p2: maxPitch);
        }
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