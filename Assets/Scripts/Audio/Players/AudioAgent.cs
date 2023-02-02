using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// An abstract parent class for audio agents.
/// </summary>
/// 
public abstract class AudioAgent : MonoBehaviour
{
    [Header("Parent Settings:")] //Local settings
    [Range(0.0f, 1.0f)]
    public float localVolume = 1f;
    [Tooltip("Mutes this agent completely.")]
    public bool isMuted = false;
    public bool is3D = false;

    public float min3D_Dist = 1f;
    public float max3D_Dist = 500f;

    protected virtual void Awake()
    {
        AudioManager.Instance.agents.Add(this);

        if(isMuted)
            Debug.LogWarning($"Audio agent is muted on awake, location: {gameObject.name}.");
    }

    protected abstract void Update();

    public virtual void SetMute(bool status) { isMuted = status; }

    protected virtual void OnDestroy()
    {
        AudioManager.Instance?.agents.Remove(this);
    }
}
