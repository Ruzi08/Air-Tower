using UnityEngine;

public class LetterDisplay : MonoBehaviour
{
    void Start()
    {
        if (PowerManager.Instance != null)
        {
            Debug.Log($"✅ {gameObject.name} подписался на PowerManager");
            PowerManager.Instance.OnPowerOut += OnPowerOut;
            PowerManager.Instance.OnPowerRestored += OnPowerRestored;
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name}: PowerManager.Instance = null!");
        }
    }
    
    void OnDestroy()
    {
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= OnPowerOut;
            PowerManager.Instance.OnPowerRestored -= OnPowerRestored;
        }
    }
    
    private void OnPowerOut()
    {
        Debug.Log($"🔌 {gameObject.name}: выключаю");
        gameObject.SetActive(false);  // ← Скрываем ВЕСЬ объект
    }
    
    private void OnPowerRestored()
    {
        Debug.Log($"⚡ {gameObject.name}: включаю");
        gameObject.SetActive(true);   // ← Показываем обратно
    }
}