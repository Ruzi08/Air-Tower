using UnityEngine;

public class LidOpener : MonoBehaviour, Interactable
{
    [Header("Настройки открывания")]
    public Vector3 openRotation = new Vector3(0, -90, 0);
    public float openSpeed = 180f;
    
    [Header("Ссылки")]
    public BreakerPanel panel;
    
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool isAnimating = false;
    
    void Start()
    {
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
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