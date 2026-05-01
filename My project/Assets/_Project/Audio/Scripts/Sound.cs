using UnityEngine;

public class Sound : MonoBehaviour
{
    [Header("Settings")]
    public bool loop;
    public bool destroyed;
    public float minPitch = 0.85f;
    public float maxPitch = 1.2f;
    public AudioClip[] sounds;
    public bool isPlaying;              // Можно не использовать, оставлено для совместимости
    public float minVolume = 0f;
    public float maxVolume = 1f;
    public float maxDistance = 20f;
    public float fadeSpeed = 2f;

    [Header("Components")]
    public AudioListener listener;      // Если не задан, найдётся автоматически

    [SerializeField] protected float volume;   // стартовое значение громкости (обычно maxVolume)
    protected AudioSource AudioSrc;
    protected bool IsSoundAvailable;
    protected float CurrentVolume;
    protected float TargetVolume;

    protected virtual void Awake()
    {
        AudioSrc = GetComponent<AudioSource>();
        if (AudioSrc == null)
            AudioSrc = gameObject.AddComponent<AudioSource>();

        // Автоматический поиск AudioListener, если не назначен вручную
        if (listener == null)
        {
            listener = FindObjectOfType<AudioListener>();
            if (listener == null)
                Debug.LogWarning("Sound: No AudioListener found in scene. Sound will not fade by distance.", this);
        }

        volume = maxVolume;
        CurrentVolume = minVolume;
        TargetVolume = minVolume;
    }

    protected virtual void Start()
    {
        if (sounds == null || sounds.Length == 0 || sounds[0] == null)
        {
            Debug.LogWarning("Sound.Start() requires sounds[0] to be assigned before playback can begin.", this);
            return;
        }
        // Старт с автоматическим учётом расстояния и видимости
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: minPitch, p2: maxPitch);
    }

    protected virtual void Update()
    {
        if (listener == null) return;

        float distance = Vector3.Distance(transform.position, listener.transform.position);

        // Вне зоны слышимости -> полное затухание
        if (distance > maxDistance)
        {
            TargetVolume = minVolume;
            CurrentVolume = Mathf.Lerp(CurrentVolume, TargetVolume, Time.deltaTime * fadeSpeed);
            AudioSrc.volume = CurrentVolume;
            return;
        }

        // Громкость в зависимости от расстояния
        float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
        float calculatedVolume = Mathf.Lerp(minVolume, maxVolume, distanceVolume);

        // Проверка видимости (препятствия)
        IsSoundAvailable = CheckVisibility(listener.transform.position);

        TargetVolume = IsSoundAvailable ? calculatedVolume : minVolume;

        // Плавное изменение громкости
        CurrentVolume = Mathf.Lerp(CurrentVolume, TargetVolume, Time.deltaTime * fadeSpeed);
        AudioSrc.volume = CurrentVolume;
    }

    protected virtual bool CheckVisibility(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        float sphereRadius = 0.3f;

        if (Physics.SphereCast(transform.position, sphereRadius, direction, out RaycastHit hit, distance))
        {
            // Игрок или объект с тегом "Player" – видим
            if (hit.transform.CompareTag("Player") ||
                hit.transform == listener.transform ||
                hit.transform.root.CompareTag("Player"))
            {
                Debug.DrawRay(transform.position, direction * distance, Color.green);
                return true;
            }

            // Доп. проверка: возможно луч прошёл мимо, но игрок очень близко к точке попадания
            float distanceToPlayer = Vector3.Distance(hit.point, targetPosition);
            if (distanceToPlayer < sphereRadius)
            {
                Debug.DrawRay(transform.position, direction * distance, Color.yellow);
                return true;
            }

            Debug.DrawRay(transform.position, direction * distance, Color.red);
            return false;
        }

        // Нет препятствий
        Debug.DrawRay(transform.position, direction * distance, Color.green);
        return true;
    }

    // ----------------------------------------------------------------------
    //  ГЛАВНОЕ ИСПРАВЛЕНИЕ: PlaySnd больше не игнорирует расстояние/видимость
    // ----------------------------------------------------------------------
    public virtual void PlaySnd(AudioClip clip, float volume = 1f, bool destroyed = false, float p1 = 0.85f, float p2 = 1.2f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySnd: clip is null", this);
            return;
        }

        AudioSrc.clip = clip;
        AudioSrc.loop = loop;
        AudioSrc.pitch = Random.Range(p1, p2);

        // Рассчитываем корректную начальную громкость с учётом дистанции и видимости
        float initialVolume = CalculateCurrentVolume();

        AudioSrc.volume = initialVolume;
        CurrentVolume = initialVolume;
        TargetVolume = initialVolume;

        AudioSrc.Play();
    }

    // Вспомогательный метод для вычисления громкости прямо сейчас
    protected virtual float CalculateCurrentVolume()
    {
        if (listener == null) return maxVolume;   // или minVolume – зависит от логики, но лучше max т.к. нет слушателя

        float distance = Vector3.Distance(transform.position, listener.transform.position);
        if (distance > maxDistance) return minVolume;

        float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
        float calculatedVolume = Mathf.Lerp(minVolume, maxVolume, distanceVolume);

        bool visible = CheckVisibility(listener.transform.position);
        return visible ? calculatedVolume : minVolume;
    }

    // Перегрузка для вызова без параметров (использует sounds[0] и поля класса)
    public virtual void PlaySnd()
    {
        if (sounds == null || sounds.Length == 0 || sounds[0] == null)
        {
            Debug.LogWarning("Sound.PlaySnd() – no valid sound assigned", this);
            return;
        }
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: minPitch, p2: maxPitch);
    }

    public virtual void StopSnd()
    {
        if (AudioSrc != null && AudioSrc.isPlaying)
            AudioSrc.Stop();
    }

    public void ChangeVolume(float newVolume)
    {
        TargetVolume = Mathf.Clamp(newVolume, minVolume, maxVolume);
    }

    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0.1f, speed);
    }

    // Визуальная отладка в редакторе
    private void OnDrawGizmosSelected()
    {
        if (listener == null && Application.isPlaying == false)
        {
            // Пытаемся найти слушателя для отображения в редакторе (не критично)
            if (Camera.main != null)
                listener = Camera.main.GetComponent<AudioListener>();
        }

        if (listener == null) return;

        Gizmos.color = Color.red;
        Vector3 direction = (listener.transform.position - transform.position).normalized;
        Gizmos.DrawRay(transform.position, direction * maxDistance);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        Gizmos.color = Color.blue;
        float sphereRadius = 0.3f;
        Gizmos.DrawWireSphere(transform.position + direction * maxDistance, sphereRadius);
    }
}