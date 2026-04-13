using UnityEngine;

public class BlinkCameraEffect : MonoBehaviour
{
    [Header("Материал fullscreen эффекта")]
    public Material blinkMaterial;

    private int alphaID;

    void Awake()
    {
        alphaID = Shader.PropertyToID("_Alpha");

        if (blinkMaterial == null)
        {
            Debug.LogError("❌ Не назначен blinkMaterial!");
        }
    }

    public void SetAlpha(float value)
    {
        if (blinkMaterial != null)
        {
            blinkMaterial.SetFloat(alphaID, Mathf.Clamp01(value));
        }
    }
}