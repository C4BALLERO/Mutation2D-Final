using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm
{
    [System.Serializable]
    public struct SpawnEntry { public GeneType gene; public bool spawnLeft; public float delay; }

    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        public GameObject enemyPrefab;
        public Transform  enemiesContainer;
        public float      spawnX = 14f;
        public float      spawnY = 0f;

        public WaveStats       CurrentStats  { get; private set; } = new();
        public List<EnemyBase> ActiveEnemies { get; private set; } = new();

        List<SpawnEntry> _queue = new();
        float _elapsed;
        bool  _spawning;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // The run is started from the main menu via GameManager.StartGame().

        public void StartNextWave()
        {
            CurrentStats.Reset();
            GameManager.Instance.StartWave();
            ActiveEnemies.Clear();
            _queue  = BuildQueue(GameManager.Instance.WaveNum);
            _elapsed = 0f;
            _spawning = true;
        }

        void Update()
        {
            if (!_spawning || GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;

            _elapsed += Time.deltaTime;

            for (int i = _queue.Count - 1; i >= 0; i--)
            {
                var entry = _queue[i];
                if (_elapsed >= entry.delay)
                {
                    SpawnEnemy(entry.gene, entry.spawnLeft);
                    _queue.RemoveAt(i);
                }
            }

            EvolutionSystem.Instance?.UpdateDominant(ActiveEnemies);

            if (_queue.Count == 0 && ActiveEnemies.Count == 0 && _spawning)
            {
                _spawning = false;
                PlayerStats.Instance.AddDna(GameManager.Instance.WaveNum * 15 + 20);
                EvolutionSystem.Instance?.Evolve(CurrentStats, GameManager.Instance.WaveNum);
                GameManager.Instance.OnWaveComplete();
            }
        }

        List<SpawnEntry> BuildQueue(int waveNum)
        {
            var q = new List<SpawnEntry>();
            int count = waveNum * 5 + 5;
            float intervalBase = Mathf.Max(0.5f, 1.5f - waveNum * 0.05f);
            for (int i = 0; i < count; i++)
            {
                q.Add(new SpawnEntry
                {
                    gene      = EvolutionSystem.Instance?.RollGene() ?? GeneType.None,
                    spawnLeft = Random.value > 0.5f,
                    delay     = i * intervalBase,
                });
            }
            return q;
        }

        void SpawnEnemy(GeneType gene, bool spawnLeft)
        {
            float x = spawnLeft ? -spawnX : spawnX;
            var go = Instantiate(enemyPrefab, new Vector3(x, spawnY, 0), Quaternion.identity, enemiesContainer);
            go.SetActive(true);
            var e  = go.GetComponent<EnemyBase>();
            int wn = GameManager.Instance.WaveNum;

            // Base stats
            float hp    = 50f + wn * 8f;
            float spd   = 2.5f + wn * 0.1f;
            float dmg   = 8f  + wn * 1.5f;
            float armor = 1f;
            bool flies = false, slow = false, poison = false, spiny = false, corrupt = false;
            var extra = new GeneType[0];

            switch (gene)
            {
                case GeneType.Poison:  hp *= 1.1f; poison = true; break;
                case GeneType.Speed:   hp *= 1.2f; spd *= 2f; break;
                case GeneType.Spiny:   hp *= 1.4f; spiny = true; dmg *= 1.5f; break;
                case GeneType.Armored: hp *= 1.8f; armor = 0.35f; spd *= 0.8f; break;
                case GeneType.Psychic: hp *= 1.3f; flies = true; slow = true; spd *= 0.9f; break;
                case GeneType.Corrupt:
                    hp *= 3.5f; spd *= 1.4f; dmg *= 2f; corrupt = true;
                    var extraList = new List<GeneType>();
                    GeneType r1 = RandGene(), r2 = RandGene();
                    extraList.Add(r1); extraList.Add(r2); extra = extraList.ToArray();
                    poison = r1 == GeneType.Poison || r2 == GeneType.Poison;
                    spiny  = r1 == GeneType.Spiny  || r2 == GeneType.Spiny;
                    armor  = (r1 == GeneType.Armored || r2 == GeneType.Armored) ? 0.4f : 1f;
                    flies  = r1 == GeneType.Psychic || r2 == GeneType.Psychic;
                    slow   = flies;
                    break;
            }

            e.Init(gene, extra, hp, spd, dmg, armor, flies, slow, poison, spiny, corrupt);
            ActiveEnemies.Add(e);
        }

        static GeneType RandGene()
        {
            var g = new[] { GeneType.Poison, GeneType.Speed, GeneType.Spiny, GeneType.Armored, GeneType.Psychic };
            return g[Random.Range(0, g.Length)];
        }

        public void RemoveEnemy(EnemyBase e) => ActiveEnemies.Remove(e);
    }
}
