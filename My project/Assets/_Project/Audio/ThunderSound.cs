using UnityEngine;

/// <summary>
/// Дочерний класс Sound для воспроизведения звуков молнии/грома.
/// Источник звука размещается в случайной точке вокруг слушателя.
/// </summary>
public class ThunderSound : Sound
{
    [Header("Thunder Settings")]
    public float minDistance = 10f;   // Минимальное расстояние до молнии
    public float maxDistance = 50f;   // Максимальное расстояние до молнии
    public bool autoDestroy = true;   // Уничтожить объект после воспроизведения
    public float destroyDelay = 0.5f; // Задержка перед уничтожением (чтобы звук доиграл)

    protected override void Awake()
    {
        base.Awake();
        // Для молнии звук однократный, не зацикленный
        loop = false;
        destroyed = false; // не уничтожаем в базовом классе, управляем сами
    }

    protected override void Start()
    {
        // Не вызываем base.Start(), чтобы не начинать воспроизведение sounds[0] автоматически.
        // Вместо этого инициализируем listener, если он не назначен.
        if (listener == null)
        {
            AudioListener audioListener = FindObjectOfType<AudioListener>();
            if (audioListener != null)
                listener = audioListener;
            else
                Debug.LogError("ThunderSound: No AudioListener found in scene.");
        }
    }

    /// <summary>
    /// Воспроизвести звук молнии в случайной позиции вокруг слушателя.
    /// </summary>
    /// <param name="thunderClip">Аудиоклип грома (если null, использует sounds[0])</param>
    public void PlayRandomThunder()
    {
        if (listener == null)
        {
            Debug.LogError("ThunderSound: No AudioListener assigned.");
            return;
        }

        // Выбираем случайное направление (по горизонтали) и расстояние
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 randomOffset = new Vector3(randomDirection.x, 0f, randomDirection.y) * distance;
        
        // Немного случайной высоты (опционально)
        randomOffset.y = Random.Range(-2f, 5f);
        
        // Устанавливаем позицию источника звука относительно слушателя
        transform.position = listener.transform.position + randomOffset;

        // Выбираем клип
        AudioClip clip;
        int index = (sounds.Length > 1) ? Random.Range(0, 2) : 0;
        clip = sounds[index];
        
        // Воспроизводим
        PlaySnd(clip, volume, destroyed, minPitch, maxPitch, loop);

        if (autoDestroy)
            Destroy(gameObject, destroyDelay + clip.length);
    }

    // Опционально: автоматический вызов через таймер
    public void ScheduleRandomThunder(float minDelay, float maxDelay)
    {
        float delay = Random.Range(minDelay, maxDelay);
        Invoke(nameof(PlayRandomThunder), delay);
    }
}