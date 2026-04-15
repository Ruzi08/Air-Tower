using UnityEngine;

// Обрати внимание: мы добавляем , IInteractable после MonoBehaviour
public class NO : MonoBehaviour, Interactable
{
    public Light targetLight;
    private bool isLightOn = false;

    // Обязательный метод из нашего интерфейса
    public void Interact()
    {
        isLightOn = !isLightOn;
        
        if (targetLight != null)
        {
            targetLight.enabled = isLightOn;
        }
        
        Debug.Log("Игрок щелкнул выключателем!");
    }

    public string GetDescription()
    {
        return "Переключить свет";
    }
}