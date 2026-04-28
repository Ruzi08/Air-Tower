using UnityEngine;

public class CameraHeadBob : MonoBehaviour
{
    [Header("Настройки покачивания")]
    [SerializeField] private float walkingBobbingSpeed = 14f;
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float runningBobbingSpeed = 18f;
    [SerializeField] private float runningBobbingAmount = 0.08f;
    
    [Header("Настройки движения")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float runningSpeed = 7f;
    
    private float defaultPosY = 0f;
    private float timer = 0f;
    private float currentBobbingSpeed;
    private float currentBobbingAmount;
    
    void Start()
    {
        // Запоминаем позицию камеры при старте
        defaultPosY = transform.localPosition.y;
        Debug.Log($"Стартовая высота камеры: {defaultPosY}");
    }
    
    // Убрали OnEnable и OnDisable чтобы не сбрасывать позицию
    
    void Update()
    {
        if (!characterController.isGrounded) return;
        
        bool isMoving = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
        
        if (isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && characterController.velocity.magnitude > runningSpeed;
            
            if (isRunning)
            {
                currentBobbingSpeed = runningBobbingSpeed;
                currentBobbingAmount = runningBobbingAmount;
            }
            else
            {
                currentBobbingSpeed = walkingBobbingSpeed;
                currentBobbingAmount = bobbingAmount;
            }
            
            timer += Time.deltaTime * currentBobbingSpeed;
            float newY = defaultPosY + Mathf.Sin(timer) * currentBobbingAmount;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            timer = 0;
            Vector3 newPos = transform.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, defaultPosY, Time.deltaTime * 10f);
            transform.localPosition = newPos;
        }
    }
    
    public void ResetToOriginalPosition()
    {
        Vector3 resetPos = transform.localPosition;
        resetPos.y = defaultPosY;
        transform.localPosition = resetPos;
        timer = 0;
    }
}