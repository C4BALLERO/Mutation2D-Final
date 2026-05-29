using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Control de oleadas, spawning y transiciones hacia evolución/upgrade.
    /// </summary>
    public class Script_02_WaveManager : MonoBehaviour
    {
        [SerializeField] private SO_WaveConfig _config;
        [SerializeField] private string _upgradeSceneName = "Scene_03_UpgradeMenu";

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

        public void StartWave() { /* Implementación en PROMPT 03 */ }
        public void OnEnemyDied(EnemyCombatData data) { /* Implementación en PROMPT 03 */ }
        public void EndWave() { /* Implementación en PROMPT 03 */ }
        public void StartNextWave() { /* Implementación en PROMPT 03 */ }

        /// <summary>
        /// Inicia fase de construcción/upgrades entre oleadas.
        /// </summary>
        public void EnterUpgradePhase()
        {
            CurrentWaveState = WaveState.UpgradePhase;
            Script_03_EventBus.Publish(new UpgradePhaseEvent());

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
