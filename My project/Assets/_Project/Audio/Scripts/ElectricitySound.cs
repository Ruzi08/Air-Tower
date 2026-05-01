using Unity.VisualScripting;
using UnityEngine;

public class ElectricitySound : Sound
{
    [SerializeField] PowerManager powerManager;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if (powerManager.hasPower && !isPlaying )
        {
            Debug.Log("Electricity Sound On");
            PlaySnd();
        }
        if (!powerManager.hasPower)
        {
            StopSnd();
        }
    }
}
