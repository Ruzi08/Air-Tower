using UnityEngine;

public class LidOpener : MonoBehaviour, Interactable
{
    [Header("Настройки открывания")]
    public Vector3 openRotation = new Vector3(0, -90, 0);
    public float openSpeed = 180f;
    
    [Header("Ссылки")]
    public BreakerPanel panel;
    public CameraHeadBob cameraHeadBob; // Перетащи сюда камеру с компонентом
    
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool isAnimating = false;
    
    void Start()
    {
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
        
        if (cameraHeadBob == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cameraHeadBob = cam.GetComponent<CameraHeadBob>();
        }
    }
    
    void Update()
    {
        if (isAnimating)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, 
                targetRotation, 
                openSpeed * Time.deltaTime
            );
            
            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
                
                if (panel != null)
                {
                    if (isOpen)
                        panel.OnLidOpened();
                    else
                        panel.OnLidClosed();
                }
            }
        }
    }
    
    public void Interact()
    {
        if (isAnimating) return;
        
        isOpen = !isOpen;
        
        if (cameraHeadBob != null)
        {
            // Просто включаем/выключаем компонент
            cameraHeadBob.enabled = !isOpen;
            
            // Сбрасываем позицию камеры
            if (isOpen)
            {
                cameraHeadBob.ResetToOriginalPosition();
            }
        }
        
        if (isOpen)
            targetRotation = closedRotation * Quaternion.Euler(openRotation);
        else
            targetRotation = closedRotation;
        
        isAnimating = true;
    }
    
    public bool IsOpen()
    {
        return isOpen;
    }
    
    public string GetDescription()
    {
        return isOpen ? "Нажмите, чтобы закрыть крышку" : "Нажмите, чтобы открыть крышку";
    }
}