using UnityEngine;

public class Sound_RainVeterGrom : Sound
{   
    // Новые параметры для случайного воспроизведения второго звука
    public float minDelay = 5f;     // Минимальная задержка между звуками
    public float maxDelay = 15f;    // Максимальная задержка между звуками
    public int secondSoundIndex = 1; // Индекс второго звука в массиве sounds (по умолчанию 1)
    public float secondSoundVolume = 0.7f; // Громкость второго звука
    public float secondSoundPitchMin = 0.9f; // Минимальный питч второго звука
    public float secondSoundPitchMax = 1.1f; // Максимальный питч второго звука
    
    private float nextSoundTimer;    // Таймер до следующего случайного звука
    private bool isSecondSoundPlaying; // Флаг, играет ли второй звук
    private AudioSource secondAudioSource; // Отдельный AudioSource для второго звука
    
    void Start()
    {
        // Создаем отдельный AudioSource для второго звука
        secondAudioSource = gameObject.AddComponent<AudioSource>();
        secondAudioSource.playOnAwake = false;
        secondAudioSource.loop = false;
        
        // Запускаем основной звук
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: MinPitch, p2: MaxPitch);
        
        // Инициализация таймера
        SetRandomTimer();
    }

    void Update()
    {
        // Вызываем Update родительского класса для обработки основной логики
        base.Update();
        
        if (listener == null) return;
        
        // Обработка случайного воспроизведения второго звука
        if (!isSecondSoundPlaying && sounds.Length > secondSoundIndex && secondAudioSource != null)
        {
            nextSoundTimer -= Time.deltaTime;
            if (nextSoundTimer <= 0)
            {
                PlayRandomSecondSound();
                SetRandomTimer();
            }
        }
        
        // Обновляем громкость второго AudioSource в зависимости от текущей громкости основного
        if (secondAudioSource != null && secondAudioSource.isPlaying)
        {
            // Громкость второго звука зависит от громкости основного (эффект затухания вместе с дождем)
            secondAudioSource.volume = secondSoundVolume * audioSrc.volume;
        }
    }
    

    private void PlayRandomSecondSound()
    {
        if (sounds.Length <= secondSoundIndex || sounds[secondSoundIndex] == null) return;
        if (secondAudioSource == null) return;
        
        isSecondSoundPlaying = true;
        
        secondAudioSource.clip = sounds[secondSoundIndex];
        secondAudioSource.volume = secondSoundVolume * audioSrc.volume;
        secondAudioSource.pitch = Random.Range(secondSoundPitchMin, secondSoundPitchMax);
        secondAudioSource.Play();
        
        // Сбрасываем флаг через длительность звука
        Invoke(nameof(ResetSecondSoundFlag), sounds[secondSoundIndex].length);
    }
    
    private void ResetSecondSoundFlag()
    {
        isSecondSoundPlaying = false;
    }
    
    private void SetRandomTimer()
    {
        nextSoundTimer = Random.Range(minDelay, maxDelay);
    }
    
    // Переопределяем метод StopSnd, чтобы останавливать и второй звук
    public new void StopSnd()
    {
        base.StopSnd();
        if (secondAudioSource != null && secondAudioSource.isPlaying)
        {
            secondAudioSource.Stop();
        }
        isSecondSoundPlaying = false;
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