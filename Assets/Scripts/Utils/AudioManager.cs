using UnityEngine;

namespace MutationSwarm
{
    // Generates all game audio procedurally (chiptune / 8-bit arcade style) at runtime.
    // No external audio assets needed — everything is synthesized into AudioClips on Awake.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        const int SR = 44100;

        AudioSource _music, _sfx;
        AudioClip _musicClip, _enemyDeath, _playerDeath, _growl, _shoot, _hurt, _build, _wave, _pickup, _upgrade;
        float _growlCd;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true; _music.playOnAwake = false; _music.volume = 0.28f;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false; _sfx.volume = 0.55f;

            _musicClip   = BuildMusic();
            _enemyDeath  = BuildSweep("enemyDeath", 520f, 90f, 0.22f, 0.40f, 0.30f);
            _playerDeath = BuildSweep("playerDeath", 420f, 50f, 1.10f, 0.50f, 0.18f);
            _growl       = BuildGrowl();
            _shoot       = BuildBlip("shoot", 1100f, 1700f, 0.06f, 0.22f, false);
            _hurt        = BuildBlip("hurt", 320f, 130f, 0.16f, 0.40f, true);
            _build       = BuildBlip("build", 600f, 950f, 0.10f, 0.30f, false);
            _wave        = BuildArpeggio("wave", new[] { 392f, 523.25f, 659.25f, 880f }, 0.09f, 0.30f);
            _pickup      = BuildBlip("pickup", 880f, 1320f, 0.07f, 0.20f, false);
            _upgrade     = BuildArpeggio("upgrade", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.08f, 0.28f);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (_growlCd > 0f) _growlCd -= Time.unscaledDeltaTime;
            if (gm == null) return;

