using UnityEngine;
using System.Collections.Generic;

public class Sound_RandomPlayerAdvanced : Sound
{
    public float minDelay = 5f;
    public float maxDelay = 15f;
    public float randomPitchMin = 0.85f;
    public float randomPitchMax = 1.2f;
    
    public List<int> allowedSoundIndices = new List<int>();
    
    public bool excludeMainSound = true;
    
    private float nextSoundTimer;
    private bool isRandomSoundPlaying;
    private AudioSource randomAudioSource;
    private List<int> availableIndices = new List<int>();
    
    protected override void Awake()
    {
        base.Awake();
        
        randomAudioSource = gameObject.AddComponent<AudioSource>();
        randomAudioSource.playOnAwake = false;
        randomAudioSource.loop = false;
        
        UpdateAvailableIndices();
    }
    
    protected override void Start()
    {
        base.Start();
        SetRandomTimer();
    }
    
    private void UpdateAvailableIndices()
    {
        availableIndices.Clear();
        
        if (sounds == null) return;
        
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] == null) continue;
            
            // Проверяем исключение основного звука
            if (excludeMainSound && i == 0) continue;
            
            // Проверяем разрешенные индексы
            if (allowedSoundIndices.Count > 0)
            {
                if (allowedSoundIndices.Contains(i))
                    availableIndices.Add(i);
            }
            else
            {
                availableIndices.Add(i);
            }
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (listener == null) return;
        
        bool canPlay = !isRandomSoundPlaying && availableIndices.Count > 0;
        
        if (canPlay)
        {
            bool playerCondition = IsSoundAvable;
            
            if (playerCondition)
            {
                nextSoundTimer -= Time.deltaTime;
                if (nextSoundTimer <= 0)
                {
                    PlayRandomSoundFromAvailable();
                    SetRandomTimer();
                }
            }
        }
        
        if (randomAudioSource != null && randomAudioSource.isPlaying && audioSrc != null)
        {
            randomAudioSource.volume = volume * audioSrc.volume;
        }
    }
    
    private void PlayRandomSoundFromAvailable()
    {
        if (availableIndices.Count == 0) return;
        
        int randomIndex = Random.Range(0, availableIndices.Count);
        int soundIndex = availableIndices[randomIndex];
        
        if (soundIndex >= sounds.Length || sounds[soundIndex] == null) return;
        
        isRandomSoundPlaying = true;
        
        randomAudioSource.clip = sounds[soundIndex];
        randomAudioSource.volume = volume * audioSrc.volume;
        randomAudioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
        randomAudioSource.Play();
        
        Debug.Log($"Воспроизведен звук [{soundIndex}]: {sounds[soundIndex].name}");
        
        CancelInvoke(nameof(ResetRandomSoundFlag));
        Invoke(nameof(ResetRandomSoundFlag), sounds[soundIndex].length);
    }
    
    private void ResetRandomSoundFlag()
    {
        isRandomSoundPlaying = false;
    }
    
    private void SetRandomTimer()
    {
        nextSoundTimer = Random.Range(minDelay, maxDelay);
    }
    
    public void ManualPlayRandomSound()
    {
        PlayRandomSoundFromAvailable();
    }
    
    public void SetRandomDelay(float min, float max)
    {
        minDelay = Mathf.Max(0.1f, min);
        maxDelay = Mathf.Max(minDelay, max);
        SetRandomTimer();
    }
    
    public void RefreshAllowedIndices()
    {
        UpdateAvailableIndices();
    }
    
    public override void StopSnd()
    {
        base.StopSnd();
        if (randomAudioSource != null && randomAudioSource.isPlaying)
        {
            randomAudioSource.Stop();
        }
        isRandomSoundPlaying = false;
        CancelInvoke(nameof(ResetRandomSoundFlag));
    }
    
    void OnDestroy()
    {
        if (randomAudioSource != null)
        {
            Destroy(randomAudioSource);
        }
        CancelInvoke();
    }
}