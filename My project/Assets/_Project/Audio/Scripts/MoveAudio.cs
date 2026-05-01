using UnityEngine;

public class MoveAudio : MonoBehaviour
{
    [SerializeField] GameObject PrefabAudio;

    [SerializeField] private Transform point;
    
    [SerializeField] AudioClip[] _audioClips;
    [SerializeField] private int time, Movetime;

    
    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (time != 0) return;
            int I = Random.Range(0, _audioClips.Length);
            time = Movetime;
            
            GameObject temp = Instantiate(PrefabAudio, point);
            temp.GetComponent<AudioSource>().clip = _audioClips[I];
            temp.GetComponent<AudioSource>().Play();
            Destroy(temp, 1);
        }
    }

    private void FixedUpdate()
    {
        if (time > 0)
        {
            time--;
        }
    }
}
