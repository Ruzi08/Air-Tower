using Unity.VisualScripting;
using UnityEngine;

public class LampSound : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private Sound yellowSource;
    [SerializeField] private Sound redSource;
    [SerializeField] private Sound greenSource; // Отдельный для зелёного

    void Start()
    {
        // Создаём источники, если не назначены
        if (yellowSource == null)
        {
            yellowSource = gameObject.AddComponent<Sound>();
        }
        if (redSource == null)
        {
            redSource = gameObject.AddComponent<Sound>();
        }
        if (greenSource == null)
        {
            greenSource = gameObject.AddComponent<Sound>();
        }
    }

    public void PlayYellowLoop()
    {
        if (yellowSource != null)
        {
            yellowSource.PlaySnd();
        }
    }

    public void StopYellow()
    {
        if (yellowSource != null)
            yellowSource.StopSnd();
    }

    public void PlayRedLoop()
    {
        if (redSource != null)
        {
            redSource.PlaySnd();
        }
    }

    public void StopRed()
    {
        if (redSource != null)
            redSource.StopSnd();
    }

    public void PlayGreenOneShot()
    {
        if (greenSource != null)
        {
            greenSource.PlaySnd();
        }
    }

    public void StopGreen()
    {
        if (greenSource != null)
            greenSource.StopSnd();
    }
}