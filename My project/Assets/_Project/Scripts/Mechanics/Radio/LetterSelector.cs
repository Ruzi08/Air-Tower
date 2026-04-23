using UnityEngine;
using TMPro;

public class LetterSelector : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshPro displayText;

    [Header("Settings")]
    [SerializeField] private char startLetter = 'A';

    private char currentLetter;

    public System.Action<char> OnLetterChanged;

    public char CurrentLetter => currentLetter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLetter = startLetter;
        UpdateDisplay();
    }
    public void NextLetter()
    {
        if (currentLetter < 'K')
            currentLetter++;
        else
            currentLetter = 'A';

        UpdateDisplay();
        Debug.Log($"Буква изменена на: {currentLetter}");
    }

    public void PreviousLetter()
    {
        if (currentLetter > 'A')
            currentLetter--;
        else
            currentLetter = 'K';

        UpdateDisplay();
        Debug.Log($"Буква изменена на: {currentLetter}");
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text = currentLetter.ToString();
        }

        OnLetterChanged?.Invoke(currentLetter);
    }
}
