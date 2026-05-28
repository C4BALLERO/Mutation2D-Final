using System;
using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Core
{
    [CreateAssetMenu(fileName = "SO_AudioData", menuName = "MutationSwarm/Audio Data")]
    public class SO_AudioData : ScriptableObject
    {
        [Serializable]
        public struct AudioEntry
        {
            public string clipKey;
            public AudioClip clip;
        }

        public List<AudioEntry> musicTracks = new();
        public List<AudioEntry> sfxClips = new();
    }
}
