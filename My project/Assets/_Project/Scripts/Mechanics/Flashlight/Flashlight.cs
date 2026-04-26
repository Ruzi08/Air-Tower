using UnityEngine;
using System.Collections;

public class Flashlight : MonoBehaviour
{
    [Header("Настройки фонарика")]
    public Light flashlightLight;           // Компонент Light
    public float intensity = 2f;            // Яркость
    public float range = 20f;               // Дальность
    public float spotAngle = 40f;           // Угол луча (для SpotLight)
    
    [Header("Эффект мигания при включении")]
    public float blinkDelay = 1f;            // Задержка перед миганием
    public int blinkCount = 3;               // Количество миганий
    public float blinkInterval = 0.1f;       // Интервал между миганиями
    
    private bool isActive = false;
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    
    void Start()
    {
        if (flashlightLight == null)
            flashlightLight = GetComponent<Light>();
        
        if (flashlightLight == null)
        {
            Debug.LogError("Flashlight: не найден компонент Light!");
            return;
        }
        
        // Настройки света
        flashlightLight.type = LightType.Spot;
        flashlightLight.intensity = intensity;
        flashlightLight.range = range;
        flashlightLight.spotAngle = spotAngle;
        
        // По умолчанию выключен
        flashlightLight.enabled = false;
        
        // Подписываемся на события электричества
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += OnPowerOut;
            PowerManager.Instance.OnPowerRestored += TurnOff;
        }
    }
    
    void OnDestroy()
    {
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= OnPowerOut;
            PowerManager.Instance.OnPowerRestored -= TurnOff;
        }
    }
    
    private void OnPowerOut()
    {
        // Запускаем мигание с задержкой
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkAndTurnOn());
    }
    
    private IEnumerator BlinkAndTurnOn()
    {
        isBlinking = true;
        
        // Ждём перед началом мигания
        yield return new WaitForSeconds(blinkDelay);
        
        // Мигаем несколько раз
        for (int i = 0; i < blinkCount; i++)
        {
            flashlightLight.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
            flashlightLight.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // Включаем окончательно
        flashlightLight.enabled = true;
        isActive = true;
        isBlinking = false;
        Debug.Log("🔦 Фонарик включился после мигания");
    }
    
    private void TurnOff()
    {
        // Если сейчас идёт мигание — прерываем
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            isBlinking = false;
        }
        
        if (flashlightLight != null)
        {
            flashlightLight.enabled = false;
            isActive = false;
            Debug.Log("🔦 Фонарик выключился (свет вернулся)");
        }
    }
    
    public bool IsActive() => isActive;
    public bool IsBlinking() => isBlinking;
}