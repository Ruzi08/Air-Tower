using UnityEngine;
using System.Collections;
using TMPro;

public class NumberRegulator : MonoBehaviour, Interactable
{
    [Header("Display")]
    [SerializeField] private TextMeshPro displayText;

    [Header("Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 49;
    [SerializeField] private int currentValue = 0;
    [SerializeField] private float mouseWheelSensitivity = 1f;
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float maxInteractDistance = 2.5f;
    [SerializeField] private string format = "D2";

    [Header("Audio")]
    [SerializeField] private AudioClip dialSound;
    private AudioSource audioSource;
    private SoundRotateButton SoundRotateButton;

    [Header("Visual")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material hoverMaterial;
    private Renderer dialRenderer;

    [Header("Rotation")]
    [SerializeField] private Transform dialTransform;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private bool invertRotation = false;

    [Header("Camera Control")]
    [SerializeField] private MonoBehaviour cameraController;
    [SerializeField] private Transform distanceCheckTarget;

    public System.Action<int> OnValueChanged;
    public int CurrentValue => currentValue;

    private bool isDragging = false;
    private float lastMouseX;
    private float dragAccumulator;
    private bool wasCursorVisible;
    private CursorLockMode wasCursorLocked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundRotateButton = GetComponent<SoundRotateButton>();
        
        // Проверяем наличие компонента
        if (SoundRotateButton == null)
        {
            Debug.LogWarning("SoundRotateButton component not found on the same GameObject. Please add it for random sound playback.", this);
        }
        audioSource = GetComponent<AudioSource>();
        dialRenderer = GetComponent<Renderer>();

        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        UpdateDisplay();
        UpdateDialRotation();
    }

    private void Update()
    {
        if (!isDragging && dialRenderer != null && defaultMaterial != null && IsOutOfRange() && dialRenderer.material == hoverMaterial)
        {
            dialRenderer.material = defaultMaterial;
        }
    }

    public void Interact()
    {
        StartCoroutine(DragRoutine());
    }

    public string GetDescription()
    {
        return isDragging ? "Отпустите LMB чтобы закончить" : "Крутить регулятор [LMB]";
    }

    private IEnumerator DragRoutine()
    {
        isDragging = true;

        wasCursorVisible = Cursor.visible;
        wasCursorLocked = Cursor.lockState;

        if (cameraController != null)
            cameraController.enabled = false;

        Cursor.lockState = CursorLockMode.Confined;

        lastMouseX = Input.mousePosition.x;

        // Визуальная обратная связь
        if (dialRenderer != null && activeMaterial != null)
        {
            dialRenderer.material = activeMaterial;
        }

        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        dragAccumulator = 0f;


        while (Input.GetMouseButton(0))
        {
            if (IsOutOfRange())
            {
                break;
            }

            float currentMouseX = Input.mousePosition.x;
            float deltaX = currentMouseX - lastMouseX;

            if (Mathf.Abs(deltaX) > 0.1f)
            {
                dragAccumulator += deltaX * Mathf.Max(0.01f, mouseSensitivity);
                int rawChange = (int)dragAccumulator;
                int change = rawChange;

                if (invertRotation)
                    change = -change;

                if (change != 0)
                {
                    dragAccumulator -= rawChange;
                    int newValue = currentValue + change;

                    // Зацикливание
                    newValue = WrapValue(newValue);

                    if (newValue != currentValue)
                    {
                        currentValue = newValue;
                        UpdateDisplay();
                        UpdateDialRotation();
                        OnValueChanged?.Invoke(currentValue);

                        if (audioSource != null && dialSound != null)
                        {
                            audioSource.PlayOneShot(dialSound);
                        }

                        Debug.Log($"Значение: {currentValue}");
                    }


                }

                lastMouseX = currentMouseX;
            }

            yield return null;
        }


        // Заканчиваем перетаскивание
        StopDragging();
    }
    
    private void PlayRandomDialSound()
    {
        if (SoundRotateButton != null)
        {
            // Проигрываем случайный звук из списка
            SoundRotateButton.PlayRandomSound();
        }
        else if (audioSource != null && dialSound != null)
        {
            audioSource.PlayOneShot(dialSound);
        }
    }

    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = currentValue.ToString(format);
        }
    }

    private int WrapValue(int value)
    {
        int range = maxValue - minValue + 1;
        if (range <= 0)
        {
            return minValue;
        }

        return minValue + (((value - minValue) % range) + range) % range;
    }

    private bool IsOutOfRange()
    {
        Transform target = GetDistanceCheckTarget();
        if (target == null)
        {
            return false;
        }

        return Vector3.Distance(target.position, transform.position) > maxInteractDistance;
    }

    private Transform GetDistanceCheckTarget()
    {
        if (distanceCheckTarget != null)
        {
            return distanceCheckTarget;
        }

        if (cameraController != null)
        {
            return cameraController.transform;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return null;
    }

    private void StopDragging()
    {
        isDragging = false;

        if (cameraController != null)
            cameraController.enabled = true;

        Cursor.visible = wasCursorVisible;
        Cursor.lockState = wasCursorLocked;

        if (dialRenderer != null && defaultMaterial != null)
        {
            dialRenderer.material = defaultMaterial;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateDialRotation()
    {
        if (dialTransform != null)
        {
            float angle = (float)currentValue / maxValue * 360f;
            dialTransform.localRotation = Quaternion.Euler(rotationAxis * angle);
        }
    }

    private void OnMouseEnter()
    {
        if (!isDragging && !IsOutOfRange() && dialRenderer != null && hoverMaterial != null)
        {
            dialRenderer.material = hoverMaterial;
        }
    }
    private void OnMouseExit()
    {
        if (!isDragging && dialRenderer != null && defaultMaterial != null)
        {
            dialRenderer.material = defaultMaterial;
        }
    }
}
