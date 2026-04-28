using UnityEngine;
using System.Collections;

public class RadioButton : MonoBehaviour, Interactable
{
    [Header("Button Settings")]
    [SerializeField] private bool isUpButton = true;
    [SerializeField] private LetterSelector parentSelector;
    [SerializeField] private float maxInteractDistance = 2.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float pressDepth = 0.0001f;
    

    [Header("Distance Check")]
    [SerializeField] private MonoBehaviour cameraController;
    [SerializeField] private Transform distanceCheckTarget;

    private Renderer buttonRenderer;
    private Vector3 originalPosition;
    private bool isPressed = false;
    private bool hasPower = true;
    private RadioSound radio;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        radio = GetComponentInParent<RadioSound>();
        originalPosition = transform.localPosition;

        if (defaultMaterial == null && buttonRenderer != null)
            defaultMaterial = buttonRenderer.material;

        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut += OnPowerOut;
            PowerManager.Instance.OnPowerRestored += OnPowerRestored;
        }
    }

    private void Update()
    {
        if (!isPressed && buttonRenderer != null && defaultMaterial != null && IsOutOfRange() && buttonRenderer.material == hoverMaterial)
        {
            buttonRenderer.material = defaultMaterial;
        }
    }

    void OnDestroy()
    {
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerOut -= OnPowerOut;
            PowerManager.Instance.OnPowerRestored -= OnPowerRestored;
        }
    }

    private void OnPowerOut()
    {
        hasPower = false;
        Debug.Log($"RadioButton {gameObject.name}: электричество отключено");
    }

    private void OnPowerRestored()
    {
        hasPower = true;
        Debug.Log($"RadioButton {gameObject.name}: электричество включено");
    }

    public void Interact()
    {
        if (IsOutOfRange())
        {
            return;
        }

        // ✅ Анимация и звук ВСЕГДА работают
        StartCoroutine(PressAnimation());

        // ✅ Меняем букву ТОЛЬКО если есть свет
        if (hasPower && parentSelector != null)
        {
            if (isUpButton)
                parentSelector.NextLetter();
            else
                parentSelector.PreviousLetter();
        }
        else if (!hasPower)
        {
            Debug.Log("Нет электричества! Буква не меняется");
        }
    }

    public string GetDescription()
    {
        if (!hasPower) return "🔌 Нет электричества...";
        return isUpButton ? "Следующая буква [LMB]" : "Предыдущая буква [LMB]";
    }

    private IEnumerator PressAnimation()
    {
        if (isPressed) yield break;
        isPressed = true;

        //if (buttonRenderer != null && pressedMaterial != null)
        //{
        //    buttonRenderer.material = pressedMaterial;
        //}

        Vector3 startWorldPos = transform.position;

        Vector3 pressDirection = -transform.forward;

        Debug.Log($"Начальная позиция (world): {startWorldPos}");
        Debug.Log($"Направление нажатия: {pressDirection}");

        // Двигаем в world space
        transform.position = startWorldPos + pressDirection * pressDepth;

        Debug.Log($"Позиция при нажатии (world): {transform.position}");
        
        radio.Click();

        yield return new WaitForSeconds(0.1f);

        //if (buttonRenderer != null && defaultMaterial != null)
        //{
        //    buttonRenderer.material = defaultMaterial;
        //}

        transform.position = startWorldPos;
        isPressed = false;
    }

    private void OnMouseEnter()
    {
        if (!isPressed && !IsOutOfRange() && buttonRenderer != null && hoverMaterial != null && hasPower)
        {
            buttonRenderer.material = hoverMaterial;
        }
    }
    
    private void OnMouseExit()
    {
        if (!isPressed && buttonRenderer != null && defaultMaterial != null)
        {
            buttonRenderer.material = defaultMaterial;
        }
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
}
