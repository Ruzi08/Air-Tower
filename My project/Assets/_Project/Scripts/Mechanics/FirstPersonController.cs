using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float walkSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Настройки камеры")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Блокируем курсор мыши в центре экрана и делаем его невидимым
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Вычисляем поворот вверх/вниз и ограничиваем его, чтобы не "сломать шею"
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Применяем вращение вверх/вниз только к камере
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Вращаем влево/вправо само тело игрока (цилиндр)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        // Получаем нажатия кнопок WASD
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Вычисляем вектор движения относительно того, куда смотрит игрок
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // Обработка гравитации (чтобы игрок падал, если выйдет за край)
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшой прижим к земле
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}