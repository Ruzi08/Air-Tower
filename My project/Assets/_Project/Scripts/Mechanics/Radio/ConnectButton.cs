using UnityEngine;
using System.Collections;

public class ConnectButton : MonoBehaviour, Interactable
{
    [Header("References")]
    [SerializeField] private RadioController radioController;

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
        if (!isPressed && buttonRenderer != null && hoverMaterial != null && hasPower)
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
}