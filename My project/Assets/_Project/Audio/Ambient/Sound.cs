using UnityEngine;

public class Sound : MonoBehaviour
{   
    public bool loop;
    public bool destroyed;
    public float MinPitch;
    public float MaxPitch;
    public AudioClip[] sounds;
    public bool isPlaying;
    public AudioListener listener;
    public float minVolume;
    public float maxVolume;
    public float maxDistance = 20f; // Максимальная дистанция слышимости
    public float fadeSpeed = 2f;    // Скорость затухания/нарастания
    protected float volume;
    protected AudioSource audioSrc;
    protected bool IsSoundAvable;
    protected float currentVolume;    // Текущая плавно изменяемая громкость
    protected float targetVolume;     // Целевая громкость
    
    protected virtual void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null)
            audioSrc = gameObject.AddComponent<AudioSource>();
        volume = maxVolume;
        currentVolume = minVolume;
        targetVolume = minVolume;
    }
    
    protected virtual void Start()
    {
        PlaySnd(sounds[0], loop: loop, volume: volume, destroyed: destroyed, p1: MinPitch, p2: MaxPitch);
    }

    protected virtual void Update()
{
    if (listener == null) return;
    
    // Расчет расстояния до слушателя
    float distance = Vector3.Distance(transform.position, listener.transform.position);
    
    // Если игрок вне зоны слышимости - выходим
    if (distance > maxDistance)
    {
        targetVolume = minVolume;
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * fadeSpeed);
        audioSrc.volume = currentVolume;
        return;
    }
    
    // Расчет громкости на основе расстояния
    float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
    float calculatedVolume = Mathf.Lerp(minVolume, maxVolume, distanceVolume);
    
    // Проверяем видимость
    IsSoundAvable = CheckVisibility(listener.transform.position);
    
    if (IsSoundAvable)
    {
        targetVolume = calculatedVolume;
    }
    else
    {
        targetVolume = minVolume;
    }
    
    // Плавное изменение громкости
    currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * fadeSpeed);
    audioSrc.volume = currentVolume;
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
        audioSrc.clip = clip;
        audioSrc.loop = loop;
        audioSrc.pitch = Random.Range(p1, p2);
        audioSrc.volume = volume;
        audioSrc.Play();
        
        // Инициализируем текущую громкость
        currentVolume = volume;
        targetVolume = volume;
    }
    
    public void ChangeVolume(float newVolume)
    {
        targetVolume = Mathf.Clamp(newVolume, minVolume, maxVolume);
    }
    
    public virtual void StopSnd()
    {
        audioSrc.Stop();
    }
    
    // Дополнительный метод для установки скорости затухания
    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0.1f, speed);
    }
}