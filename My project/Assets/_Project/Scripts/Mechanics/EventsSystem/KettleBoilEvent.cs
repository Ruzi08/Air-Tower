using UnityEngine;

public class KettleBoilEvent : MonoBehaviour
{
    [Header("Настройки")]
    public float activationDelay = 45f;
    public Collider kitchenZone;
    public Collider corridorZone;
    
    [Header("Что включить")]
    public SimpleCoffeeMaker coffeeMaker;
    
    [Header("Статус")]
    public bool isPlayed = false;             // 🔥 Был ли выполнен ивент
    
    private bool isActive = false;
    private bool hasBeenInKitchen = false;
    private Transform player;
    
    void Start()
    {
        player = Camera.main.transform;
        Invoke("Activate", activationDelay);
    }
    
    void Activate()
    {
        isActive = true;
        Debug.Log("⚡ Ивент 'Чайник в коридоре' активирован");
    }
    
    void Update()
    {
        if (!isActive || isPlayed) return; // 🔥 Проверяем isPlayed
        
        bool isInKitchen = kitchenZone.bounds.Contains(player.position);
        bool isInCorridor = corridorZone.bounds.Contains(player.position);
        
        if (isInKitchen)
        {
            hasBeenInKitchen = true;
            Debug.Log("📍 Игрок был на кухне");
        }
        
        if (hasBeenInKitchen && isInCorridor && !isInKitchen)
        {
            Debug.Log("🎯 Игрок был на кухне, теперь в коридоре! Запускаем чайник...");
            TriggerKettleBoil();
        }
    }
    
    private void TriggerKettleBoil()
    {
        isPlayed = true; // 🔥 Отмечаем что ивент выполнен
        
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