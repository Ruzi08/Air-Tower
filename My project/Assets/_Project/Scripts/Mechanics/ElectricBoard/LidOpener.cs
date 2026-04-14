using UnityEngine;

public class LidOpener : MonoBehaviour, Interactable
{
    [Header("Настройки открывания")]
    public Vector3 openRotation = new Vector3(0, -90, 0);  // Поворот при открытии
    public float openSpeed = 180f;  // Скорость вращения (градусов в секунду)
    
    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private bool isOpen = false;
    private bool isAnimating = false;
    
    void Start()
    {
        // Запоминаем начальное положение (закрыто)
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
    }
    
    void Update()
    {
        if (isAnimating)
        {
            // Плавно поворачиваем к цели
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, 
                targetRotation, 
                openSpeed * Time.deltaTime
            );
            
            // Если дошли до цели — анимация закончена
            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
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
    
    public string GetDescription()
    {
        return isOpen ? "Нажмите, чтобы закрыть крышку" : "Нажмите, чтобы открыть крышку";
    }
}