            // Soundtrack plays during active gameplay phases.
            bool active = gm.Phase == GamePhase.Playing || gm.Phase == GamePhase.Building
                       || gm.Phase == GamePhase.Upgrade  || gm.Phase == GamePhase.Paused;
            if (active)
            {
                if (_music.clip != _musicClip) _music.clip = _musicClip;
                if (gm.Phase == GamePhase.Paused) { if (_music.isPlaying) _music.Pause(); }
                else if (!_music.isPlaying) _music.UnPause();
                if (!_music.isPlaying && gm.Phase != GamePhase.Paused) _music.Play();
            }
            else if (_music.isPlaying) _music.Stop();
        }

        // ── Public SFX API ──────────────────────────────────────────────
        public void PlayEnemyDeath()  { if (_sfx) _sfx.PlayOneShot(_enemyDeath, 0.45f); }
        public void PlayPlayerDeath() { if (_sfx) _sfx.PlayOneShot(_playerDeath, 0.85f); }
        public void PlayShoot()       { if (_sfx) _sfx.PlayOneShot(_shoot, 0.16f); }
        public void PlayHurt()        { if (_sfx) _sfx.PlayOneShot(_hurt, 0.45f); }
        public void PlayBuild()       { if (_sfx) _sfx.PlayOneShot(_build, 0.40f); }
        public void PlayWave()        { if (_sfx) _sfx.PlayOneShot(_wave, 0.50f); }
        public void PlayPickup()      { if (_sfx) _sfx.PlayOneShot(_pickup, 0.25f); }
        public void PlayUpgrade()     { if (_sfx) _sfx.PlayOneShot(_upgrade, 0.55f); }

        public void PlayGrowl()
        {
            if (_growlCd > 0f || _sfx == null) return;
            _growlCd = 0.45f; // throttle so a swarm doesn't spam growls
            _sfx.PlayOneShot(_growl, Random.Range(0.28f, 0.5f));
        }

        // ── Procedural synthesis ────────────────────────────────────────
        static float Env(int i, int total, float atk, float rel)
        {
            float t = (float)i / total;
            float a = atk > 0f ? Mathf.Clamp01(t / atk) : 1f;
            float r = rel > 0f ? Mathf.Clamp01((1f - t) / rel) : 1f;
            return a * r;
        }

        static AudioClip Make(string name, float[] data)
        {
            var c = AudioClip.Create(name, data.Length, 1, SR, false);
            c.SetData(data, 0);
            return c;
        }

        // Square-wave blip sweeping f0 -> f1. Optionally adds gritty noise.
        AudioClip BuildBlip(string name, float f0, float f1, float dur, float amp, bool gritty)
        {
            int n = (int)(SR * dur);
            var data = new float[n];
            var rnd = new System.Random(99);
            float phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(f0, f1, t);
                phase += 2f * Mathf.PI * f / SR;
                float sq = Mathf.Sin(phase) >= 0 ? 1f : -1f;
                if (gritty) sq = sq * 0.7f + (float)(rnd.NextDouble() * 2 - 1) * 0.3f;
                data[i] = sq * amp * Env(i, n, 0.08f, 0.35f);
            }
            return Make(name, data);
        }

        // Descending square sweep mixed with noise — explosions / deaths.
        AudioClip BuildSweep(string name, float f0, float f1, float dur, float amp, float noiseMix)
        {
            int n = (int)(SR * dur);
            var data = new float[n];
            var rnd = new System.Random(2024);
            float phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(f0, f1, t * t); // accelerating downward
                phase += 2f * Mathf.PI * f / SR;
                float sq = Mathf.Sin(phase) >= 0 ? 1f : -1f;
                float noise = (float)(rnd.NextDouble() * 2 - 1);
                data[i] = (sq * (1f - noiseMix) + noise * noiseMix) * amp * Env(i, n, 0.02f, 0.55f);
            }
            return Make(name, data);
        }

        // Low, harsh, vibrato'd growl — enemy snarls.
        AudioClip BuildGrowl()
        {
            float dur = 0.34f;
            int n = (int)(SR * dur);
            var data = new float[n];
            var rnd = new System.Random(777);
            float phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = 78f + 22f * Mathf.Sin(t * 38f); // low growl with vibrato
                phase += 2f * Mathf.PI * f / SR;
                float tone = Mathf.Sin(phase) >= 0 ? 1f : -1f;
                float noise = (float)(rnd.NextDouble() * 2 - 1);
                data[i] = (tone * 0.55f + noise * 0.45f) * 0.5f * Env(i, n, 0.12f, 0.45f);
            }
            return Make("growl", data);
        }

        // Quick ascending arpeggio — wave start / upgrade jingles.
        AudioClip BuildArpeggio(string name, float[] notes, float noteDur, float amp)
        {
            int per = (int)(SR * noteDur);
            int n = per * notes.Length;
            var data = new float[n];
            float phase = 0;
            for (int s = 0; s < notes.Length; s++)
            {
                float f = notes[s];
                for (int i = 0; i < per; i++)
                {
                    phase += 2f * Mathf.PI * f / SR;
                    float sq = Mathf.Sin(phase) >= 0 ? 1f : -1f;
                    data[s * per + i] = sq * amp * Env(i, per, 0.06f, 0.25f);
                }
            }
            return Make(name, data);
        }

        // Looping arcade chiptune: driving bass (eighth notes) + melody over Am–F–C–G.
        AudioClip BuildMusic()
        {
            float stepDur = 0.14f;
            int per = (int)(SR * stepDur);

            // 0 = rest
            float[] mel =
            {
                440f, 523.25f, 659.25f, 523.25f, 440f, 0f, 329.63f, 0f,   // Am
                349.23f, 440f, 523.25f, 440f, 349.23f, 0f, 261.63f, 0f,   // F
                329.63f, 392f, 523.25f, 392f, 329.63f, 0f, 261.63f, 0f,   // C
                293.66f, 392f, 493.88f, 392f, 293.66f, 0f, 196f, 0f,      // G
            };
            float[] bass =
            {
                110f,110f,110f,110f, 110f,110f,110f,110f,   // A2
                87.31f,87.31f,87.31f,87.31f, 87.31f,87.31f,87.31f,87.31f, // F2
                130.81f,130.81f,130.81f,130.81f, 130.81f,130.81f,130.81f,130.81f, // C3
                98f,98f,98f,98f, 98f,98f,98f,98f,           // G2
            };

            int steps = mel.Length;
            int n = per * steps;
            var data = new float[n];
            float mp = 0, bp = 0;
            for (int s = 0; s < steps; s++)
            {
                float mf = mel[s], bf = bass[s];
                for (int i = 0; i < per; i++)
                {
                    float e = Env(i, per, 0.05f, 0.18f);
                    float v = 0f;
                    if (mf > 0f) { mp += 2f * Mathf.PI * mf / SR; v += (Mathf.Sin(mp) >= 0 ? 1f : -1f) * 0.16f * e; }
                    if (bf > 0f) { bp += 2f * Mathf.PI * bf / SR; v += (Mathf.Sin(bp) >= 0 ? 1f : -1f) * 0.18f * e; }
                    data[s * per + i] = v;
                }
            }
            return Make("music", data);
        }
    }
}
