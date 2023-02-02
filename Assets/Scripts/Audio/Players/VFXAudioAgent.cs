using UnityEngine;

public class VFXAudioAgent : SoloAudioAgent
{
    private bool hasStarted = false;
    public void Play(AudioClip clip)
    {
        mainClip = clip;
        hasStarted = true;
        base.Play();
    }

    protected override void Update()
    {
        base.Update();

        if(!player.IsPlaying() && hasStarted)
        {
            Destroy(gameObject);
        }
    }
}
