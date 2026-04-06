using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    public float interactionDistance = 3f; // Как далеко игрок может дотянуться
    public LayerMask interactableLayer;    // Слой, на котором лежат интерактивные предметы

    private Camera playerCamera;

    void Start()
    {
        // Автоматически находим камеру на этом объекте или его детях
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Проверяем нажатие Левой Кнопки Мыши (К.Д.)
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        // Создаем луч из центра камеры вперед
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Пускаем луч. Если он во что-то попал на нужной дистанции и на нужном слое...
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // ...пытаемся найти на объекте наш интерфейс
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            
            if (interactable != null)
            {
                // Если интерфейс есть, вызываем взаимодействие!
                interactable.Interact();
            }
        }
    }
}