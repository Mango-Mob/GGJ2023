using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    public class VolumeSync : MonoBehaviour
    {
        public AudioManager.VolumeChannel m_myChannel;

        private AudioSource m_source;
        private void Awake()
        {
            m_source = GetComponent<AudioSource>();
        }
        public void Update()
        {
            m_source.volume = AudioManager.Instance.GetVolume(m_myChannel, null);
        }
    }
}
