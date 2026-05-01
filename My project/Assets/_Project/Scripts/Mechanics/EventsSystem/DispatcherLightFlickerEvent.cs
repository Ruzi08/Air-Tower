using UnityEngine;
using System.Collections;

public class DispatcherLightFlickerEvent : MonoBehaviour
{
    [Header("Настройки активации")]
    public float activationDelay = 90f;
    public Collider kitchenZone;
    public Collider corridorZone;
    
    [Header("Что контролировать")]
    public LightSwitch dispatcherLightSwitch;
    public Light[] dispatcherLights;
    
    [Header("Настройки моргания")]
    public int flickerCount = 5;
    public float flickerMinInterval = 0.2f;
    public float flickerMaxInterval = 0.6f;
    public float pauseBeforeFlicker = 1f;
    
    [Header("Звуки")]
    public AudioSource flickSound;
    public AudioSource powerBuzzSound;
    
    [Header("Статус")]
    public bool isPlayed = false;
    
    private bool isActive = false;
    private bool hasBeenInKitchen = false;
    private Transform player;
    private bool isFlickering = false;
    private bool[] originalLightsState;
    private bool originalSwitchState;
    
    void Start()
    {
        player = Camera.main.transform;
        Invoke("Activate", activationDelay);
    }
    
    void Activate()
    {
        isActive = true;
        Debug.Log("🎬 Ивент 'Шалости со светом в диспетчерской' активирован");
    }
    
    void Update()
    {
        if (!isActive || isPlayed || isFlickering) return;
        
        bool isInKitchen = kitchenZone.bounds.Contains(player.position);
        bool isInCorridor = corridorZone.bounds.Contains(player.position);
        
        if (isInKitchen)
        {
            hasBeenInKitchen = true;
            Debug.Log("📍 Игрок был на кухне");
        }
        
        if (hasBeenInKitchen && isInCorridor && !isInKitchen)
        {
            Debug.Log("🎭 Игрок вышел в коридор! Начинаем представление...");
            StartCoroutine(StartFlickerShow());
        }
    }
    
    private IEnumerator StartFlickerShow()
    {
        isPlayed = true;
        isFlickering = true;
        
        // Сохраняем исходное состояние
        if (dispatcherLightSwitch != null)
        {
            originalSwitchState = dispatcherLightSwitch.isOn;
        }
        
        if (dispatcherLights != null && dispatcherLights.Length > 0)
        {
            originalLightsState = new bool[dispatcherLights.Length];
            for (int i = 0; i < dispatcherLights.Length; i++)
            {
                if (dispatcherLights[i] != null)
                    originalLightsState[i] = dispatcherLights[i].enabled;
            }
        }
        
        if (powerBuzzSound != null)
            powerBuzzSound.Play();
        
        yield return new WaitForSeconds(pauseBeforeFlicker);
        
        // Моргание
        for (int i = 0; i < flickerCount; i++)
        {
            float interval = Random.Range(flickerMinInterval, flickerMaxInterval);
            yield return StartCoroutine(FlickerOnce(interval));
        }
        
        // Финальное состояние
        bool finalState = Random.value > 0.5f;
        
        if (dispatcherLightSwitch != null)
        {
            if (finalState)
            {
                // 🔥 Включаем с анимацией
                dispatcherLightSwitch.TurnOnWithAnimation();
                Debug.Log("💡 Свет в диспетчерской остался ВКЛЮЧЁН");
            }
            else
            {
                // 🔥 Выключаем с анимацией
                dispatcherLightSwitch.TurnOffWithAnimation();
                Debug.Log("💡 Свет в диспетчерской остался ВЫКЛЮЧЕН");
            }
            dispatcherLightSwitch.UpdateLampsState();
        }
        
        SetLightsState(finalState);
        
        isFlickering = false;
        Debug.Log("🎭 Представление закончено!");
    }
    
    private IEnumerator FlickerOnce(float duration)
    {
        // 🔥 ВЫКЛЮЧАЕМ с анимацией
        if (dispatcherLightSwitch != null)
        {
            dispatcherLightSwitch.TurnOffWithAnimation();
        }
        SetLightsState(false);
        
        if (flickSound != null)
            flickSound.Play();
        
        yield return new WaitForSeconds(duration);
        
        // 🔥 ВКЛЮЧАЕМ с анимацией
        if (dispatcherLightSwitch != null)
        {
            dispatcherLightSwitch.TurnOnWithAnimation();
        }
        SetLightsState(true);
        
        if (flickSound != null)
            flickSound.Play();
        
        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
    }
    
    private void SetLightsState(bool state)
    {
        if (dispatcherLights != null)
        {
            foreach (Light light in dispatcherLights)
            {
                if (light != null)
                    light.enabled = state;
            }
        }
    }
    
    public void ManualTrigger()
    {
        if (!isPlayed)
        {
            StartCoroutine(StartFlickerShow());
        }
    }
}