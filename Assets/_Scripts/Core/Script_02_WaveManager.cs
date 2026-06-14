using System;
using System.Collections;
using System.Collections.Generic;
using MutationSwarm.Entities;
using MutationSwarm.Evolution;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Core
{
    public enum WaveState
    {
        Waiting,
        Spawning,
        Active,
        Evolution,
        UpgradePhase
    }

    /// <summary>
    /// Control de oleadas con spawning funcional para prototipo.
    /// Soporta tanto el sistema antiguo (Transform[]) como el nuevo (Script_39_SpawnPoint).
    /// </summary>
    public class Script_02_WaveManager : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private SO_WaveConfig _config;
        [SerializeField] private SO_EnemySpawnConfig _spawnConfig;
        [SerializeField] private string _upgradeSceneName = "Scene_03_UpgradeMenu";

        [Header("Sistema antiguo (backward compatibility)")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _baseEnemies = 6;
        [SerializeField] private float _spawnInterval = 1.2f;

        [Header("Sistema nuevo (recomendado)")]
        [SerializeField] private Script_39_SpawnPoint[] _enemySpawnPoints;
        [SerializeField] private Script_SpawnPointGizmos _levelSpawnManager;

        public WaveState CurrentWaveState { get; private set; } = WaveState.Waiting;
        public int CurrentWave { get; private set; }
        public int EnemiesSpawned { get; private set; }
        public int EnemiesAlive { get; private set; }

        public List<Genome> CurrentGenomePool { get; private set; } = new();
        public List<EnemyCombatData> CombatDataThisWave { get; private set; } = new();

        public event Action<int> OnWaveStarted;
        public event Action<int, WaveSummary> OnWaveEnded;
        public event Action<GameObject> OnEnemySpawned;
#pragma warning disable CS0067 // Reserved events — raised in future expansion phases
        public event Action<EvolutionSummary> OnEvolutionPhaseStarted;
        public event Action<BossData> OnBossSpawning;
#pragma warning restore CS0067
        public event Action OnUpgradePhaseStarted;

        private Coroutine _waveRoutine;

        private void OnEnable()
        {
            Script_03_EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDiedEvent);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDiedEvent);
        }

        private void Awake()
        {
            if (CurrentGenomePool.Count == 0)
                CurrentGenomePool.Add(new Genome { RangoVision = 1f, Velocidad = 1.2f });

            if (_levelSpawnManager == null)
                _levelSpawnManager = FindFirstObjectByType<Script_SpawnPointGizmos>();

            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
            {
                _enemySpawnPoints = FindObjectsByType<Script_39_SpawnPoint>(FindObjectsSortMode.None);
                _enemySpawnPoints = System.Array.FindAll(_enemySpawnPoints,
                    sp => sp.Type == Script_39_SpawnPoint.SpawnType.Enemy);
            }
        }

        public void StartWave()
        {
            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);

            _waveRoutine = StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            CurrentWave++;
            CurrentWaveState = WaveState.Spawning;
            CombatDataThisWave.Clear();
            EnemiesSpawned = 0;

            float coopMul = Script_01_GameManager.Instance != null
                ? Script_01_GameManager.Instance.GetCoopEnemyMultiplier()
                : 1f;
            int toSpawn = Mathf.RoundToInt(_baseEnemies * coopMul);
            if (_config != null)
            {
                toSpawn = _config.baseEnemiesPerWave + _config.enemiesScalingPerWave * (CurrentWave - 1);
                if (_config.maxEnemiesCap > 0)
                    toSpawn = Mathf.Min(toSpawn, _config.maxEnemiesCap);
            }

            Script_03_EventBus.Publish(new WaveStartedEvent { waveNumber = CurrentWave });
            OnWaveStarted?.Invoke(CurrentWave);
            Script_03_EventBus.Publish(new EnemyCountChangedEvent { alive = 0, total = toSpawn });

            float interval = _spawnInterval;
            if (_config != null)
                interval = Mathf.Max(_config.minTimeBetweenSpawns,
                    _config.baseTimeBetweenSpawns - _config.spawnReductionPerWave * CurrentWave);

            for (int i = 0; i < toSpawn; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(interval);
            }

            CurrentWaveState = WaveState.Active;

            while (EnemiesAlive > 0)
                yield return null;

            EndWave();
        }

        public void SpawnEnemy()
        {
            // Intentar usar el nuevo sistema primero
            if (_enemySpawnPoints != null && _enemySpawnPoints.Length > 0)
            {
                SpawnEnemyNew();
                return;
            }

            // Fallback al sistema antiguo
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                return;

            GameObject prefabFallback = _spawnConfig != null
                ? _spawnConfig.GetRandomEnemyPrefab()
                : _enemyPrefab;
            if (prefabFallback == null) prefabFallback = _enemyPrefab;
            if (prefabFallback == null) return;

            Transform sp = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            if (sp == null) return;
            GameObject enemyGo = Instantiate(prefabFallback, sp.position, Quaternion.identity);
            EnemiesSpawned++;
            EnemiesAlive++;

            if (enemyGo.TryGetComponent(out Script_13_EnemyBase enemy))
            {
                Genome g = CurrentGenomePool[UnityEngine.Random.Range(0, CurrentGenomePool.Count)];
                enemy.Initialize(ScaleGenome(g.Clone()), GetHpMultiplier());
            }

            OnEnemySpawned?.Invoke(enemyGo);
            Script_03_EventBus.Publish(new EnemySpawnedEvent { enemy = enemyGo });
            Script_03_EventBus.Publish(new EnemyCountChangedEvent { alive = EnemiesAlive, total = EnemiesSpawned });
        }

        private Genome ScaleGenome(Genome g)
        {
            if (_config == null) return g;
            g.Velocidad = Mathf.Clamp(g.Velocidad + _config.speedMultiplierPerWave * (CurrentWave - 1),
                                       MutationSwarm.Evolution.Genome.VelocidadMin,
                                       MutationSwarm.Evolution.Genome.VelocidadMax);
            return g;
        }

        private float GetHpMultiplier()
        {
            if (_config == null) return 1f;
            return 1f + _config.hpMultiplierPerWave * (CurrentWave - 1);
        }

        /// <summary>
        /// Spawna un enemigo usando el nuevo sistema con Script_39_SpawnPoint.
        /// </summary>
        private void SpawnEnemyNew()
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0)
                return;

            // Seleccionar un spawn point aleatorio
            Script_39_SpawnPoint spawnPoint = _enemySpawnPoints[UnityEngine.Random.Range(0, _enemySpawnPoints.Length)];
            if (spawnPoint == null)
                return;

            // Determinar qué prefab spawnar: config → lista del nivel → spawn point individual → prefab global
            GameObject prefabToUse = null;
            if (_spawnConfig != null)
                prefabToUse = _spawnConfig.GetRandomEnemyPrefab();
            if (prefabToUse == null && _levelSpawnManager != null)
                prefabToUse = _levelSpawnManager.GetRandomLevelEnemy();
            if (prefabToUse == null)
                prefabToUse = spawnPoint.PrefabToSpawn;
            if (prefabToUse == null)
                prefabToUse = _enemyPrefab;

            if (prefabToUse == null)
            {
                Debug.LogWarning($"[WaveManager] No hay prefab de enemigo para spawnar en {spawnPoint.gameObject.name}");
                return;
            }

            // Instanciar
            GameObject enemyGo = Instantiate(prefabToUse, spawnPoint.SpawnPosition, Quaternion.identity);
            EnemiesSpawned++;
            EnemiesAlive++;

            // Inicializar con genoma escalado por oleada
            if (enemyGo.TryGetComponent(out Script_13_EnemyBase enemy))
            {
                Genome g = CurrentGenomePool[UnityEngine.Random.Range(0, CurrentGenomePool.Count)];
                enemy.Initialize(ScaleGenome(g.Clone()), GetHpMultiplier());
            }

            OnEnemySpawned?.Invoke(enemyGo);
            Script_03_EventBus.Publish(new EnemySpawnedEvent { enemy = enemyGo });
            Script_03_EventBus.Publish(new EnemyCountChangedEvent { alive = EnemiesAlive, total = EnemiesSpawned });
        }

        private void OnEnemyDiedEvent(EnemyDiedEvent e)
        {
            OnEnemyDied(e.combatData);
        }

        public void OnEnemyDied(EnemyCombatData data)
        {
            CombatDataThisWave.Add(data);
            EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
            Script_03_EventBus.Publish(new EnemyCountChangedEvent { alive = EnemiesAlive, total = EnemiesSpawned });

            // Las monedas las otorga Script_42_CoinManager vía EnemyDiedEvent
        }

        public void EndWave()
        {
            CurrentWaveState = WaveState.Evolution;
            WaveSummary summary = new()
            {
                enemiesKilled = CombatDataThisWave.Count,
                waveDuration = 0f
            };

            Script_03_EventBus.Publish(new WaveEndedEvent { waveNumber = CurrentWave, summary = summary });
            OnWaveEnded?.Invoke(CurrentWave, summary);
            CurrentWaveState = WaveState.Waiting;
        }

        public void StartNextWave()
        {
            StartWave();
        }

        public void EnterUpgradePhase()
        {
            // Eliminado: carga aditiva de escena de mejoras reemplazada por tienda en-escena
        }

        public void ExitUpgradePhase()
        {
            CurrentWaveState = WaveState.Waiting;
        }
    }

    [Serializable]
    public struct WaveSummary
    {
        public int enemiesKilled;
        public float waveDuration;
    }

    [Serializable]
    public struct BossData
    {
        public Genome mergedGenome;
        public int phaseCount;
    }
}
