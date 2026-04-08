using UnityEngine;
using System.Collections;

public class SimpleCoffeeMaker : MonoBehaviour, Interactable
{
    [Header("=== НАСТРОЙКИ ===")]
    public float boilTime = 10f;
    public float hotWaterDuration = 20f;
    public float coffeeRestoreAmount = 25f;
    
    [Header("=== СОСТОЯНИЯ ===")]
    public bool isBoiling = false;
    public bool isWaterHot = false;
    public bool isCoffeeReady = false;
    
    [Header("=== ССЫЛКИ ===")]
    public FatigueManager fatigueManager;
    
    private float boilTimer = 0;
    private float hotWaterTimer = 0;
    private Renderer myRenderer;
    private Color originalColor;
    
    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
            originalColor = myRenderer.material.color;
        
        if (fatigueManager == null)
            fatigueManager = FindObjectOfType<FatigueManager>();
        
        Debug.Log($"✅ Чайник {gameObject.name} готов! Нажми ЛКМ по мне");
        Debug.Log($"📊 Параметры: время кипения={boilTime}сек, восстановление={coffeeRestoreAmount}%");
    }
    
    void Update()
    {
        if (isBoiling)
        {
            boilTimer += Time.deltaTime;
            if (myRenderer != null)
            {
                float progress = boilTimer / boilTime;
                myRenderer.material.color = Color.Lerp(Color.yellow, Color.red, progress);
            }
            
            if (boilTimer >= boilTime)
            {
                isBoiling = false;
                isWaterHot = true;
                hotWaterTimer = 0;
                if (myRenderer != null)
                    myRenderer.material.color = Color.red;
                Debug.Log($"🔥 ЧАЙНИК ЗАКИПЕЛ! Вода горячая {hotWaterDuration} секунд");
            }
        }
        
        if (isWaterHot && !isBoiling)
        {
            hotWaterTimer += Time.deltaTime;
            if (hotWaterTimer >= hotWaterDuration)
            {
                isWaterHot = false;
                if (myRenderer != null)
                    myRenderer.material.color = originalColor;
                Debug.Log("🌡️ Вода остыла!");
            }
        }
    }
    
    public void Interact()
    {
        Debug.Log($"🖱️ Нажал на чайник! Статус: кипит={isBoiling}, вода горячая={isWaterHot}, кофе готов={isCoffeeReady}");
        
        if (isCoffeeReady)
        {
            DrinkCoffee();
        }
        else if (isWaterHot && !isCoffeeReady)
        {
            BrewCoffee();
        }
        else if (!isBoiling && !isWaterHot)
        {
            StartBoiling();
        }
        else if (isBoiling)
        {
            float remaining = boilTime - boilTimer;
            Debug.Log($"🫖 Чайник кипит! Осталось {remaining:F1} секунд");
        }
    }
    
    public string GetDescription()
    {
        if (isCoffeeReady) return "☕ Выпить кофе";
        if (isWaterHot) return "☕ Заварить кофе";
        if (isBoiling) return $"🫖 Кипит... {(boilTime - boilTimer):F0} сек";
        return "🫖 Вскипятить чайник";
    }
    
    void StartBoiling()
    {
        isBoiling = true;
        boilTimer = 0;
        Debug.Log($"🫖 ЧАЙНИК НАЧАЛ КИПЕТЬ! Жди {boilTime} секунд");
    }
    
    void BrewCoffee()
    {
        isCoffeeReady = true;
        isWaterHot = false;
        if (myRenderer != null)
            myRenderer.material.color = new Color(0.55f, 0.27f, 0.07f);
        Debug.Log("☕ КОФЕ ЗАВАРЕНО! Нажми ЛКМ чтобы выпить");
    }
    
    void DrinkCoffee()
    {
        if (fatigueManager != null)
        {
            fatigueManager.RestoreWakeness(coffeeRestoreAmount);
            Debug.Log($"🥤 КОФЕ ВЫПИТ! +{coffeeRestoreAmount}% бодрости");
        }
        else
        {
            Debug.LogError("❌ FatigueManager не найден!");
        }
        
        isCoffeeReady = false;
        if (myRenderer != null)
            myRenderer.material.color = originalColor;
    }
}