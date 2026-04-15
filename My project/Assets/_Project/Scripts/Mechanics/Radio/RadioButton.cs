using UnityEngine;
using System.Collections;

public class RadioButton : MonoBehaviour, Interactable
{
    [Header("Button Settings")]
    [SerializeField] private bool isUpButton = true; // true = вверх, false = вниз
    [SerializeField] private LetterSelector parentSelector;

    [Header("Visual Feedback")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material pressedMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float pressDepth = 0.01f;

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
    }

    public void Interact()
    {
        if (isPressed) return;

        if (parentSelector != null)
        {
            if (isUpButton)
                parentSelector.NextLetter();
            else
                parentSelector.PreviousLetter();

            StartCoroutine(PressAnimation());
        }
        else
        {
            Debug.LogWarning($"У кнопки {gameObject.name} не назначен parentSelector!");
        }
    }

    public string GetDescription()
    {
        return isUpButton ? "Следующая буква [LMB]" : "Предыдущая буква [LMB]";
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

        yield return new WaitForSeconds(0.1f);

        if (buttonRenderer != null && defaultMaterial != null)
        {
            buttonRenderer.material = defaultMaterial;
        }

        transform.localPosition = originalPosition;

        isPressed = false;
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
