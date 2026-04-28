using UnityEngine;

public class KettleBoilEvent : MonoBehaviour
{
    [Header("Настройки")]
    public float activationDelay = 45f;
    public Collider kitchenZone;
    public Collider corridorZone;
    
    [Header("Что включить")]
    public SimpleCoffeeMaker coffeeMaker;
    
    private bool isActive = false;
    private bool triggered = false;
    private bool wasInCorridor = false;  // Был ли в коридоре
    private Transform player;
    
    void Start()
    {
        player = Camera.main.transform;
        Invoke("Activate", activationDelay);
    }
    
    void Activate()
    {
        isActive = true;
        Debug.Log("⚡⚡⚡ Ивент 'Чайник в коридоре' АКТИВИРОВАН! ⚡⚡⚡");
    }
    
    void Update()
    {
        if (!isActive || triggered) return;
        
        bool isInKitchen = kitchenZone.bounds.Contains(player.position);
        bool isInCorridor = corridorZone.bounds.Contains(player.position);
        
        // Отладка
        if (isInKitchen) Debug.Log("📍 Игрок на кухне");
        if (isInCorridor) Debug.Log("📍 Игрок в коридоре");
        
        // 🔥 НОВАЯ ЛОГИКА: если игрок в коридоре И НЕ на кухне И мы ещё не триггерили
        if (isInCorridor && !isInKitchen && !triggered)
        {
            Debug.Log("🎯 Игрок в коридоре и не на кухне! Запускаем чайник...");
            TriggerKettleBoil();
        }
    }
    
    private void TriggerKettleBoil()
    {
        triggered = true;
        
        Debug.Log("🔥🔥🔥 TRIGGER KETTLE BOIL ВЫЗВАН! 🔥🔥🔥");
        
        if (coffeeMaker != null)
        {
            coffeeMaker.StartBoiling();
            Debug.Log("🫖 Чайник начал кипеть в коридоре!");
        }
        else
        {
            Debug.LogError("❌ Не назначен CoffeeMaker!");
        }
    }
}