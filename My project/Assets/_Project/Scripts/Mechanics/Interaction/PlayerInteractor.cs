using UnityEngine;
using UnityEngine.EventSystems;  // ← ДОБАВИТЬ

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
        crosshair = FindObjectOfType<CrosshairController>();
        
        if (playerCamera == null)
            Debug.LogError("❌ Нет камеры на игроке!");
    }
    
    void Update()
    {
        // Если диалог активен — не взаимодействуем с 3D
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            if (crosshair != null)
                crosshair.SetCrosshairNormal();
            currentInteractable = null;
            return;
        }
        
        CheckLookAt();
        
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
    
    private void CheckLookAt()
    {
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                if (crosshair != null)
                    crosshair.SetCrosshairHighlight();
                    
                currentInteractable = interactable;
                return;
            }
        }
        
        if (crosshair != null)
            crosshair.SetCrosshairNormal();
            
        currentInteractable = null;
    }
    
    private void TryInteract()
    {
        // Если курсор над UI — НЕ ВЗАИМОДЕЙСТВУЕМ с 3D
        if (IsPointerOverUI())
        {
            Debug.Log("🖱️ Курсор над UI, игнорирую 3D объекты");
            return;
        }
        
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            Debug.Log($"🎯 Попал в: {hit.collider.gameObject.name}");
            
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                Debug.Log($"✅ Вызываю Interact() на {hit.collider.gameObject.name}");
                interactable.Interact();
            }
        }
    }
}