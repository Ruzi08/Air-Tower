using UnityEngine;

public class ElectricityFixSound : Sound
{


    void Start()
    {
        
    }
    
    public void PlaySwitchOn()
    {
            PlaySnd(sounds[0], volume, destroyed, minPitch, maxPitch, loop);
    }
    
    public void PlaySwitchOff()
    {
            PlaySnd(sounds[1], volume, destroyed, minPitch, maxPitch, loop);
    }
    
    public void PlayLidOpen()
    {
            PlaySnd(sounds[2], volume, destroyed, minPitch, maxPitch, loop);
    }
    
    public void PlayLidClose()
    {
            PlaySnd(sounds[3], volume, destroyed, minPitch, maxPitch, loop);
    }
}