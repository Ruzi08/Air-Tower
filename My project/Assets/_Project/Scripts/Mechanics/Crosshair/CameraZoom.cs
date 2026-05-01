using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Настройки зума")]
    public float normalFOV = 60f;
    public float zoomedFOV = 40f;
    public float zoomSpeed = 8f;
    public KeyCode zoomKey = KeyCode.Mouse1;
    
    [Header("Опционально")]
    public bool holdToZoom = true; // Зажимать или переключатель
    
    private Camera playerCamera;
    private bool isZoomed = false;
    private bool zoomToggle = false;
    
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        playerCamera.fieldOfView = normalFOV;
    }
    
    void Update()
    {
        if (holdToZoom)
        {
            if (Input.GetKeyDown(zoomKey))
                isZoomed = true;
            if (Input.GetKeyUp(zoomKey))
                isZoomed = false;
        }
        else
        {
            if (Input.GetKeyDown(zoomKey))
            {
                zoomToggle = !zoomToggle;
                isZoomed = zoomToggle;
            }
        }
        
        float targetFOV = isZoomed ? zoomedFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
    
    public void ForceUnzoom()
    {
        isZoomed = false;
        zoomToggle = false;
        playerCamera.fieldOfView = normalFOV;
    }
}