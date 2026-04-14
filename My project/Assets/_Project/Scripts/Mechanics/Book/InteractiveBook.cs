using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractiveBook : MonoBehaviour, Interactable
{
    [Header("Страницы книги")]
    [TextArea(3, 10)]
    public string[] pages;
    public Sprite[] pageImages;
    public int currentPage = 0;
    
    [Header("UI отображение")]
    public GameObject bookUI;
    public TextMeshProUGUI pageText;
    public TextMeshProUGUI pageNumber;
    public Image pageImage;
    
    [Header("Управление")]
    public KeyCode nextPageKey = KeyCode.D;
    public KeyCode prevPageKey = KeyCode.A;
    public KeyCode closeBookKey = KeyCode.E;
    
    [Header("Настройки позиции")]
    public float holdDistance = 0.8f;
    public float holdDown = -0.2f;
    public Vector3 holdRotation = new Vector3(0, -90, 0);
    public float animationSpeed = 8f;
    
    private FirstPersonController playerController;
    private Transform playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool isOpen = false;
    private bool isAnimating = false;
    
    // Ссылка на прицел
    private CrosshairController crosshair;
    
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            playerCamera = player.GetComponentInChildren<Camera>().transform;
        }
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
        
        // Находим прицел
        crosshair = FindObjectOfType<CrosshairController>();
        
        if (pages == null || pages.Length == 0)
        {
            pages = new string[] { "Страница 1", "Страница 2", "Страница 3" };
        }
        
        UpdatePageDisplay();
    }
    
    void Update()
    {
        if (isOpen && !isAnimating)
        {
            if (Input.GetKeyDown(nextPageKey))
                NextPage();
            if (Input.GetKeyDown(prevPageKey))
                PreviousPage();
            if (Input.GetKeyDown(closeBookKey))
                CloseBook();
        }
    }
    
    public void Interact()
    {
        if (isAnimating) return;
        if (!isOpen) OpenBook();
        else CloseBook();
    }
    
    private void OpenBook()
    {
        isOpen = true;
        
        if (playerController != null)
            playerController.LockAll();
        
        // Скрываем прицел
        if (crosshair != null)
            crosshair.gameObject.SetActive(false);
        
        transform.SetParent(playerCamera);
        StartCoroutine(AnimateOpen());
    }
    
    private IEnumerator AnimateOpen()
    {
        isAnimating = true;
        
        Vector3 startLocalPos = transform.localPosition;
        Quaternion startLocalRot = transform.localRotation;
        Vector3 targetPos = new Vector3(0, holdDown, holdDistance);
        Quaternion targetRot = Quaternion.Euler(holdRotation);
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * animationSpeed;
            transform.localPosition = Vector3.Lerp(startLocalPos, targetPos, t);
            transform.localRotation = Quaternion.Lerp(startLocalRot, targetRot, t);
            yield return null;
        }
        
        transform.localPosition = targetPos;
        transform.localRotation = targetRot;
        
        UpdatePageDisplay();
        
        isAnimating = false;
    }
    
    private void CloseBook()
    {
        StartCoroutine(AnimateClose());
    }
    
    private IEnumerator AnimateClose()
    {
        isAnimating = true;
        
        transform.SetParent(originalParent);
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * animationSpeed;
            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            yield return null;
        }
        
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        isOpen = false;
        isAnimating = false;
        
        if (playerController != null)
            playerController.UnlockAll();
        
        // Показываем прицел обратно
        if (crosshair != null)
            crosshair.gameObject.SetActive(true);
    }
    
    private void UpdatePageDisplay()
    {
        if (pageText != null)
            pageText.text = pages[currentPage];
        
        if (pageImage != null)
        {
            if (pageImages != null && currentPage < pageImages.Length && pageImages[currentPage] != null)
            {
                pageImage.sprite = pageImages[currentPage];
                pageImage.gameObject.SetActive(true);
            }
            else
            {
                pageImage.gameObject.SetActive(false);
            }
        }
        
        if (pageNumber != null)
            pageNumber.text = $"{currentPage + 1} / {pages.Length}";
    }
    
    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePageDisplay();
        }
    }
    
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePageDisplay();
        }
    }
    
    public string GetDescription()
    {
        return isOpen ? "Нажмите E, чтобы закрыть книгу" : "Нажмите, чтобы открыть книгу";
    }
}