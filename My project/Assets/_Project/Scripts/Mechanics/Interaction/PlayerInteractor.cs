using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    
    private Camera playerCamera;
    private CrosshairController crosshair;
    private Interactable currentInteractable;
    
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // Находим прицел на Canvas
        crosshair = FindObjectOfType<CrosshairController>();
    }
    
    void Update()
    {
        // Проверяем, на что смотрим
        CheckLookAt();
        
        // Клик ЛКМ
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    private void CheckLookAt()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                // Смотрим на интерактивный объект
                if (crosshair != null)
                    crosshair.SetCrosshairHighlight();
                    
                currentInteractable = interactable;
                return;
            }
        }
        
        // Не смотрим ни на что интерактивное
        if (crosshair != null)
            crosshair.SetCrosshairNormal();
            
        currentInteractable = null;
    }
    
    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}