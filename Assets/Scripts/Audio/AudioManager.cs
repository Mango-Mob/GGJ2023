using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

///
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// A global singleton used to handle all listeners and audio agents within the current scene, by
/// containing the global volume settings.
/// 
/// Note: Agents/Listeners are incharge of being added/removed when they are awake/destroyed. 
/// </summary>
/// 
public class AudioManager : SingletonPersistent<AudioManager>
{
    //Agent and listener lists:
    public List<AudioAgent> agents { get; private set; } = new List<AudioAgent>();
    public List<ListenerAgent> listeners { get; private set; } = new List<ListenerAgent>();

    public List<IntRange> m_clipRanges;
    public List<AudioClip> m_backgroundMusic;

    public JukeboxAgent m_mainPlayer;
    public JukeboxAgent m_backPlayer;
    public JukeboxAgent m_combatPlayer;

    //private array of volumes
    public float[] volumes;
    public float m_globalPitch = 1.0f;

    private int lastLoadedScene = -1;

    //Volume types: 
    //(Add more to dynamically expand the above array)
    public enum VolumeChannel
    {
        MASTER,
        SOUND_EFFECT,
        MUSIC,
    }

    /// <summary>
    /// Called imediately after creation in the constructor
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        volumes = new float[Enum.GetNames(typeof(AudioManager.VolumeChannel)).Length];
        for (int i = 0; i < volumes.Length; i++)
        {
            volumes[i] = PlayerPrefs.GetFloat($"volume{i}", 1.0f);
        }
    }

    public void Update()
    {
        if(GameManager.HasInstance() && GameManager.Instance.IsInCombat && !m_combatPlayer.IsPlaying())
        {
            m_combatPlayer.Shuffle();
            m_combatPlayer.Play();
        }
        else if(GameManager.HasInstance() && !GameManager.Instance.IsInCombat && m_combatPlayer.IsPlaying())
        {
            m_combatPlayer.Stop();
            m_mainPlayer.Play();
        }
    }

    private void Start()
    {
        LoadMusic();
    }

    public void OnLevelWasLoaded(int level)
    {
        LoadMusic();
    }

    private void LoadMusic()
    {
        if (SceneManager.GetActiveScene().buildIndex == lastLoadedScene)
            return;

        lastLoadedScene = SceneManager.GetActiveScene().buildIndex;

        if (m_mainPlayer.currentlyPlaying)
            m_mainPlayer.Stop();

        //Switch players;
        var temp = m_mainPlayer;
        m_mainPlayer = m_backPlayer;
        m_backPlayer = temp;

        int select = SceneManager.GetActiveScene().buildIndex;
        m_mainPlayer.audioClips.Clear();
        for (int i = m_clipRanges[select].min; i < m_clipRanges[select].max + 1; i++)
        {
            m_mainPlayer.audioClips.Add(m_backgroundMusic[i]);
        }

        m_mainPlayer.ResetOrder();
        m_mainPlayer.Shuffle();
        m_mainPlayer.Play();
    }

    public void SaveData()
    {
        for (int i = 0; i < volumes.Length; i++)
        {
            PlayerPrefs.SetFloat($"volume{i}", volumes[i]);
        }
    }

    private void OnDestroy()
    {
        SaveData();  
    }

    /// <summary>
    /// Gets the volume of the type additionally based on the agent's location
    /// </summary>
    /// <param name="type">Volume type to accurately base calculate the volume on.</param>
    /// <param name="agent">The agent to base the 3d volume on. Use NULL instead for gobal volume.</param>
    /// <returns> volume data between 0.0f and 1.0f </returns>
    public float GetVolume(VolumeChannel type, AudioAgent agent)
    {
        if (type == VolumeChannel.MASTER)
            return volumes[(int)VolumeChannel.MASTER] * CalculateHearingVolume(agent);

        return volumes[(int)VolumeChannel.MASTER] * volumes[(int)type] * CalculateHearingVolume(agent);
    }

    /// <summary>
    /// Makes the agent the only one playing with volume, the others will be muted. 
    /// </summary>
    /// <param name="_agent">Agent to prioritise.</param>
    public void MakeSolo(AudioAgent _agent)
    {
        if (_agent == null) //Edge case
        {
            Debug.LogError($"Agent attempting to be solo is null, ignored function call");
            return;
        }
        
        //Mute all other agents
        foreach (var agent in agents)
        {
            agent.SetMute(true);
        }

        //Unmute param agent
        _agent.SetMute(false);
    }

    /// <summary>
    /// Unmutes all agents within the scene.
    /// </summary>
    public void UnMuteAll()
    {
        foreach (var agent in agents)
        {
            agent.SetMute(false);
        }
    }

    /// <summary>
    /// Calculates the 3D volume based on the distance from all listeners.
    /// </summary>
    /// <param name="agent">Agent to get the volume of. use NULL for global volume.</param>
    /// <returns>Largest volume returned from all listener calculations.</returns>
    private float CalculateHearingVolume(AudioAgent agent)
    {
        if (listeners.Count == 0) //Edge case
            return 1.0f;

        if (agent == null) //Edge case
            return 1.0f;

        float max = 0.0f;
        foreach (var listener in listeners)
        {
            max = Mathf.Max(listener.CalculateHearingVol(agent.transform.position), max);
        }
        return max;
    }

    public void PlayAudioTemporary(Vector3 _position, AudioClip _clip, VolumeChannel _channel = VolumeChannel.SOUND_EFFECT, float range = -1.0f, float _localVolume = 1.0f)
    {
        GameObject temp = new GameObject();
        temp.transform.position = _position;

        VFXAudioAgent agent = temp.AddComponent<VFXAudioAgent>();
        agent.is3D = range > 0;
        agent.max3D_Dist = range;
        agent.channel = _channel;
        agent.localVolume = _localVolume;
        agent.Play(_clip);
    }
}
