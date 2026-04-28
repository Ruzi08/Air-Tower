using UnityEngine;

public class RadioSound : Sound
{
    void Start()
    {
        
    }

    public void Click()
    {
        AudioSrc.PlayOneShot(sounds[0],volume);
    }

    public void Priem()
    {
        PlaySnd(sounds[1], loop: loop, volume: volume, destroyed: destroyed, p1: minPitch, p2: maxPitch);
    }
}
