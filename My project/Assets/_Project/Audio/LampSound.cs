using Unity.VisualScripting;
using UnityEngine;

public class LampSound : Sound
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource yellowSource;
    [SerializeField] private AudioSource redSource;
    [SerializeField] private AudioSource greenSource; // Отдельный для зелёного

    [Header("Audio Clips")]
    [SerializeField] private AudioClip yellowLoop;
    [SerializeField] private AudioClip redLoop;
    [SerializeField] private AudioClip greenOneShot;

    void Start()
    {
        // Создаём источники, если не назначены
        if (yellowSource == null)
        {
            yellowSource = gameObject.AddComponent<AudioSource>();
            yellowSource.loop = true;
            yellowSource.playOnAwake = false;
            yellowSource.volume = volume;
        }

        if (redSource == null)
        {
            redSource = gameObject.AddComponent<AudioSource>();
            redSource.loop = true;
            redSource.playOnAwake = false;
            redSource.volume = volume;
        }

        if (greenSource == null)
        {
            greenSource = gameObject.AddComponent<AudioSource>();
            greenSource.loop = false;
            greenSource.playOnAwake = false;
            greenSource.volume = volume;
        }
    }

    public void PlayYellowLoop()
    {
        if (yellowSource != null && yellowLoop != null && !yellowSource.isPlaying)
        {
            yellowSource.clip = yellowLoop;
            yellowSource.Play();
        }
    }

    public void StopYellow()
    {
        if (yellowSource != null && yellowSource.isPlaying)
            yellowSource.Stop();
    }

    public void PlayRedLoop()
    {
        if (redSource != null && redLoop != null && !redSource.isPlaying)
        {
            redSource.clip = redLoop;
            redSource.Play();
        }
    }

    public void StopRed()
    {
        if (redSource != null && redSource.isPlaying)
            redSource.Stop();
    }

    public void PlayGreenOneShot()
    {
        if (greenSource != null && greenOneShot != null)
        {
            greenSource.PlayOneShot(greenOneShot);
        }
    }

    public void StopGreen()
    {
        if (greenSource != null && greenSource.isPlaying)
            greenSource.Stop();
    }
}