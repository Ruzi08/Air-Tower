using UnityEngine;

public class SoundRainVeterGrom : Sound
{   
    // Новые параметры для случайного воспроизведения второго звука
    public float minDelay = 5f;     // Минимальная задержка между звуками
    public float maxDelay = 15f;    // Максимальная задержка между звуками
    public int secondSoundIndex = 1; // Индекс второго звука в массиве sounds (по умолчанию 1)
    public float secondSoundVolume = 0.7f; // Громкость второго звука
    public float secondSoundPitchMin = 0.9f; // Минимальный питч второго звука
    public float secondSoundPitchMax = 1.1f; // Максимальный питч второго звука
    
    private float _nextSoundTimer;    // Таймер до следующего случайного звука
    private bool _isSecondSoundPlaying; // Флаг, играет ли второй звук
    private AudioSource _secondAudioSource; // Отдельный AudioSource для второго звука
    
    void Start()
    {
        if (sounds == null || sounds.Length == 0 || sounds[0] == null)
        {
            Debug.LogWarning("Sound.Start() requires sounds[0] to be assigned before playback can begin.", this);
            return;
        }
        // Создаем отдельный AudioSource для второго звука
        _secondAudioSource = gameObject.AddComponent<AudioSource>();
        // Запускаем основной звук
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: minPitch, p2: maxPitch);
        
        // Копируем настройки маршрутизации/пространственного звука с основного AudioSource
        CopyAudioSourceSettings(AudioSrc, _secondAudioSource);
        _secondAudioSource.playOnAwake = false;
        _secondAudioSource.loop = false;
        
        // Инициализация таймера
        SetRandomTimer();
    }
    private void CopyAudioSourceSettings(AudioSource source, AudioSource target)
    {
        if (source == null || target == null) return;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.bypassEffects = source.bypassEffects;
        target.bypassListenerEffects = source.bypassListenerEffects;
        target.bypassReverbZones = source.bypassReverbZones;
        target.priority = source.priority;
        target.mute = source.mute;
        target.spatialBlend = source.spatialBlend;
        target.reverbZoneMix = source.reverbZoneMix;
        target.dopplerLevel = source.dopplerLevel;
        target.spread = source.spread;
        target.rolloffMode = source.rolloffMode;
        target.minDistance = source.minDistance;
        target.maxDistance = source.maxDistance;
        target.panStereo = source.panStereo;
        target.velocityUpdateMode = source.velocityUpdateMode;
        target.ignoreListenerPause = source.ignoreListenerPause;
        target.ignoreListenerVolume = source.ignoreListenerVolume;
        if (source.rolloffMode == AudioRolloffMode.Custom)
        {
            target.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
        }
    }

    void Update()
    {
        // Вызываем Update родительского класса для обработки основной логики
        base.Update();
        
        if (listener == null) return;
        
        // Обработка случайного воспроизведения второго звука
        if (!_isSecondSoundPlaying && sounds.Length > secondSoundIndex && _secondAudioSource != null)
        {
            _nextSoundTimer -= Time.deltaTime;
            if (_nextSoundTimer <= 0)
            {
                PlayRandomSecondSound();
                SetRandomTimer();
            }
        }
        
        // Обновляем громкость второго AudioSource в зависимости от текущей громкости основного
        if (_secondAudioSource != null && _secondAudioSource.isPlaying)
        {
            // Громкость второго звука зависит от громкости основного (эффект затухания вместе с дождем)
            _secondAudioSource.volume = secondSoundVolume * AudioSrc.volume;
        }
    }
    

    private void PlayRandomSecondSound()
    {
        if (sounds.Length <= secondSoundIndex || sounds[secondSoundIndex] == null) return;
        if (_secondAudioSource == null) return;
        
        _isSecondSoundPlaying = true;
        
        _secondAudioSource.clip = sounds[secondSoundIndex];
        _secondAudioSource.volume = secondSoundVolume * AudioSrc.volume;
        _secondAudioSource.pitch = Random.Range(secondSoundPitchMin, secondSoundPitchMax);
        _secondAudioSource.Play();
        
        // Сбрасываем флаг через длительность звука
        Invoke(nameof(ResetSecondSoundFlag), sounds[secondSoundIndex].length);
    }
    
    private void ResetSecondSoundFlag()
    {
        _isSecondSoundPlaying = false;
    }
    
    private void SetRandomTimer()
    {
        _nextSoundTimer = Random.Range(minDelay, maxDelay);
    }
    
    // Переопределяем метод StopSnd, чтобы останавливать и второй звук
    public override void StopSnd()
    {
        base.StopSnd();
        if (_secondAudioSource != null && _secondAudioSource.isPlaying)
        {
            _secondAudioSource.Stop();
        }
        _isSecondSoundPlaying = false;
    }
    
    // Метод для ручного вызова второго звука
    public void ManualPlaySecondSound()
    {
        PlayRandomSecondSound();
    }
    
    // Метод для изменения индекса второго звука
    public void SetSecondSoundIndex(int index)
    {
        if (index > 0 && index < sounds.Length)
            secondSoundIndex = index;
    }
    
    // Метод для настройки интервалов
    public void SetRandomDelay(float min, float max)
    {
        minDelay = min;
        maxDelay = max;
        SetRandomTimer();
    }
}