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
    [SerializeField] private float yellowBlinkInterval = 0.5f;
    [SerializeField] private float redBlinkInterval = 0.25f;

    [Header("Green Alert Settings")]
    [SerializeField] private float greenBlinkDuration = 0.3f;   // как долго горит зелёная лампа при появлении самолёта

    [Header("Sound")]
    [SerializeField] protected LampSound lampsnd;

    private float yellowTimer = 0f;
    private float redTimer = 0f;
    private bool yellowBlinkState = false;
    private bool redBlinkState = false;

    private enum AlarmMode { None, Yellow, Red }
    private AlarmMode currentMode = AlarmMode.None;

    private float greenBlinkTimer = 0f;
    private bool isGreenBlinking = false;

    void Start()
    {
        if (radarManager == null)
            radarManager = FindObjectOfType<RadarManager>();
        if (lampsnd == null)
            lampsnd = GetComponent<LampSound>();
        if (lampsnd == null)
            lampsnd = gameObject.AddComponent<LampSound>();

        TurnOffAllLamps();
        SetMode(AlarmMode.None);

        // Подписка на событие появления нового самолёта
        if (radarManager != null)
            radarManager.OnAircraftSpawned += OnAircraftSpawned;
    }

    void OnDestroy()
    {
        if (radarManager != null)
            radarManager.OnAircraftSpawned -= OnAircraftSpawned;
    }

    void Update()
    {
        if (radarManager == null) return;

        // 1. Если нет ни одного самолёта → всё выключено
        if (radarManager.GetActiveAircraftCount() == 0)
        {
            if (currentMode != AlarmMode.None)
                SetMode(AlarmMode.None);
            return;
        }

        // 2. Обработка критических и предупреждений
        bool hasWarning = radarManager.HasCollisionWarning();
        bool hasCritical = radarManager.HasCriticalCollision();

        if (hasCritical)
        {
            if (currentMode != AlarmMode.Red)
            {
                SetMode(AlarmMode.Red);
                StopGreenBlink();          // зелёный сигнал отключается
                greenLamp.TurnOff();
                yellowLamp.TurnOff();
                lampsnd.StopYellow();
                lampsnd.StopGreen();
            }
            BlinkRedLamp();
        }
        else if (hasWarning)
        {
            if (currentMode != AlarmMode.Yellow)
            {
                SetMode(AlarmMode.Yellow);
                StopGreenBlink();
                greenLamp.TurnOff();
                redLamp.TurnOff();
                lampsnd.StopGreen();
                lampsnd.StopRed();
            }
            BlinkYellowLamp();
        }
        else
        {
            // Нет угроз, но самолёты есть — ничего не горит (зелёный только по событию)
            if (currentMode != AlarmMode.None)
            {
                SetMode(AlarmMode.None);
                StopGreenBlink();
                yellowLamp.TurnOff();
                redLamp.TurnOff();
                lampsnd.StopYellow();
                lampsnd.StopRed();
                lampsnd.StopGreen();
            }
        }

        // Отдельно зелёное мигание (при появлении самолёта)
        if (isGreenBlinking)
        {
            greenBlinkTimer -= Time.deltaTime;
            if (greenBlinkTimer <= 0f)
            {
                isGreenBlinking = false;
                greenLamp.TurnOff();
                lampsnd.StopGreen();
            }
        }
    }

    private void OnAircraftSpawned()
    {
        // Защита: не играть зелёный сигнал, если уже есть предупреждение или критическая ситуация
        if (radarManager.HasCriticalCollision() || radarManager.HasCollisionWarning())
            return;

        // Зелёный аларм: звук + лампа на короткое время
        StopGreenBlink(); // если уже мигает – сбросим
        isGreenBlinking = true;
        greenBlinkTimer = greenBlinkDuration;
        greenLamp.TurnOn();
        lampsnd.PlayGreenOneShot();   // однократный звук (не цикл)
    }

    private void StopGreenBlink()
    {
        isGreenBlinking = false;
        greenBlinkTimer = 0f;
        greenLamp.TurnOff();
    }

    private void BlinkYellowLamp()
    {
        yellowTimer += Time.deltaTime;
        if (yellowTimer >= yellowBlinkInterval)
        {
            yellowTimer = 0f;
            yellowBlinkState = !yellowBlinkState;
            yellowLamp.SetState(yellowBlinkState);

            if (yellowBlinkState)
                lampsnd.PlayYellowLoop();   // лампа зажглась – запускаем циклич. звук
            else
                lampsnd.StopYellow();       // погасла – останавливаем звук
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

            if (redBlinkState)
                lampsnd.PlayRedLoop();
            else
                lampsnd.StopRed();
        }
    }

    private void SetMode(AlarmMode newMode)
    {
        currentMode = newMode;
        yellowTimer = 0f;
        redTimer = 0f;
        yellowBlinkState = false;
        redBlinkState = false;
    }

    private void TurnOffAllLamps()
    {
        greenLamp?.TurnOff();
        yellowLamp?.TurnOff();
        redLamp?.TurnOff();
    }
}