using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;
    public LayerMask obstacleLayer; // 🔥 Добавь слой для стен и препятствий
    
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
                // 🔥 Проверяем, есть ли препятствие между камерой и объектом
                if (IsObstacleBetween(playerCamera.transform.position, hit.point))
                {
                    if (crosshair != null)
                        crosshair.SetCrosshairNormal();
                    currentInteractable = null;
                    return;
                }
                
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
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // 🔥 Проверяем, есть ли препятствие между камерой и объектом
            if (IsObstacleBetween(ray.origin, hit.point))
            {
                Debug.Log($"🚫 Не могу взаимодействовать: препятствие на пути к {hit.collider.gameObject.name}");
                return;
            }
            
            Debug.Log($"🎯 Попал в: {hit.collider.gameObject.name}");
            
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                Debug.Log($"✅ Вызываю Interact() на {hit.collider.gameObject.name}");
                interactable.Interact();
            }
            else
            {
                Debug.Log($"❌ Нет компонента Interactable на {hit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("❌ Луч никуда не попал");
        }
    }
    
    // 🔥 Метод проверки препятствий
    private bool IsObstacleBetween(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        
        // Если obstacleLayer не задан, используем всё кроме interactableLayer
        LayerMask mask = obstacleLayer.value != 0 ? obstacleLayer : ~interactableLayer;
        
        RaycastHit obstacleHit;
        if (Physics.Raycast(start, direction, out obstacleHit, distance, mask))
        {
            // Если препятствие не является интерактивным объектом
            if (obstacleHit.collider.GetComponent<Interactable>() == null)
            {
                Debug.Log($"🚫 Препятствие: {obstacleHit.collider.name}");
                return true;
            }
        }
        
        return false;
    }
}