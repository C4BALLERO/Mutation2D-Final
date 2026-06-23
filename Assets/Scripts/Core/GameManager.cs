using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm
{
    public enum GamePhase { Splash, Story, Menu, Playing, Upgrade, Dead, Paused, Building }

    [System.Serializable]
    public class UpgradeOption
    {
        public string id;
        public string name;
        public string desc;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GamePhase Phase { get; private set; } = GamePhase.Splash;
        public int WaveNum { get; private set; }
        public int BestScore { get; private set; }
        public int StoryPage { get; private set; }
        public const int StoryPagesCount = 5;
        public List<UpgradeOption> UpgradeOptions { get; private set; } = new();

        static readonly UpgradeOption[] AllUpgrades =
        {
            new() { id="piercing",     name="Balas Perforantes", desc="Las balas atraviesan 2 enemigos" },
            new() { id="electric",     name="Municion Electrica", desc="Cada 5ta bala encadena enemigos cercanos" },
            new() { id="dashExplosive",name="Dash Explosivo",    desc="El dash deja rastro de explosiones" },
            new() { id="drone",        name="Dron Acompanante", desc="Un dron orbita y dispara enemigos" },
            new() { id="regen",        name="Regeneracion",     desc="+2 HP por segundo" },
            new() { id="fastBuild",    name="Constructor Rapido", desc="Defensas colocadas mas rapido" },
            new() { id="moreDamage",   name="Mas Dano",         desc="+8 dano por bala" },
            new() { id="fasterReload", name="Recarga Rapida",   desc="Disparo mucho mas rapido" },
            new() { id="moreHp",       name="Mas Vida Maxima",  desc="+40 HP maximo y curacion" },
        };

        static bool _autoStart;
        float _splashTimer = 3f;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1f;
            BestScore = PlayerPrefs.GetInt("ms_best", 0);
        }

        void Start()
        {
            // After a restart skip splash/story and jump straight back into a run.
            if (_autoStart) { _autoStart = false; Phase = GamePhase.Menu; StartGame(); }
        }

        void Update()
        {
            if (Phase == GamePhase.Splash)
            {
                _splashTimer -= Time.deltaTime;
                if (_splashTimer <= 0f) GoToStory();
            }
        }

        public void SkipSplash()
        {
            if (Phase == GamePhase.Splash) GoToStory();
        }

        void GoToStory()
        {
            StoryPage = 0;
            Phase = GamePhase.Story;
        }

        public void AdvanceStory()
        {
            if (Phase != GamePhase.Story) return;
            StoryPage++;
            if (StoryPage >= StoryPagesCount)
            {
                StoryPage = 0;
                Phase = GamePhase.Menu;
            }
        }

        // Begins a fresh run (from the main menu or after a restart).
        public void StartGame()
        {
            if (Phase != GamePhase.Menu) return;
            Phase = GamePhase.Playing;
            WaveNum = 0;
            Time.timeScale = 1f;
            WaveManager.Instance?.StartNextWave();
        }

        public void StartWave()
        {
            WaveNum++;
            AudioManager.Instance?.PlayWave();
            Phase = GamePhase.Playing;
        }

        public void OnWaveComplete()
        {
            UpgradeOptions = PickUpgrades();
            Phase = GamePhase.Upgrade;
        }

        public void ApplyUpgrade(int idx)
        {
            if (idx < 0 || idx >= UpgradeOptions.Count) return;
            var up = UpgradeOptions[idx];
            var stats = PlayerStats.Instance;
            stats.AddUpgrade(up.id);
            if (up.id == "moreHp") { stats.MaxHp += 40; stats.Heal(40); }
            AudioManager.Instance?.PlayUpgrade();
            WaveManager.Instance.StartNextWave();
            Phase = GamePhase.Playing;
        }

        public void PlayerDied()
        {
            if (WaveNum > BestScore)
            {
                BestScore = WaveNum;
                PlayerPrefs.SetInt("ms_best", BestScore);
            }
            AudioManager.Instance?.PlayPlayerDeath();
            Phase = GamePhase.Dead;
        }

        public void Restart()
        {
            _autoStart = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TogglePause()
        {
            if (Phase == GamePhase.Playing)   { Phase = GamePhase.Paused; Time.timeScale = 0f; }
            else if (Phase == GamePhase.Paused){ Phase = GamePhase.Playing; Time.timeScale = 1f; }
        }

        public void ToggleBuild()
        {
            if (Phase == GamePhase.Playing)  Phase = GamePhase.Building;
            else if (Phase == GamePhase.Building) Phase = GamePhase.Playing;
        }

        List<UpgradeOption> PickUpgrades()
        {
            var available = new List<UpgradeOption>();
            var owned = PlayerStats.Instance.Upgrades;
            foreach (var up in AllUpgrades)
            {
                bool repeatable = up.id is "moreDamage" or "fasterReload" or "moreHp";
                if (!owned.Contains(up.id) || repeatable) available.Add(up);
            }
            available.Sort((_, __) => Random.value > 0.5f ? 1 : -1);
            return available.Count >= 3 ? available.GetRange(0, 3) : available;
        }
    }
}
