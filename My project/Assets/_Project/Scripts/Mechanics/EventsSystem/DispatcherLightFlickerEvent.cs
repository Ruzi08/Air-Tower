using UnityEngine;
using System.Collections;

public class DispatcherLightFlickerEvent : MonoBehaviour
{
    [Header("Настройки активации")]
    public float activationDelay = 90f;
    public Collider kitchenZone;                 // Зона кухни
    public Collider corridorZone;                // Зона коридора
    public Collider stopZone;                    // 🔥 Зона, после пересечения которой моргание прекращается
    
    [Header("Что контролировать")]
    public LightSwitch dispatcherLightSwitch;
    public Light[] dispatcherLights;
    
    [Header("Настройки моргания")]
    public float flickerMinInterval = 0.2f;
    public float flickerMaxInterval = 0.6f;
    public float pauseBeforeFlicker = 1f;
    
    [Header("Звуки")]
    public AudioSource flickSound;
    public AudioSource powerBuzzSound;
    
    [Header("Статус")]
    public bool isPlayed = false;
    public bool isFlickeringActive = false;      // 🔥 Активно ли сейчас моргание
    
    private bool isActive = false;
    private bool hasBeenInKitchen = false;
    private Transform player;
    private Coroutine flickerCoroutine;
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
        if (!isActive || isPlayed) return;
        
        bool isInKitchen = kitchenZone.bounds.Contains(player.position);
        bool isInCorridor = corridorZone.bounds.Contains(player.position);
        bool isInStopZone = stopZone != null && stopZone.bounds.Contains(player.position);
        
        // 🔥 Если игрок зашёл в стоп-зону - останавливаем моргание
        if (isInStopZone && isFlickeringActive)
        {
            Debug.Log("🛑 Игрок зашёл в стоп-зону! Останавливаем моргание...");
            StopFlickering();
            isPlayed = true;
            return;
        }
        
        if (isInKitchen)
        {
            hasBeenInKitchen = true;
            Debug.Log("📍 Игрок был на кухне");
        }
        
        // Запускаем моргание только если ещё не запущено и игрок вышел в коридор
        if (!isFlickeringActive && hasBeenInKitchen && isInCorridor && !isInKitchen)
        {
            Debug.Log("🎭 Игрок вышел в коридор! Начинаем бесконечное представление...");
            StartFlickering();
        }
    }
    
    private void StartFlickering()
    {
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);
        
        isFlickeringActive = true;
        
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
        
        flickerCoroutine = StartCoroutine(InfiniteFlickerCoroutine());
    }
    
    private void StopFlickering()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
        
        isFlickeringActive = false;
        
        // Восстанавливаем исходное состояние (опционально)
        // SetLightsState(true);
        // if (dispatcherLightSwitch != null)
        // {
        //     dispatcherLightSwitch.TurnOnWithAnimation();
        //     dispatcherLightSwitch.UpdateLampsState();
        // }
        
        Debug.Log("🎭 Моргание остановлено!");
    }
    
    private IEnumerator InfiniteFlickerCoroutine()
    {
        // Звук "игр с электричеством"
        if (powerBuzzSound != null)
            powerBuzzSound.Play();
        
        yield return new WaitForSeconds(pauseBeforeFlicker);
        
        // Бесконечное моргание
        while (isFlickeringActive)
        {
            float interval = Random.Range(flickerMinInterval, flickerMaxInterval);
            yield return StartCoroutine(FlickerOnce(interval));
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }
    
    private IEnumerator FlickerOnce(float duration)
    {
        // Выключаем
        if (dispatcherLightSwitch != null)
        {
            dispatcherLightSwitch.TurnOffWithAnimation();
        }
        SetLightsState(false);
        
        if (flickSound != null)
            flickSound.Play();
        
        yield return new WaitForSeconds(duration);
        
        // Включаем
        if (dispatcherLightSwitch != null)
        {
            dispatcherLightSwitch.TurnOnWithAnimation();
        }
        SetLightsState(true);
        
        if (flickSound != null)
            flickSound.Play();
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
    
    // Метод для ручного запуска
    public void ManualStart()
    {
        if (!isPlayed && !isFlickeringActive)
        {
            StartFlickering();
        }
    }
    
    // Метод для ручной остановки
    public void ManualStop()
    {
        if (isFlickeringActive)
        {
            StopFlickering();
            isPlayed = true;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (stopZone != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(stopZone.bounds.center, stopZone.bounds.size);
        }
    }
}