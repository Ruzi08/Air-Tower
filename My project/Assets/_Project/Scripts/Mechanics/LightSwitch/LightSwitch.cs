using UnityEngine;

public class LightSwitch : Sound, Interactable
{
    [Header("Настройки выключателя")]
    public Lamp[] lamps;
    public bool isOn = true;
    
    [Header("Визуальные эффекты")]
    public GameObject switchOnVisual;
    public GameObject switchOffVisual;
    
    protected override void Start()
    {
        // НЕ вызываем base.Start() — чтобы не играл звук при старте
        // Вместо этого инициализируем только необходимое
        
        // Инициализируем AudioSource (как в Sound.Awake, но без звука)
        AudioSrc = GetComponent<AudioSource>();
        if (AudioSrc == null)
            AudioSrc = gameObject.AddComponent<AudioSource>();
        
        volume = maxVolume;
        CurrentVolume = minVolume;
        TargetVolume = minVolume;
        
        // Устанавливаем начальное состояние ламп
        SetLampsState(isOn);
        UpdateVisual();
    }
    
    public void Interact()
    {
        isOn = !isOn;
        SetLampsState(isOn);
        UpdateVisual();
        
        // Воспроизводим звук при нажатии
        PlayClickSound();
        
        Debug.Log($"Свет { (isOn ? "включён" : "выключен") }");
    }
    
    private void PlayClickSound()
    {
        // Используем метод Sound.PlaySnd
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
            switchOnVisual.SetActive(isOn);
        
        if (switchOffVisual != null)
            switchOffVisual.SetActive(!isOn);
    }
    
    public string GetDescription()
    {
        return isOn ? "Нажмите, чтобы выключить свет" : "Нажмите, чтобы включить свет";
    }
}