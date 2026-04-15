using UnityEngine;
using System.Collections;

public class SimpleCoffeeMaker : Sound, Interactable
{
    [Header("=== НАСТРОЙКИ ===")]
    public float boilTime = 10f;
    public float coffeeRestoreAmount = 25f;
    
    [Header("=== СОСТОЯНИЯ ===")]
    public bool isBoiling = false;
    public bool isWaterHot = false;
    public bool isCoffeeReady = false;
    
    [Header("=== АНИМАЦИЯ ЧАЙНИКА ===")]
    public Transform pourPoint;
    public Vector3 pourRotation = new Vector3(-30, 0, 0);
    public float kettleFlySpeed = 3f;
    public float pourDuration = 1.5f;
    
    [Header("=== АНИМАЦИЯ КРУЖКИ ===")]
    public Transform cupAnchor;
    public float cupFlySpeed = 10f;
    
    [Header("=== ССЫЛКИ ===")]
    public FatigueManager fatigueManager;
    public GameObject cup;
    public MeshRenderer cupRenderer;
    public Material emptyMaterial;
    public Material fullMaterial;
    public Transform playerCamera;
    
    [Header("=== ЧАСТИЦЫ ===")]
    public ParticleSystem steamParticles;
    public ParticleSystem pourParticles;
    
    // Сохраняем исходные данные кружки
    private Vector3 kettleOriginalPosition;
    private Quaternion kettleOriginalRotation;
    private Vector3 cupOriginalPosition;
    private Quaternion cupOriginalRotation;
    private Transform cupOriginalParent;
    private Vector3 cupOriginalScale;
    private float boilTimer = 0;
    private bool isPouring = false;
    private bool isDrinking = false;
    private FirstPersonController playerController;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            if (playerCamera == null)
                playerCamera = player.GetComponentInChildren<Camera>().transform;
        }
        
        if (fatigueManager == null)
            fatigueManager = FindObjectOfType<FatigueManager>();
        
        kettleOriginalPosition = transform.position;
        kettleOriginalRotation = transform.rotation;
        
        if (cup != null)
        {
            cupOriginalPosition = cup.transform.position;
            cupOriginalRotation = cup.transform.rotation;
            cupOriginalParent = cup.transform.parent;
            cupOriginalScale = cup.transform.localScale;
            
            if (emptyMaterial != null && cupRenderer != null)
                cupRenderer.material = emptyMaterial;
        }
        
        if (steamParticles != null)
            steamParticles.Stop();
        if (pourParticles != null)
            pourParticles.Stop();
        
        StopSnd();
    }
    
    void Update()
    {
        if (isBoiling)
        {
            boilTimer += Time.deltaTime;
            if (boilTimer >= boilTime)
            {
                isBoiling = false;
                isWaterHot = true;
                
                if (steamParticles != null)
                    steamParticles.Stop();
                
                StopSnd();
                Debug.Log("🔥 Чайник закипел!");
            }
        }
    }
    
    public void Interact()
    {
        if (isPouring || isDrinking) return;
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
            PlaySnd(sounds[0], volume: 0.5f);
        
        if (isCoffeeReady)
        {
            StartCoroutine(DrinkCoffee());
        }
        else if (isWaterHot && !isCoffeeReady)
        {
            StartCoroutine(PourCoffee());
        }
        else if (!isBoiling && !isWaterHot && !isCoffeeReady)
        {
            StartBoiling();
        }
    }
    
    void StartBoiling()
    {
        isBoiling = true;
        boilTimer = 0;
        
        if (steamParticles != null)
            steamParticles.Play();
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
            PlaySnd(sounds[0], volume: maxVolume, loop: true);
        
        Debug.Log("🫖 Чайник начал кипеть!");
    }
    
    IEnumerator PourCoffee()
    {
        isPouring = true;
        
        float t = 0;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        while (t < 1)
        {
            t += Time.deltaTime * kettleFlySpeed;
            transform.position = Vector3.Lerp(startPos, pourPoint.position, t);
            transform.rotation = Quaternion.Lerp(startRot, pourPoint.rotation, t);
            yield return null;
        }
        
        Quaternion targetRot = transform.rotation * Quaternion.Euler(pourRotation);
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * kettleFlySpeed;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);
            yield return null;
        }
        
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
            PlaySnd(sounds[0], volume: maxVolume);
        
        if (pourParticles != null)
            pourParticles.Play();
        
        yield return new WaitForSeconds(pourDuration);
        
        if (pourParticles != null)
            pourParticles.Stop();
        
        t = 0;
        startPos = transform.position;
        startRot = transform.rotation;
        while (t < 1)
        {
            t += Time.deltaTime * kettleFlySpeed;
            transform.position = Vector3.Lerp(startPos, kettleOriginalPosition, t);
            transform.rotation = Quaternion.Lerp(startRot, kettleOriginalRotation, t);
            yield return null;
        }
        
        if (cupRenderer != null && fullMaterial != null)
            cupRenderer.material = fullMaterial;
        
        isWaterHot = false;
        isCoffeeReady = true;
        isPouring = false;
        
        Debug.Log("☕ Кофе заварен! Нажми на кружку");
    }
    
