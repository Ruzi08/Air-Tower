using UnityEngine;

public class RadarScreenInteractable : MonoBehaviour, Interactable
{
    [Header("Cameras")]
    [SerializeField] private Camera radarCamera;
    [SerializeField] private Camera playerCamera;

    [Header("Components")]
    [SerializeField] private Canvas radarCanvas;
    [SerializeField] private RadarManager radarManager;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private MonoBehaviour cameraController;

    [Header("Settings")]
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;
    [SerializeField] private float transitionTime = 0f;

    private bool isUsingRadar = false;

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
    }

    public void Interact()
    {
        if (!isUsingRadar)
            ActivateRadar();
    }

    public string GetDescription()
    {
        return "Использовать радар [E]";
    }

    private void ActivateRadar()
    {
        isUsingRadar = true;

        // Скрываем точку в центре экрана
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
    }

    private void DeactivateRadar()
    {
        // Показываем точку обратно
        if (CrosshairController.Instance != null)
            CrosshairController.Instance.Show();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

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
        if (isUsingRadar && Input.GetKeyDown(exitKey))
        {
            DeactivateRadar();
        }
    }
}