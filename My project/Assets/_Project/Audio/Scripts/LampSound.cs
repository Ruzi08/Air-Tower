public class LampSound : Sound
{
    // Флаги для цикличных звуков (как было ранее)
    private bool isGreenLoopPlaying = false;
    private bool isYellowPlaying = false;
    private bool isRedPlaying = false;

    protected override void Start()
    {
        
    }
    
    protected override void Awake()
    {
        base.Awake();
        loop = false;   // по умолчанию не зациклено
    }

    public void PlayGreenOneShot()
    {
        if (sounds != null && sounds.Length > 0 && sounds[0] != null)
        {
            // Однократное воспроизведение
            PlaySnd(sounds[0], volume, destroyed, 1, 1, false);
        }
    }

    public void PlayYellowLoop()
    {
        if (isYellowPlaying) return;
        StopAll();
        if (sounds != null && sounds.Length > 1 && sounds[1] != null)
        {
            PlaySnd(sounds[1], volume, destroyed, 1, 1, true);
            isYellowPlaying = true;
        }
    }

    public void PlayRedLoop()
    {
        if (isRedPlaying) return;
        StopAll();
        if (sounds != null && sounds.Length > 2 && sounds[2] != null)
        {
            PlaySnd(sounds[2], volume, destroyed, 1, 1, true);
            isRedPlaying = true;
        }
    }

    public void StopGreen()   // для зелёного цикла (не используется в однократном режиме, но на всякий случай)
    {
        if (!isGreenLoopPlaying) return;
        if (AudioSrc != null && AudioSrc.isPlaying && AudioSrc.clip == sounds[0])
            StopSnd();
        isGreenLoopPlaying = false;
    }

    public void StopYellow()
    {
        if (!isYellowPlaying) return;
        if (AudioSrc != null && AudioSrc.isPlaying && AudioSrc.clip == sounds[1])
            StopSnd();
        isYellowPlaying = false;
    }

    public void StopRed()
    {
        if (!isRedPlaying) return;
        if (AudioSrc != null && AudioSrc.isPlaying && AudioSrc.clip == sounds[2])
            StopSnd();
        isRedPlaying = false;
    }

    private void StopAll()
    {
        if (AudioSrc != null && AudioSrc.isPlaying)
            StopSnd();
        isGreenLoopPlaying = false;
        isYellowPlaying = false;
        isRedPlaying = false;
    }
}