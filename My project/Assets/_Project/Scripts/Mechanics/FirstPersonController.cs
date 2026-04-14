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
    
    // Флаги блокировки
    private bool movementLocked = false;
    private bool cameraLocked = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!cameraLocked)
            HandleLook();
            
        if (!movementLocked)
            HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    // --- Публичные методы для блокировки из других скриптов ---
    
    public void LockMovement()
    {
        movementLocked = true;
    }
    
    public void UnlockMovement()
    {
        movementLocked = false;
    }
    
    public void LockCamera()
    {
        cameraLocked = true;
    }
    
    public void UnlockCamera()
    {
        cameraLocked = false;
    }
    
    public void LockAll()
    {
        movementLocked = true;
        cameraLocked = true;
    }
    
    public void UnlockAll()
    {
        movementLocked = false;
        cameraLocked = false;
    }
    
    public bool IsMovementLocked() => movementLocked;
    public bool IsCameraLocked() => cameraLocked;
}