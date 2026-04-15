using UnityEngine;
using System.Collections;

public class NumberRegulator : MonoBehaviour, Interactable
{
    [Header("Display")]
    [SerializeField] private TextMesh displayText;

    [Header("Settings")]
    [SerializeField] private int minValue = 0;
    [SerializeField] private int maxValue = 99;
    [SerializeField] private int currentValue = 0;
    [SerializeField] private float mouseWheelSensitivity = 1f;
    [SerializeField] private string format = "D2";

    [Header("Audio")]
    [SerializeField] private AudioClip dialSound;
    private AudioSource audioSource;

    [Header("Visual")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material hoverMaterial;
    private Renderer dialRenderer;

    [Header("Rotation (опционально)")]
    [SerializeField] private Transform dialTransform;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private bool invertRotation = false;

    public System.Action<int> OnValueChanged;
    public int CurrentValue => currentValue;

    private bool isDragging = false;
    private float lastMouseY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        dialRenderer = GetComponent<Renderer>();

        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        UpdateDisplay();
        UpdateDialRotation();
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
        lastMouseY = Input.mousePosition.y;

        // Визуальная обратная связь
        if (dialRenderer != null && activeMaterial != null)
        {
            dialRenderer.material = activeMaterial;
        }

        // Показываем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;


        // Ждем пока игрок не отпустит E
        while (Input.GetKey(KeyCode.Mouse0))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.001f)
            {
                int change = Mathf.RoundToInt(scroll * mouseWheelSensitivity * 10f);

                if (change != 0)
                {
                    int newValue = currentValue + change;

                    // Зацикливание или ограничение
                    if (newValue > maxValue)
                        newValue = minValue;
                    else if (newValue < minValue)
                        newValue = maxValue;

                    if (newValue != currentValue)
                    {
                        currentValue = newValue;
                        UpdateDisplay();
                        UpdateDialRotation();
                        OnValueChanged?.Invoke(currentValue);

                        // Звук
                        if (audioSource != null && dialSound != null)
                        {
                            audioSource.PlayOneShot(dialSound);
                        }

                        Debug.Log($"Значение изменено на: {currentValue}");
                    }
                }
            }

            yield return null;
        }


        // Заканчиваем перетаскивание
        isDragging = false;

        if (dialRenderer != null && defaultMaterial != null)
        {
            dialRenderer.material = defaultMaterial;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = currentValue.ToString(format);
        }
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
        if (!isDragging && dialRenderer != null && hoverMaterial != null)
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
