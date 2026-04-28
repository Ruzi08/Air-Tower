using UnityEngine;
using System.Collections;

public class ConnectButton : MonoBehaviour, Interactable
{
    [Header("References")]
    [SerializeField] private RadioController radioController;
    [SerializeField] private float maxInteractDistance = 2.5f;

    [Header("Visual")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float pressDepth = 0.01f;

    [Header("Indicator Light")]
    [SerializeField] private Light indicatorLight;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color errorColor = Color.red;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    [Header("Distance Check")]
    [SerializeField] private MonoBehaviour cameraController;
    [SerializeField] private Transform distanceCheckTarget;

    private Renderer buttonRenderer;
    private Vector3 originalPosition;
    private bool isPressed = false;
    private bool hasPower = true;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.localPosition;

        if (defaultMaterial == null && buttonRenderer != null)
            defaultMaterial = buttonRenderer.material;

        if (indicatorLight != null)
            indicatorLight.enabled = false;

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
        if (indicatorLight != null)
            indicatorLight.enabled = false;
        Debug.Log("ConnectButton: электричество отключено");
    }

    private void OnPowerRestored()
    {
        hasPower = true;
        Debug.Log("ConnectButton: электричество включено");
    }

    public void Interact()
    {
        if (IsOutOfRange())
        {
            return;
        }

        // ✅ Анимация и звук ВСЕГДА
        StartCoroutine(PressAnimation());

        // ✅ Логика ТОЛЬКО если есть свет
        if (hasPower && radioController != null)
        {
            radioController.TryConnect();
        }
        else if (!hasPower)
        {
            Debug.Log("Нет электричества! Связь не установить");
        }
    }

    public string GetDescription()
    {
        if (!hasPower) return "🔌 Нет электричества...";
        return "Установить связь [LMB]";
    }

    private IEnumerator PressAnimation()
    {
        if (isPressed) yield break;
        isPressed = true;

        if (buttonRenderer != null && pressedMaterial != null)
        {
            buttonRenderer.material = pressedMaterial;
        }

        transform.localPosition = originalPosition + Vector3.down * pressDepth;

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        yield return new WaitForSeconds(0.2f);

        if (buttonRenderer != null && defaultMaterial != null)
        {
            buttonRenderer.material = defaultMaterial;
        }

        transform.localPosition = originalPosition;
        isPressed = false;
    }

    public void FlashSuccess()
    {
        if (indicatorLight != null && hasPower)
            StartCoroutine(FlashLight(successColor));
    }

    public void FlashError()
    {
        if (indicatorLight != null && hasPower)
            StartCoroutine(FlashLight(errorColor));
    }

    private IEnumerator FlashLight(Color color)
    {
        indicatorLight.enabled = true;
        indicatorLight.color = color;
        yield return new WaitForSeconds(0.5f);
        indicatorLight.enabled = false;
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
