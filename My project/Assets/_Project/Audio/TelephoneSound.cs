using UnityEngine;

public class TelephoneSound : Sound
{
    [Header("Ring Settings")]
    public AudioClip ringClip;
    [Range(0, 1)] public float ringVolume = 0.8f;
    public float ringMinPitch = 0.9f;
    public float ringMaxPitch = 1.1f;

    [Header("Pickup / Hangup")]
    public AudioClip pickUpClip;
    public AudioClip hangUpClip;
    [Range(0, 1)] public float pickUpVolume = 0.7f;
    [Range(0, 1)] public float hangUpVolume = 0.7f;
    public float pickUpPitch = 1f;
    public float hangUpPitch = 1f;

    private AudioSource fxSource;
    private bool isRinging = false;

    void Start()
    {
        
    }
    protected override void Awake()
    {
        base.Awake();

        // Добавляем отдельный источник для коротких звуков
        fxSource = gameObject.AddComponent<AudioSource>();
        fxSource.playOnAwake = false;
        fxSource.spatialBlend = 1f;          // 3D звук
        fxSource.rolloffMode = AudioRolloffMode.Linear;
        fxSource.maxDistance = maxDistance;

        // Автоматически находим слушателя (камера игрока)
        if (listener == null)
            listener = FindObjectOfType<AudioListener>();

        // Используем ringClip, если sounds[] не заполнен
        if (ringClip != null && (sounds == null || sounds.Length == 0))
            sounds = new AudioClip[] { ringClip };
    }

    public void StartRing()
    {   
        Debug.Log($"StartRing called, sounds[0] = {(sounds != null && sounds.Length>0 ? sounds[0].name : "null")}");
        if (sounds == null || sounds.Length == 0 || sounds[0] == null)
        {
            Debug.LogError("TelephoneSound: no ring clip assigned!");
            return;
        }

        isRinging = true;
        // Запускаем зацикленный звонок через унаследованный PlaySnd
        PlaySnd(sounds[0], ringVolume, false, ringMinPitch, ringMaxPitch, true);
    }

    public void StopRing()
    {
        Debug.Log("StopRing вызван, AudioSrc.isPlaying = " + (AudioSrc != null && AudioSrc.isPlaying));
        if (AudioSrc != null && AudioSrc.isPlaying)
            AudioSrc.Stop();
    }

    public void PlayPickUp()
    {
        if (pickUpClip == null)
        {
            Debug.LogWarning("TelephoneSound: pickUpClip not assigned");
            return;
        }
        fxSource.pitch = pickUpPitch;
        fxSource.PlayOneShot(pickUpClip, pickUpVolume);
    }

    public void PlayHangUp()
    {
        if (hangUpClip == null)
        {
            Debug.LogWarning("TelephoneSound: hangUpClip not assigned");
            return;
        }
        fxSource.pitch = hangUpPitch;
        fxSource.PlayOneShot(hangUpClip, hangUpVolume);
    }
}