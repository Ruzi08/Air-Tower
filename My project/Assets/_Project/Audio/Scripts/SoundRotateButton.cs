using UnityEngine;
using System.Collections.Generic;

public class SoundRotateButton : Sound
{
    [Header("Random Fast Sound Settings")]
    public bool allowOverlap = true;       // Разрешить наложение звуков
    
    private List<AudioSource> audioSourcesPool; // Пул источников для наложения
    private int currentSourceIndex;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Создаем пул AudioSource для наложения звуков
        audioSourcesPool = new List<AudioSource>();
        
        // Основной AudioSource уже есть от родителя
        audioSourcesPool.Add(AudioSrc);
        
        // Добавляем дополнительные AudioSource для наложения
        if (allowOverlap)
        {
            for (int i = 0; i < 5; i++) // 5 дополнительных источников
            {
                AudioSource additionalSource = gameObject.AddComponent<AudioSource>();
                additionalSource.playOnAwake = false;
                additionalSource.loop = false;
                additionalSource.spatialBlend = AudioSrc.spatialBlend;
                additionalSource.rolloffMode = AudioSrc.rolloffMode;
                additionalSource.maxDistance = AudioSrc.maxDistance;
                audioSourcesPool.Add(additionalSource);
            }
        }
    }
    
    protected override void Start()
    {
        if (sounds == null || sounds.Length == 0)
        {
            Debug.LogWarning("RandomFastSound.Start() requires sounds array to be assigned.", this);
            return;
        }
    }
    
    /// <summary>
    /// Проиграть случайный звук из списка
    /// </summary>
    public void PlayRandomSound()
    {
        if (sounds == null || sounds.Length == 0) return;
        
        int randomIndex = Random.Range(0, sounds.Length);
        AudioClip randomClip = sounds[randomIndex];
        
        if (randomClip != null)
        {
            PlayFastSound(randomClip);
        }
    }
    
    /// <summary>
    /// Быстрое проигрывание звука с возможностью наложения
    /// </summary>
    public void PlayFastSound(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource sourceToUse = GetAvailableAudioSource();
        
        if (sourceToUse != null)
        {
            float randomPitch = Random.Range(minPitch, maxPitch);
            float randomVolume = Random.Range(minVolume, maxVolume);
            
            sourceToUse.clip = clip;
            sourceToUse.pitch = randomPitch;
            sourceToUse.volume = CurrentVolume; // Используем текущую громкость от 3D позиции
            sourceToUse.loop = false;
            sourceToUse.Play();
            
            // Автоматически очищаем clip после проигрывания
            if (!sourceToUse.loop)
            {
                StartCoroutine(ClearClipAfterPlay(sourceToUse, clip.length / randomPitch));
            }
        }
    }
    
    /// <summary>
    /// Проиграть звук с определенной задержкой
    /// </summary>
    public void PlayRandomSoundDelayed(float delay)
    {
        StartCoroutine(PlayWithDelay(delay));
    }
    
    /// <summary>
    /// Проиграть определенный звук из списка по индексу
    /// </summary>
    public void PlaySoundByIndex(int index)
    {
        if (index >= 0 && index < sounds.Length && sounds[index] != null)
        {
            PlayFastSound(sounds[index]);
        }
    }
    
    /// <summary>
    /// Получить доступный AudioSource из пула
    /// </summary>
    private AudioSource GetAvailableAudioSource()
    {
        if (!allowOverlap)
        {
            // Если наложение запрещено, используем только первый источник, если он не играет
            if (!audioSourcesPool[0].isPlaying)
                return audioSourcesPool[0];
            return null;
        }
        
        // Ищем неиграющий источник
        foreach (AudioSource source in audioSourcesPool)
        {
            if (!source.isPlaying)
                return source;
        }
        
        // Если все играют и разрешено наложение, возвращаем следующий по кругу
        currentSourceIndex = (currentSourceIndex + 1) % audioSourcesPool.Count;
        return audioSourcesPool[currentSourceIndex];
    }
    
    /// <summary>
    /// Очистить clip после проигрывания (для освобождения ресурсов)
    /// </summary>
    private System.Collections.IEnumerator ClearClipAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (source != null && !source.isPlaying)
        {
            source.clip = null;
        }
    }
    
    /// <summary>
    /// Проиграть звук с задержкой
    /// </summary>
    private System.Collections.IEnumerator PlayWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRandomSound();
    }
    
    /// <summary>
    /// Массовое проигрывание нескольких случайных звуков одновременно
    /// </summary>
    public void PlayMultipleRandomSounds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PlayRandomSound();
        }
    }
    
    protected void OnDestroy()
    {
        if (audioSourcesPool != null)
        {
            for (int i = 1; i < audioSourcesPool.Count; i++)
            {
                if (audioSourcesPool[i] != null)
                    Destroy(audioSourcesPool[i]);
            }
        }
    }
}