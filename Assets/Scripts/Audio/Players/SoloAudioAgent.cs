using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// 
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// A single instance of the AudioAgent mainly used for easy event access for animations.
/// </summary>
/// 
public class SoloAudioAgent : AudioAgent
{
    [Header("Solo Settings:")]
    public AudioClip mainClip;
    public AudioManager.VolumeChannel channel;
    
    public bool isPlayOnAwake = false;
    public bool isLooping = false;

    protected AudioPlayer player = null;

    protected override void Awake()
    {
        base.Awake();
        player = new AudioPlayer(this.gameObject, mainClip, AudioManager.Instance.GetVolume(channel, this) * localVolume);
        player.is3D = is3D;
        player.Set3DRange(min3D_Dist, max3D_Dist);
        Update();

        if (isPlayOnAwake && this.isActiveAndEnabled)
        {
            if (isLooping)
                PlayLooping();
            else
                Play();
        }
    }

    protected override void Update()
    {
        player.is3D = is3D;
        player.Set3DRange(min3D_Dist, max3D_Dist);

        if (isMuted)
            player.SetVolume(0.0f);
        else
            player.SetVolume(AudioManager.Instance.GetVolume(channel, this) * localVolume);

        player.Update();
    }

    /// <summary>
    /// Play the audio clip once
    /// </summary>
    public void Play() 
    {
        player.SetClip(mainClip);
        player.SetLooping(isLooping);
        player.Play();
    }

    /// <summary>
    /// Play the audio clip in a looping
    /// </summary>
    public void PlayLooping()
    {
        player.SetLooping(true);
        player.Play();
    }

    public void SetClip(AudioClip audioClip)
    {
        mainClip = audioClip;
        player.SetClip(mainClip);
    }

    /// <summary>
    /// Play the audio clip alone
    /// </summary>
    public void PlaySolo()
    {
        AudioManager.Instance.MakeSolo(this);
    }

    /// <summary>
    /// Play the audio clip with random pitch between 0.75 and 1.25
    /// </summary>
    public void PlayWithRandomPitch()
    {
        if(!player.IsPlaying())
        {
            player.SetPitch(UnityEngine.Random.Range(0.75f, 1.25f));
            player.Play();
        }
    }

    /// <summary>
    /// Play the audio clip with a fade in
    /// </summary>
    public void PlayWithFadeIn(float fadeInTime = 0.25f)
    {
        if(!player.isMutating)
        {
            player.SetVolume(0.0f);
            player.Play();
            StartCoroutine(player.FadeIn(fadeInTime));
        }
    }

    /// <summary>
    /// Stop the audio clip
    /// </summary>
    public void Stop()
    {
        player.Stop(); 
        player.SetPitch(1.0f); 
    }

    /// <summary>
    /// Pause the audio clip
    /// </summary>
    public void Pause() { player.Pause(); }

    public bool IsPlaying()
    {
        return player.IsPlaying();
    }

    /// <summary>
    /// Pause the audio clip with a fade out
    /// </summary>
    public void PauseWithFadeOut(float fadeInTime = 0.25f)
    {
        if (!player.isMutating)
        {
            StartCoroutine(player.FadeOut(fadeInTime));
        }
    }
}
