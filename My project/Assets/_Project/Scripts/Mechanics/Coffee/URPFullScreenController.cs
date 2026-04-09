using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPFullScreenController : MonoBehaviour
{
    [Header("Материалы шейдеров")]
    public Material blinkMaterial;
    public Material pivoMaterial;
    
    [Header("Параметры шейдеров")]
    public string blinkAlphaParam = "_Alpha";
    public string pivoScaleParam = "_NoiseScale";
    
    [Header("Renderer Features (перетащи из PC_Renderer)")]
    public UnityEngine.Rendering.Universal.ScriptableRendererFeature pivoRendererFeature;
    
    private int blinkAlphaID;
    private int pivoScaleID;
    private bool isPivoActive = false;
    
    void Start()
    {
        blinkAlphaID = Shader.PropertyToID(blinkAlphaParam);
        pivoScaleID = Shader.PropertyToID(pivoScaleParam);
        
        if (blinkMaterial != null)
            blinkMaterial.SetFloat(blinkAlphaID, 0f);
        
        if (pivoMaterial != null)
            pivoMaterial.SetFloat(pivoScaleID, 0f);
        
        // Выключаем Pivo Feature при старте
        SetPivoFeatureActive(false);
        
        Debug.Log("✅ URPFullScreenController запущен");
    }
    
    public void SetBlinkAlpha(float value)
    {
        if (blinkMaterial != null)
            blinkMaterial.SetFloat(blinkAlphaID, Mathf.Clamp01(value));
    }
    
    public void SetPivoScale(float value)
    {
        if (pivoMaterial != null)
        {
            pivoMaterial.SetFloat(pivoScaleID, value);
            
            // Включаем/выключаем Feature в зависимости от значения
            bool shouldBeActive = value > 0.01f;
            if (shouldBeActive != isPivoActive)
            {
                SetPivoFeatureActive(shouldBeActive);
            }
        }
    }
    
    private void SetPivoFeatureActive(bool active)
    {
        if (pivoRendererFeature != null)
        {
            pivoRendererFeature.SetActive(active);
            isPivoActive = active;
            Debug.Log($"🍺 Pivo Feature {(active ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН")}");
        }
    }
}