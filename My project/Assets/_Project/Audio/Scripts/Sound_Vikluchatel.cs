using UnityEngine;

public class SoundVikluchatel : Sound
{
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
    
    public void PlayTurnOnSound()
    {
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
        {
            PlaySnd(sounds[0], volume: volume, loop: false, p1: minPitch, p2: maxPitch);
        }
        else
        {
            Debug.LogWarning("Sound 0 is not assigned!", this);
        }
    }
    
    public void PlayTurnOffSound()
    {
        if (sounds != null && sounds.Length > 1 && sounds[1] != null)
        {
            PlaySnd(sounds[1], volume: volume, loop: false, p1: minPitch, p2: maxPitch);
        }
        else
        {
            Debug.LogWarning("Sound 1 is not assigned!", this);
        }
    }
}