using UnityEngine;

public class Sound : MonoBehaviour
{   
    public bool loop;
    public bool destroyed;
    public float minPitch;
    public float maxPitch;
    public AudioClip[] sounds;
    public bool isPlaying;
    public AudioListener listener;
    public float minVolume;
    public float maxVolume;
    public float maxDistance = 20f; // Максимальная дистанция слышимости
    public float fadeSpeed = 2f;    // Скорость затухания/нарастания
    [SerializeField] protected float volume;
    protected AudioSource AudioSrc;
    protected bool IsSoundAvailable;
    protected float CurrentVolume;    // Текущая плавно изменяемая громкость
    protected float TargetVolume;     // Целевая громкость
    
    protected virtual void Awake()
    {
        AudioSrc = GetComponent<AudioSource>();
        if (AudioSrc == null)
            AudioSrc = gameObject.AddComponent<AudioSource>();
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
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: minPitch, p2: maxPitch);
    }

    protected virtual void Update()
{
    if (listener == null) return;
    
    // Расчет расстояния до слушателя
    float distance = Vector3.Distance(transform.position, listener.transform.position);
    
    // Если игрок вне зоны слышимости - выходим
    if (distance > maxDistance)
    {
        TargetVolume = minVolume;
        CurrentVolume = Mathf.Lerp(CurrentVolume, TargetVolume, Time.deltaTime * fadeSpeed);
        AudioSrc.volume = CurrentVolume;
        return;
    }
    
    // Расчет громкости на основе расстояния
    float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
    float calculatedVolume = Mathf.Lerp(minVolume, maxVolume, distanceVolume);
    
    // Проверяем видимость
    IsSoundAvailable = CheckVisibility(listener.transform.position);
    
    if (IsSoundAvailable)
    {
        TargetVolume = calculatedVolume;
    }
    else
    {
        TargetVolume = minVolume;
    }
    
    // Плавное изменение громкости
    CurrentVolume = Mathf.Lerp(CurrentVolume, TargetVolume, Time.deltaTime * fadeSpeed);
    AudioSrc.volume = CurrentVolume;
}

protected virtual bool CheckVisibility(Vector3 targetPosition)
{
    Vector3 direction = (targetPosition - transform.position).normalized;
    float distance = Vector3.Distance(transform.position, targetPosition);
    
    // Используем SphereCast для более надежной проверки
    float sphereRadius = 0.3f; // Радиус сферы для проверки
    
    if (Physics.SphereCast(transform.position, sphereRadius, direction, out RaycastHit hit, distance))
    {
        // Проверяем, достиг ли луч игрока или объект с тегом Player
        if (hit.transform.CompareTag("Player") || 
            hit.transform == listener.transform ||
            hit.transform.root.CompareTag("Player"))
        {
            Debug.DrawRay(transform.position, direction * distance, Color.green);
            return true;
        }
        
        // Дополнительная проверка: может луч прошел мимо, но игрок рядом
        // Проверяем расстояние до игрока от точки попадания
        float distanceToPlayer = Vector3.Distance(hit.point, targetPosition);
        if (distanceToPlayer < sphereRadius)
        {
            Debug.DrawRay(transform.position, direction * distance, Color.yellow);
            return true;
        }
        
        Debug.DrawRay(transform.position, direction * distance, Color.red);
        return false;
    }
    
    // Если луч ничего не задел, значит есть прямая видимость
    Debug.DrawRay(transform.position, direction * distance, Color.green);
    return true;
}

// Визуализация для отладки
void OnDrawGizmosSelected()
{
    if (listener == null) return;
    
    Gizmos.color = Color.red;
    Vector3 direction = (listener.transform.position - transform.position).normalized;
    Gizmos.DrawRay(transform.position, direction * maxDistance);
    
    // Визуализация радиуса слышимости
    Gizmos.color = new Color(1, 0, 0, 0.3f);
    Gizmos.DrawWireSphere(transform.position, maxDistance);
    
    // Визуализация SphereCast
    Gizmos.color = Color.blue;
    float sphereRadius = 0.3f;
    Gizmos.DrawWireSphere(transform.position + direction * maxDistance, sphereRadius);
}
    
    public virtual void PlaySnd(AudioClip clip, float volume = 1f, bool destroyed = false, float p1 = 0.85f, float p2 = 1.2f, bool loop = false)
    {   
        AudioSrc.clip = clip;
        AudioSrc.loop = loop;
        AudioSrc.pitch = Random.Range(p1, p2);
        AudioSrc.volume = volume;
        AudioSrc.Play();
        
        // Инициализируем текущую громкость
        CurrentVolume = volume;
        TargetVolume = volume;
    }
    
    public void ChangeVolume(float newVolume)
    {
        TargetVolume = Mathf.Clamp(newVolume, minVolume, maxVolume);
    }
    
    public virtual void StopSnd()
    {
        AudioSrc.Stop();
    }
    
    // Дополнительный метод для установки скорости затухания
    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0.1f, speed);
    }
}