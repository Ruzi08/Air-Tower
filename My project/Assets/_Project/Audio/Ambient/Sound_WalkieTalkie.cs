using UnityEngine;
using System.Collections.Generic;

public class SoundWalkieTalkie : Sound
{
    public float minDelay = 5f;
    public float maxDelay = 15f;
    public float randomPitchMin = 0.85f;
    public float randomPitchMax = 1.2f;
    
    public List<int> allowedSoundIndices = new List<int>();
    
    public bool excludeMainSound = true;
    
    private float _nextSoundTimer;
    private bool _isRandomSoundPlaying;
    private AudioSource _randomAudioSource;
    private List<int> _availableIndices = new List<int>();
    
    protected override void Awake()
    {
        base.Awake();
        
        _randomAudioSource = gameObject.AddComponent<AudioSource>();
        _randomAudioSource.playOnAwake = false;
        _randomAudioSource.loop = false;
        
        UpdateAvailableIndices();
    }
    
    protected override void Start()
    {
        
        base.Start();
        SetRandomTimer();
    }
    
    private void UpdateAvailableIndices()
    {
        _availableIndices.Clear();
        
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
                    _availableIndices.Add(i);
            }
            else
            {
                _availableIndices.Add(i);
            }
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (listener == null) return;
        
        bool canPlay = !_isRandomSoundPlaying && _availableIndices.Count > 0;
        
        if (canPlay)
        {
            bool playerCondition = IsSoundAvailable;
            
            if (playerCondition)
            {
               _nextSoundTimer -= Time.deltaTime;
                if (_nextSoundTimer <= 0)
                {
                    PlayRandomSoundFromAvailable();
                    SetRandomTimer();
                }
            }
        }
        
        if (_randomAudioSource != null && _randomAudioSource.isPlaying && AudioSrc != null)
        {
            _randomAudioSource.volume = volume * AudioSrc.volume;
        }
    }
    
    private void PlayRandomSoundFromAvailable()
    {
        if (_availableIndices.Count == 0) return;
        
        int randomIndex = Random.Range(0, _availableIndices.Count);
        int soundIndex = _availableIndices[randomIndex];
        
        if (soundIndex >= sounds.Length || sounds[soundIndex] == null) return;
        
        _isRandomSoundPlaying = true;
        
        _randomAudioSource.clip = sounds[soundIndex];
        _randomAudioSource.volume = volume;
        _randomAudioSource.pitch = Random.Range(randomPitchMin, randomPitchMax);
        _randomAudioSource.Play();
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Воспроизведен звук [{soundIndex}]: {sounds[soundIndex].name}");
        #endif
        
        CancelInvoke(nameof(ResetRandomSoundFlag));
        Invoke(nameof(ResetRandomSoundFlag), sounds[soundIndex].length);
    }
    
    private void ResetRandomSoundFlag()
    {
        _isRandomSoundPlaying = false;
    }
    
    private void SetRandomTimer()
    {
        _nextSoundTimer = Random.Range(minDelay, maxDelay);
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
        if (_randomAudioSource != null && _randomAudioSource.isPlaying)
        {
            _randomAudioSource.Stop();
        }
        _isRandomSoundPlaying = false;
        CancelInvoke(nameof(ResetRandomSoundFlag));
    }
    
    void OnDestroy()
    {
        if (_randomAudioSource != null)
        {
            Destroy(_randomAudioSource);
        }
        CancelInvoke();
    }
}