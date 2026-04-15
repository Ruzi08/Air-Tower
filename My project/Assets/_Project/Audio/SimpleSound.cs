using UnityEngine;

public class SimpleSound : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ЗВУКА ===")]
    public AudioClip clip;
    public float volume = 0.7f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public bool loop = false;
    public float maxDistance = 15f;
    
    private AudioSource audioSource;
    private Transform listener;
    private float originalVolume;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.maxDistance = maxDistance;
        audioSource.spatialBlend = 1f; // 3D звук
        
        originalVolume = volume;
        
        // Находим слушателя (камера игрока)
        Camera cam = Camera.main;
        if (cam != null)
            listener = cam.transform;
    }
    
    void Update()
    {
        if (listener == null) return;
        
        // Расчёт громкости от расстояния
        float distance = Vector3.Distance(transform.position, listener.position);
        float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
        
        // Проверка видимости (рейкаст до игрока)
        bool canHear = CheckVisibility();
        
        audioSource.volume = canHear ? originalVolume * distanceVolume : 0;
    }
    
    private bool CheckVisibility()
    {
        if (listener == null) return false;
        
        Vector3 direction = (listener.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, listener.position);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            // Если луч упёрся в игрока — звук слышен
            if (hit.transform == listener || hit.transform.root.CompareTag("Player"))
                return true;
            
            // Иначе стена блокирует звук
            return false;
        }
        
        return true;
    }
    
    public void Play()
    {
        if (clip == null) return;
        
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
    }
    
    public void Stop()
    {
        audioSource.Stop();
    }
    
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}