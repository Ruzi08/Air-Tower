using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MissedAircraftPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxMissedAircrafts = 5; // N самолётов
    [SerializeField] private GameObject aircraftIconPrefab;
    [SerializeField] private Color missedColor = Color.red;
    [SerializeField] private Color normalColor = Color.gray;

    [Header("Layout")]
    [SerializeField] private Transform iconsContainer;
    [SerializeField] private Text titleText;

    private List<Image> aircraftIcons = new List<Image>();
    private int missedCount = 0;
    private System.Action onAllMissed;

    void Start()
    {
        CreateIcons();
    }

    private void CreateIcons()
    {
        // Очищаем старые если есть
        foreach (Transform child in iconsContainer)
            Destroy(child.gameObject);
        aircraftIcons.Clear();

        // Создаём N иконок
        for (int i = 0; i < maxMissedAircrafts; i++)
        {
            GameObject iconObj = Instantiate(aircraftIconPrefab, iconsContainer);
            Image iconImage = iconObj.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.color = normalColor; // Неактивный цвет
                aircraftIcons.Add(iconImage);
            }
        }

        UpdateTitle();
    }

    public void AddMissedAircraft()
    {
        if (missedCount >= maxMissedAircrafts) return;

        // Закрашиваем следующий самолётик красным
        aircraftIcons[missedCount].color = missedColor;
        missedCount++;

        UpdateTitle();

        Debug.Log($"❌ Промах! {missedCount}/{maxMissedAircrafts}");

        // Проверяем, все ли стали красными
        if (missedCount >= maxMissedAircrafts)
        {
            AllMissed();
        }
    }

    private void AllMissed()
    {
        Debug.Log("💀 ВСЕ ПРОМАХИ ИСЧЕРПАНЫ!");

        if (titleText != null)
            titleText.text = "ПОТЕРЯНЫ ВСЕ!";

        onAllMissed?.Invoke();

        // Анимация или эффект
        StartCoroutine(FlashAllIcons());
    }

    private System.Collections.IEnumerator FlashAllIcons()
    {
        for (int flash = 0; flash < 3; flash++)
        {
            foreach (var icon in aircraftIcons)
                icon.color = Color.white;

            yield return new WaitForSeconds(0.3f);

            foreach (var icon in aircraftIcons)
                icon.color = missedColor;

            yield return new WaitForSeconds(0.3f);
        }
    }

    private void UpdateTitle()
    {
        if (titleText != null)
            titleText.text = $"Промахи: {missedCount}/{maxMissedAircrafts}";
    }

    public void ResetMissed()
    {
        missedCount = 0;
        foreach (var icon in aircraftIcons)
            icon.color = normalColor;

        UpdateTitle();
    }

    public void SetOnAllMissed(System.Action callback)
    {
        onAllMissed = callback;
    }

    public bool IsAllMissed()
    {
        return missedCount >= maxMissedAircrafts;
    }

    // Для настройки из инспектора
    public void SetMaxMissed(int max)
    {
        maxMissedAircrafts = max;
        CreateIcons();
    }
}