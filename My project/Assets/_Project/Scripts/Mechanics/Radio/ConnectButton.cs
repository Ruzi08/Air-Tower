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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.localPosition;

        if (defaultMaterial == null && buttonRenderer != null)
            defaultMaterial = buttonRenderer.material;

        if (indicatorLight != null)
            indicatorLight.enabled = false;
    }

    public void Interact()
    {
        if (isPressed) return;

        if (radioController != null)
        {
            radioController.TryConnect();
            StartCoroutine(PressAnimation());
        }
        else
        {
            Debug.LogWarning($"У кнопки {gameObject.name} не назначен radioController!");
        }
    }

    public string GetDescription()
    {
        return "Установить связь [LMB]";
    }

    private IEnumerator PressAnimation()
    {
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
        if (indicatorLight != null)
            StartCoroutine(FlashLight(successColor));
    }

    public void FlashError()
    {
        if (indicatorLight != null)
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
        if (!isPressed && buttonRenderer != null && hoverMaterial != null)
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
