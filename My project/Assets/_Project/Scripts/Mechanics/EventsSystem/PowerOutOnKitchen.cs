using UnityEngine;

public class PowerOutOnKitchen : MonoBehaviour
{
    [Header("Настройки")]
    public float activationDelay = 60f;      // Через сколько секунд ивент станет активным
    public Collider kitchenZone;              // Зона кухни
    public float timeRequired = 5f;           // Сколько секунд нужно пробыть на кухне
    
    [Header("Что выключить")]
    public LightSwitch dispatcherLightSwitch; // Перетащи сюда LightSwitch диспетчерской
    
    [Header("Эффекты")]
    public AudioSource switchSound;           // Звук выключателя (опционально)
    
    private bool isActive = false;
    private bool triggered = false;
    private float timeInKitchen = 0f;
    private Transform player;
    
    void Start()
    {
        player = Camera.main.transform;
        Invoke("Activate", activationDelay);
    }
    
    void Activate()
    {
        isActive = true;
        Debug.Log("⚡ Ивент выключения света в диспетчерской активирован");
    }
    
    void Update()
    {
        if (!isActive || triggered) return;
        
        bool isInKitchen = kitchenZone.bounds.Contains(player.position);
        
        if (isInKitchen)
        {
            timeInKitchen += Time.deltaTime;
            if (timeInKitchen >= timeRequired)
            {
                TriggerPowerOut();
            }
        }
        else
        {
            timeInKitchen = 0f;
        }
    }
    
    private void TriggerPowerOut()
    {
        triggered = true;
        
        if (dispatcherLightSwitch != null)
        {
            // 🔥 Выключаем свет и поворачиваем выключатель
            dispatcherLightSwitch.isOn = false;
            
            // 🔥 Запускаем анимацию поворота в OFF
            dispatcherLightSwitch.SetOffState();
            
            // Обновляем лампы
            dispatcherLightSwitch.UpdateLampsState();
            
            // Проигрываем звук щелчка
            if (switchSound != null)
                switchSound.Play();
            
            Debug.Log("💡 Свет в диспетчерской выключен! Выключатель повёрнут в OFF");
        }
        else
        {
            Debug.LogError("❌ Не назначен LightSwitch диспетчерской!");
        }
    }
}