using UnityEngine;
using UnityEngine.Audio;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Audio singleton: música adaptativa, SFX y volúmenes (PROMPT 10).
    /// </summary>
    public class Script_AudioManager : MonoBehaviour
    {
        public static Script_AudioManager Instance { get; private set; }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private SO_AudioData _audioData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            transform.SetParent(null); // Must be root for DontDestroyOnLoad
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySFX(string clipKey, float volume = 1f) { }
        public void PlaySFX(string clipKey, Vector2 position, float volume = 1f) { }
        public void PlayMusic(string trackKey, float fadeTime = 2f) { }
        public void SetVolume(string mixerGroup, float volume) { }
    }
}
