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
/// A single instance of the AudioAgent mainly used for multible soundeffects e.g. A player character.
/// </summary>
/// 
public class MultiAudioAgent : AudioAgent
{
    public List<AudioClip> audioClips;
    public uint audioPlayersCount = 5;
    public AudioManager.VolumeChannel channel;

    protected Dictionary<string, AudioClip> audioLibrary;
    protected AudioPlayer[] players;

    protected override void Awake()
    {
        base.Awake();
        UpdateList();

        if (audioPlayersCount != 0)
            players = new AudioPlayer[audioPlayersCount];

        for (int i = 0; i < audioPlayersCount; i++)
        {
            players[i] = new AudioPlayer(gameObject, null, AudioManager.Instance.GetVolume(channel, this) * localVolume);
            players[i].is3D = is3D;
            players[i].Set3DRange(min3D_Dist, max3D_Dist);
        }
    }

    protected override void Update()
    {
        foreach (var player in players)
        {
            if (isMuted)
                player.SetVolume(0.0f);
            else
                player.SetVolume(AudioManager.Instance.GetVolume(channel, this) * localVolume);

            player.is3D = is3D;
            player.Set3DRange(min3D_Dist, max3D_Dist);

            player.Update();
        }
    }

    public void UpdateList()
    {
        if(audioLibrary == null)
            audioLibrary = new Dictionary<string, AudioClip>();

        audioLibrary.Clear();
        foreach (var item in audioClips)
        {
            if(!audioLibrary.ContainsKey(item.name))
                audioLibrary.Add(item.name, item);
        }
    }

    public bool Play(string clipName, bool isLooping = false, float pitch = 1.0f)
    {
        AudioClip clip;
        if (audioLibrary.TryGetValue(clipName, out clip))
        {
            AudioPlayer player = GetAvailablePlayer();
            if(player != null)
            {
                player.SetClip(clip);
                player.SetLooping(isLooping);
                player.SetPitch(pitch);
                player.Play();
                return true;
            }
            Debug.LogWarning($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't have enough players to play: \"{clipName}\".");
            return false;
        }
        Debug.LogError($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't contain \"{clipName}\".");
        return false;
    }

    public bool PlayOnce(string clipName, bool isLooping = false, float pitch = 1.0f)
    {
        AudioClip clip;
        if (audioLibrary.TryGetValue(clipName, out clip))
        {
            AudioPlayer player = GetAvailablePlayer();
            if (player != null)
            {
                //if(!IsAudioPlaying(clipName))
                {
                    player.SetClip(clip);
                    player.SetLooping(isLooping);
                    player.SetPitch(pitch);
                    player.Play();
                    return true;
                }
                //return false;
            }
            Debug.LogWarning($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't have enough players to play: \"{clipName}\".");
            return false;
        }
        Debug.LogError($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't contain \"{clipName}\".");
        return false;
    }

    public void StopAudio(string clipName)
    {
        foreach (var player in players)
        {
            if (player.IsPlaying() && player.currentClip?.name == clipName)
            {
                player.Stop();
            }
        }
    }

    public bool IsAudioPlaying(string clipName)
    {
        foreach (var player in players)
        {
            if (player.IsPlaying() && player.currentClip?.name == clipName)
            {
                return true;
            }
        }
        return false;
    }

    private AudioPlayer GetAvailablePlayer()
    {
        foreach (var player in players)
        {
            if(!player.IsPlaying())
            {
                return player;
            }
        }
        return null;
    }

    public bool PlayDelayed(string clipName, float delay = 1.0f, bool isLooping = false, float pitch = 1.0f)
    {
        AudioClip clip;
        if (audioLibrary.TryGetValue(clipName, out clip))
        {
            AudioPlayer player = GetAvailablePlayer();
            if (player != null)
            {
                player.SetClip(clip);
                player.SetLooping(isLooping);
                player.SetPitch(pitch);
                StartCoroutine(player.PlayDelayed(delay));
                
                return true;
            }
            Debug.LogWarning($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't have enough players to play: \"{clipName}\".");
            return false;
        }
        Debug.LogError($"MultiAudioAgent on gameObject: \"{gameObject.name}\" doesn't contain \"{clipName}\".");
        return false;
    }
}
