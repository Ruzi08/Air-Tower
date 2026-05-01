using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class MissedAircraftPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxMissed = 5;
    [SerializeField] private Color missedColor = Color.red;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private GameObject aircraftIconPrefab;
    [SerializeField] private float showDuration = 3f; // Сколько секунд показывать
    [SerializeField] private float fadeDuration = 0.5f; // Длительность появления/исчезновения

    [Header("UI References")]
    [SerializeField] private Transform iconsContainer;
    [SerializeField] private Text titleText;
    [SerializeField] private CanvasGroup canvasGroup;

    private List<Image> icons = new List<Image>();
    private int missedCount = 0;
    private Coroutine hideCoroutine;

    void Start()
    {
        CreateIcons();
        UpdateTitle();

        // Сразу скрываем
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        else
            gameObject.SetActive(false);
    }

    private void CreateIcons()
    {
        for (int i = iconsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = iconsContainer.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        icons.Clear();

        for (int i = 0; i < maxMissed; i++)
        {
            GameObject iconObj;

            if (aircraftIconPrefab != null)
            {
                iconObj = Instantiate(aircraftIconPrefab, iconsContainer);
            }
            else
            {
                iconObj = new GameObject($"Icon_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObj.transform.SetParent(iconsContainer, false);
                RectTransform rect = iconObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(35, 35);
            }

            iconObj.name = $"AircraftIcon_{i}";

            Image img = iconObj.GetComponent<Image>();
            if (img == null) img = iconObj.AddComponent<Image>();

            img.color = normalColor;
            img.raycastTarget = false;

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                iconRect.localScale = Vector3.one;
                iconRect.localRotation = Quaternion.identity;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
            }

            icons.Add(img);
        }
    }

    public void AddMiss()
    {
        if (missedCount >= icons.Count) return;

        // Закрашиваем иконку
        icons[missedCount].color = missedColor;
        missedCount++;

        UpdateTitle();

        // Показываем панель
        Show();

        // Перезапускаем таймер скрытия
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());

        if (missedCount >= icons.Count)
        {
            OnAllMissed();
        }
    }

    private void Show()
    {
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    private void OnAllMissed()
    {
        if (titleText != null)
            titleText.text = "ПОТЕРЯНЫ ВСЕ!";

        // При всех промахах показываем постоянно
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        Show();

        StartCoroutine(FlashAllIcons());
    }

    private IEnumerator FlashAllIcons()
    {
        for (int flash = 0; flash < 4; flash++)
        {
            foreach (var icon in icons)
                icon.color = Color.white;

            yield return new WaitForSeconds(0.3f);

            foreach (var icon in icons)
                icon.color = missedColor;

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void UpdateTitle()
    {
        if (titleText != null)
        {
            int remaining = icons.Count - missedCount;
            titleText.text = $"ЖИЗНИ: {remaining}/{icons.Count}";
        }
    }

    public void ResetMisses()
    {
        missedCount = 0;
        foreach (var icon in icons)
            icon.color = normalColor;

        UpdateTitle();
    }

    public bool IsAllMissed()
    {
        return missedCount >= icons.Count;
    }

    public int GetMissedCount() => missedCount;
    public int GetMaxMissed() => maxMissed;
}