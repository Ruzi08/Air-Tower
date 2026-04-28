using UnityEngine;

public class RadarScreenInteractable : MonoBehaviour, Interactable
{
    [Header("Cameras")]
    [SerializeField] private Camera radarCamera;
    [SerializeField] private Camera playerCamera;

    [Header("Components")]
    [SerializeField] private Canvas radarCanvas;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private MonoBehaviour cameraController;

    [Header("Settings")]
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;

    private bool isUsingRadar = false;
    
    // ✅ Флаг электричества
    private bool hasPower = true;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (playerController == null)
            playerController = FindObjectOfType<PlayerInteractor>();

        if (radarCamera != null)
            radarCamera.gameObject.SetActive(false);

        if (radarCanvas != null && radarCamera != null)
            radarCanvas.worldCamera = radarCamera;

        // ✅ Подписываемся на события электричества
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += OnPowerOut;
            PowerManager.Instance.OnPowerRestored += OnPowerRestored;
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
        hasPower = false;
        
        // Если мы в радаре — выходим из него
        if (isUsingRadar)
        {
            DeactivateRadar();
        }
        
        // Отключаем Canvas визуально
        if (radarCanvas != null)
            radarCanvas.enabled = false;
        
        Debug.Log("Радар: электричество отключено");
    }

    private void OnPowerRestored()
    {
        hasPower = true;
        
        // Включаем Canvas обратно
        if (radarCanvas != null)
            radarCanvas.enabled = true;
        
        Debug.Log("Радар: электричество включено");
    }

    public void Interact()
    {
        // ✅ Без электричества — не включаем радар
        if (!hasPower)
        {
            Debug.Log("Нет электричества! Радар не работает.");
            return;
        }

        if (!isUsingRadar)
            ActivateRadar();
    }

    public string GetDescription()
    {
        if (!hasPower) return "🔌 Нет электричества...";
        return "Использовать радар [LMB]";
    }

    private void ActivateRadar()
    {
        isUsingRadar = true;

        if (CrosshairController.Instance != null)
            CrosshairController.Instance.Hide();

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);
        if (radarCamera != null)
            radarCamera.gameObject.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;
        if (cameraController != null)
            cameraController.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (radarCanvas != null)
        {
            radarCanvas.worldCamera = radarCamera;
            radarCanvas.renderMode = RenderMode.WorldSpace;

            Debug.Log($"Canvas worldCamera: {radarCanvas.worldCamera?.name}");
            Debug.Log($"RadarCamera активна: {radarCamera.gameObject.activeSelf}");
            Debug.Log($"RadarCamera позиция: {radarCamera.transform.position}");
            Debug.Log($"Canvas позиция: {radarCanvas.transform.position}");
        }
    }

    private void DeactivateRadar()
    {
        // ✅ Скрываем курсор при выходе из радара
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (CrosshairController.Instance != null)
            CrosshairController.Instance.Show();

        isUsingRadar = false;

        if (radarCamera != null)
            radarCamera.gameObject.SetActive(false);
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (playerController != null)
            playerController.enabled = true;
        if (cameraController != null)
            cameraController.enabled = true;
    }

    void Update()
    {
        // ✅ Выход по Escape ИЛИ по правой кнопке мыши
        if (isUsingRadar && (Input.GetKeyDown(exitKey) || Input.GetMouseButtonDown(1)))
        {
            DeactivateRadar();
        }
    }
}