using UnityEngine;
using System.Collections;

public class RadioController : MonoBehaviour
{
    [Header("Selectors")]
    [SerializeField] private LetterSelector letterSelector1;
    [SerializeField] private LetterSelector letterSelector2;
    [SerializeField] private NumberRegulator numberDial;

    [Header("Displays")]
    [SerializeField] private TextMesh fullIDDisplay;
    [SerializeField] private TextMesh statusDisplay;

    [Header("Connect Button")]
    [SerializeField] private ConnectButton connectButton;

    [Header("Radar")]
    [SerializeField] private RadarManager radarManager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dialSound;
    [SerializeField] private AudioClip connectSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Status Lights")]
    [SerializeField] private Light statusLight;

    private string currentFullID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (letterSelector1 != null)
            letterSelector1.OnLetterChanged += OnLetterChanged;
        if (letterSelector2 != null)
            letterSelector2.OnLetterChanged += OnLetterChanged;
        if (numberDial != null)
            numberDial.OnValueChanged += OnNumberChanged;

        UpdateFullID();
        ShowStatus("ГОТОВ", Color.white);
    }

    private void OnLetterChanged(char letter)
    {
        PlaySound(dialSound);
        UpdateFullID();
    }

    private void OnNumberChanged(int number)
    {
        PlaySound(dialSound);
        UpdateFullID();
    }

    private void UpdateFullID()
    {
        char l1 = letterSelector1 != null ? letterSelector1.CurrentLetter : 'A';
        char l2 = letterSelector2 != null ? letterSelector2.CurrentLetter : 'A';
        int num = numberDial != null ? numberDial.CurrentValue : 0;

        currentFullID = $"{l1}{l2}{num:D2}";

        if (fullIDDisplay != null)
        {
            fullIDDisplay.text = currentFullID;
        }

    }

    public void TryConnect()
    {
        if (radarManager == null)
        {
            ShowStatus("ОШИБКА СИСТЕМЫ", Color.red);
            PlaySound(errorSound);

            if (connectButton != null)
                connectButton.FlashError();

            return;
        }

        bool exists = radarManager.IsAircraftExists(currentFullID);

        if (exists)
        {
            //radarManager.ApplyPendingTrajectory(currentFullID);

            ShowStatus($"СВЯЗЬ: {currentFullID}", Color.green);
            PlaySound(connectSound);

            radarManager.SelectAircraftByID(currentFullID);

            if (connectButton != null)
                connectButton.FlashSuccess();

            if (statusLight != null)
                StartCoroutine(FlashStatusLight(Color.green));
        }
        else
        {
            ShowStatus($"НЕТ СИГНАЛА", Color.red);
            PlaySound(errorSound);

            if (connectButton != null)
                connectButton.FlashError();

            if (statusLight != null)
                StartCoroutine(FlashStatusLight(Color.red));

            Debug.Log($"[ОШИБКА] Самолет {currentFullID} не найден");
        }
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusDisplay != null)
        {
            statusDisplay.text = message;
            statusDisplay.color = color;
        }
    }

    private IEnumerator FlashStatusLight(Color color)
    {
        if (statusLight == null) yield break;

        statusLight.enabled = true;
        statusLight.color = color;

        yield return new WaitForSeconds(1f);

        statusLight.enabled = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public string GetCurrentID()
    {
        return currentFullID;
    }

    public void SetPendingTrajectory(string aircraftID, Vector2 target)
    {
       
    }
}
