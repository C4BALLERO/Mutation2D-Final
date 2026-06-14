using MutationSwarm.Combat;
using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Arranca partida en GameWorld: spawnea jugador geométrico e inicia oleada 1.
    /// </summary>
    public class Script_32_GameplayBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawn;
        [SerializeField] private bool _autoStartWave = true;

        private void Start()
        {
            Script_42_CoinManager.Instance?.ResetForNewSession();
            EnsurePlayer();
            if (_autoStartWave)
            {
                Script_02_WaveManager wave = FindFirstObjectByType<Script_02_WaveManager>();
                wave?.StartWave();
            }

            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.OnAllPlayersDead += OnGameOver;
        }

        private void OnDestroy()
        {
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.OnAllPlayersDead -= OnGameOver;
        }

        private void OnGameOver()
        {
            Invoke(nameof(ReturnToMenu), 2f);
        }

        private void ReturnToMenu()
        {
            Script_36_SceneLoader.LoadMainMenu();
        }

        private void EnsurePlayer()
        {
            if (FindFirstObjectByType<Script_11_PlayerController>() != null)
                return;

            if (_playerPrefab == null)
            {
                Debug.LogWarning("[GameplayBootstrap] Falta Prefab_Player_Geo.");
                return;
            }

            Vector3 pos = _playerSpawn != null ? _playerSpawn.position : new Vector3(-1f, -3f, 0f);
            Instantiate(_playerPrefab, pos, Quaternion.identity);
        }
    }
}
