using UnityEngine;
using System.Collections;

public class SimpleCoffeeMaker : MonoBehaviour, Interactable
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
    public Vector3 cupFaceRotation = new Vector3(0, 180, 0);
    
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
    
    [Header("=== ЛАМПОЧКА ===")]
    public Material bulbIdleMaterial;
    public Material bulbBoilingMaterial;
    public Material bulbReadyMaterial;
    public int bulbMaterialIndex = 1;
    
    [Header("=== ЗВУКИ ===")]
    public SimpleSound clickSound;
    public SimpleSound boilSound;
    public SimpleSound pourSound;
    public SimpleSound drinkSound;
    
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
    
    void Start()
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
        
        SetBulbMaterial(bulbIdleMaterial);
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
                
                if (boilSound != null)
                    boilSound.Stop();
                
                SetBulbMaterial(bulbReadyMaterial);
                
                Debug.Log("🔥 Чайник закипел!");
            }
        }
    }
    
    private void SetBulbMaterial(Material material)
    {
        if (material == null) return;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;
        
        Material[] materials = renderer.materials;
        if (bulbMaterialIndex < materials.Length)
        {
            materials[bulbMaterialIndex] = material;
            renderer.materials = materials;
        }
    }
    
    public void Interact()
    {
        if (isPouring || isDrinking) return;
        
        if (clickSound != null)
            clickSound.Play();
        
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
        
        SetBulbMaterial(bulbBoilingMaterial);
        
        if (boilSound != null)
            boilSound.Play();
        
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
        
        if (pourSound != null)
            pourSound.Play();
        
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
            Transform originalParent = cup.transform.parent;
            Vector3 originalPos = cup.transform.position;
            Quaternion originalRot = cup.transform.rotation;
            Vector3 originalScale = cup.transform.localScale;
            
            Collider cupCollider = cup.GetComponent<Collider>();
            if (cupCollider != null) cupCollider.enabled = false;
            
            float t = 0;
            Vector3 startPosWorld = cup.transform.position;
            Vector3 targetPosWorld = cupAnchor.position;
            
            while (t < 1)
            {
                t += Time.deltaTime * cupFlySpeed;
                cup.transform.position = Vector3.Lerp(startPosWorld, targetPosWorld, t);
                yield return null;
            }
            
            cup.transform.LookAt(playerCamera);
            cup.transform.Rotate(cupFaceRotation);
            
            if (drinkSound != null)
                drinkSound.Play();
            
            yield return new WaitForSeconds(1f);
            
            t = 0;
            startPosWorld = cup.transform.position;
            
            while (t < 1)
            {
                t += Time.deltaTime * cupFlySpeed;
                cup.transform.position = Vector3.Lerp(startPosWorld, originalPos, t);
                cup.transform.rotation = Quaternion.Lerp(cup.transform.rotation, originalRot, t);
                yield return null;
            }
            
            cup.transform.SetParent(originalParent);
            cup.transform.position = originalPos;
            cup.transform.rotation = originalRot;
            cup.transform.localScale = originalScale;
            
            if (cupCollider != null) cupCollider.enabled = true;
        }
        
        SetBulbMaterial(bulbIdleMaterial);
        
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