using UnityEngine;

public class SimpleSound : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ЗВУКА ===")]
    public AudioClip clip;
    public float volume = 0.7f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public bool loop = false;
    public float maxDistance = 50f;
    
    [Header("=== ПРИГЛУШЕНИЕ ЗА СТЕНАМИ ===")]
    [Range(0, 1)] public float wallMuffle = 0.3f;
    
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
        audioSource.spatialBlend = 1f;
        
        originalVolume = volume;
        
        Camera cam = Camera.main;
        if (cam != null)
            listener = cam.transform;
    }
    
    void Update()
    {
        if (listener == null) return;
        if (!audioSource.isPlaying) return;
        
        float distance = Vector3.Distance(transform.position, listener.position);
        float distanceVolume = Mathf.Clamp01(1 - (distance / maxDistance));
        float wallFactor = IsDirectlyVisible() ? 1f : wallMuffle;
        
        audioSource.volume = originalVolume * distanceVolume * wallFactor;
    }
    
    private bool IsDirectlyVisible()
    {
        if (listener == null) return true;
        
        Vector3 direction = (listener.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, listener.position);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            if (hit.transform == listener || hit.transform.root.CompareTag("Player"))
                return true;
            return false;
        }
        
        return true;
    }
    
    public void Play()
    {
        if (clip == null)
        {
            Debug.LogWarning($"SimpleSound: нет AudioClip на {gameObject.name}");
            return;
        }
        
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.loop = loop;
        audioSource.Play();
        Debug.Log($"🔊 SimpleSound.Play() на {gameObject.name}, loop={loop}");
    }
    
    public void Stop()
    {
        audioSource.Stop();
        Debug.Log($"🔇 SimpleSound.Stop() на {gameObject.name}");
    }
    
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}