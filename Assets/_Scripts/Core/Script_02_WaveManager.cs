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
    /// </summary>
    public class Script_02_WaveManager : MonoBehaviour
    {
        [SerializeField] private SO_WaveConfig _config;
        [SerializeField] private string _upgradeSceneName = "Scene_03_UpgradeMenu";
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private int _baseEnemies = 6;
        [SerializeField] private float _spawnInterval = 1.2f;

        public WaveState CurrentWaveState { get; private set; } = WaveState.Waiting;
        public int CurrentWave { get; private set; }
        public int EnemiesSpawned { get; private set; }
        public int EnemiesAlive { get; private set; }

        public List<Genome> CurrentGenomePool { get; private set; } = new();
        public List<EnemyCombatData> CombatDataThisWave { get; private set; } = new();

        public event Action<int> OnWaveStarted;
        public event Action<int, WaveSummary> OnWaveEnded;
        public event Action<GameObject> OnEnemySpawned;
        public event Action<EvolutionSummary> OnEvolutionPhaseStarted;
        public event Action OnUpgradePhaseStarted;
        public event Action<BossData> OnBossSpawning;

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
                CurrentGenomePool.Add(new Genome());
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
                toSpawn = _config.baseEnemiesPerWave + _config.enemiesScalingPerWave * (CurrentWave - 1);

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
            if (_enemyPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
                return;

            Transform sp = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
            GameObject enemyGo = Instantiate(_enemyPrefab, sp.position, Quaternion.identity);
            EnemiesSpawned++;
            EnemiesAlive++;

            if (enemyGo.TryGetComponent(out Script_13_EnemyBase enemy))
            {
                Genome g = CurrentGenomePool[UnityEngine.Random.Range(0, CurrentGenomePool.Count)];
                enemy.Initialize(g.Clone());
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

            if (MutationSwarm.Building.Script_23_BuildManager.Instance != null)
                MutationSwarm.Building.Script_23_BuildManager.Instance.AddMaterials(UnityEngine.Random.Range(1, 4));
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
            CurrentWaveState = WaveState.UpgradePhase;
            Script_03_EventBus.Publish(new UpgradePhaseEvent());
            OnUpgradePhaseStarted?.Invoke();

            if (!SceneManager.GetSceneByName(_upgradeSceneName).isLoaded)
                SceneManager.LoadScene(_upgradeSceneName, LoadSceneMode.Additive);

            if (MutationSwarm.Building.Script_23_BuildManager.Instance != null)
                MutationSwarm.Building.Script_23_BuildManager.Instance.StartBuildPhase();
        }

        public void ExitUpgradePhase()
        {
            if (SceneManager.GetSceneByName(_upgradeSceneName).isLoaded)
                SceneManager.UnloadSceneAsync(_upgradeSceneName);

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