IEnumerator DrinkCoffee()
{
    if (isDrinking) yield break;
    isDrinking = true;
    
    if (playerController != null)
        playerController.LockAll();
    
    if (cup != null && cupAnchor != null)
    {
        // Сохраняем исходные данные
        Transform originalParent = cup.transform.parent;
        Vector3 originalPos = cup.transform.position;
        Quaternion originalRot = cup.transform.rotation;
        Vector3 originalScale = cup.transform.localScale;
        
        // Отключаем коллайдер на время полёта
        Collider cupCollider = cup.GetComponent<Collider>();
        if (cupCollider != null) cupCollider.enabled = false;
        
        // Анимация полёта к камере (позиция + поворот)
        float t = 0;
        Vector3 startPosWorld = cup.transform.position;
        Quaternion startRotWorld = cup.transform.rotation;
        Vector3 targetPosWorld = cupAnchor.position;
        Quaternion targetRotWorld = cupAnchor.rotation;
        
        while (t < 1)
        {
            t += Time.deltaTime * cupFlySpeed;
            cup.transform.position = Vector3.Lerp(startPosWorld, targetPosWorld, t);
            cup.transform.rotation = Quaternion.Lerp(startRotWorld, targetRotWorld, t);
            yield return null;
        }
        
        // Звук питья
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
            PlaySnd(sounds[0], volume: maxVolume);
        
        yield return new WaitForSeconds(1f);
        
        // Анимация полёта обратно (позиция + поворот)
        t = 0;
        startPosWorld = cup.transform.position;
        startRotWorld = cup.transform.rotation;
        
        while (t < 1)
        {
            t += Time.deltaTime * cupFlySpeed;
            cup.transform.position = Vector3.Lerp(startPosWorld, originalPos, t);
            cup.transform.rotation = Quaternion.Lerp(startRotWorld, originalRot, t);
            yield return null;
        }
        
        // Возвращаем кружку в исходное состояние
        cup.transform.SetParent(originalParent);
        cup.transform.position = originalPos;
        cup.transform.rotation = originalRot;
        cup.transform.localScale = originalScale;
        
        if (cupCollider != null) cupCollider.enabled = true;
    }
    
    if (fatigueManager != null)
        fatigueManager.RestoreWakeness(coffeeRestoreAmount);
    
    if (cupRenderer != null && emptyMaterial != null)
        cupRenderer.material = emptyMaterial;
    
    isCoffeeReady = false;
    isDrinking = false;
    
    if (playerController != null)
        playerController.UnlockAll();
    
    Debug.Log($"🥤 Кофе выпит! +{coffeeRestoreAmount}% бодрости");
}
    
    public string GetDescription()
    {
        if (isDrinking) return "🥤 Пью кофе...";
        if (isPouring) return "☕ Наливаю кофе...";
        if (isCoffeeReady) return "☕ Выпить кофе";
        if (isWaterHot) return "☕ Налить кофе";
        if (isBoiling) return $"🫖 Кипит... {(boilTime - boilTimer):F0} сек";
        return "🫖 Вскипятить чайник";
    }
}