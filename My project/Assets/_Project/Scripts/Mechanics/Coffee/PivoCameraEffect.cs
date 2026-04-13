using UnityEngine;

public class PivoCameraEffect : MonoBehaviour
{
    public Material pivoMaterial;
    
    private int intensityID;
    private float currentIntensity = 0f;
    
    void Awake()
    {
        intensityID = Shader.PropertyToID("_Intensity");
        
        if (pivoMaterial == null)
        {
            Debug.LogError("❌ PivoMaterial не назначен!");
        }
    }
    
    public void SetIntensity(float value)
    {
        currentIntensity = Mathf.Clamp01(value);
        if (pivoMaterial != null)
        {
            pivoMaterial.SetFloat(intensityID, currentIntensity);
        }
        
        Debug.Log($"🍺 Pivo Intensity установлен: {currentIntensity}");
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // ПРИМЕНЯЕМ ЭФФЕКТ ТОЛЬКО ЕСЛИ ЕСТЬ ИНТЕНСИВНОСТЬ
        if (pivoMaterial != null && currentIntensity > 0.01f)
        {
            Graphics.Blit(src, dest, pivoMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}