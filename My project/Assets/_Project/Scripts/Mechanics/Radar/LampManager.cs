using UnityEngine;

public class LampManager : MonoBehaviour
{
    [Header("Lamps")]
    [SerializeField] private LampController greenLamp;
    [SerializeField] private LampController yellowLamp;
    [SerializeField] private LampController redLamp;

    [Header("Radar")]
    [SerializeField] private RadarManager radarManager;

    [Header("Blink Settings")]
    [SerializeField] private float yellowBlinkInterval = 0.5f;  // Мигание жёлтой лампы
    [SerializeField] private float redBlinkInterval = 0.25f;     // Мигание красной лампы

    private float yellowTimer = 0f;
    private float redTimer = 0f;
    private bool yellowBlinkState = false;
    private bool redBlinkState = false;

    void Start()
    {
        if (radarManager == null)
            radarManager = FindObjectOfType<RadarManager>();

        TurnOffAllLamps();
    }

    void Update()
    {
        if (radarManager == null) return;

        // Получаем статус опасности
        bool hasWarning = radarManager.HasCollisionWarning();
        bool hasCritical = radarManager.HasCriticalCollision();

        if (hasCritical)
        {
            // Красная мигает, остальные выключены
            greenLamp.TurnOff();
            yellowLamp.TurnOff();
            BlinkRedLamp();
        }
        else if (hasWarning)
        {
            // Жёлтая мигает, красная выключена
            greenLamp.TurnOff();
            redLamp.TurnOff();
            BlinkYellowLamp();
        }
        else
        {
            // Только зелёная горит
            redLamp.TurnOff();
            yellowLamp.TurnOff();
            greenLamp.TurnOn();
        }
    }

    private void BlinkYellowLamp()
    {
        yellowTimer += Time.deltaTime;
        if (yellowTimer >= yellowBlinkInterval)
        {
            yellowTimer = 0f;
            yellowBlinkState = !yellowBlinkState;
            yellowLamp.SetState(yellowBlinkState);
        }
    }

    private void BlinkRedLamp()
    {
        redTimer += Time.deltaTime;
        if (redTimer >= redBlinkInterval)
        {
            redTimer = 0f;
            redBlinkState = !redBlinkState;
            redLamp.SetState(redBlinkState);
        }
    }

    private void TurnOffAllLamps()
    {
        greenLamp?.TurnOff();
        yellowLamp?.TurnOff();
        redLamp?.TurnOff();
    }
}