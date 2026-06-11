using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Bootstraps every room scene:
    ///   1. Spawns the player at the room's "p1" spawn point.
    ///   2. Starts the first wave after _waveStartDelay seconds.
    ///   3. Listens for all-players-dead → returns to MainMenu.
    /// Place this component on a _RoomBootstrap GameObject in every room scene.
    /// The editor tool (MutationSwarmFullSetup) adds it automatically.
    /// </summary>
    public class Script_37_RoomBootstrap : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform  _spawnPoint;

        [Header("Wave")]
        [SerializeField] private bool  _autoStartWave = true;
        [SerializeField] private float _waveStartDelay = 1.2f;

        private void Start()
        {
            SpawnPlayer();

            if (_autoStartWave)
                Invoke(nameof(StartWave), _waveStartDelay);

            // Subscribe game-over → return to main menu
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.OnAllPlayersDead += OnGameOver;
        }

        private void OnDestroy()
        {
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.OnAllPlayersDead -= OnGameOver;
        }

        private void SpawnPlayer()
        {
            // If a player already exists (co-op re-load), don't duplicate.
            if (FindFirstObjectByType<Script_11_PlayerController>() != null)
                return;

            if (_playerPrefab == null)
            {
                Debug.LogError("[RoomBootstrap] No player prefab assigned. Assign Prefab_Player.");
                return;
            }

            Vector3 pos = ResolveSpawnPosition();
            Instantiate(_playerPrefab, pos, Quaternion.identity);
        }

        private Vector3 ResolveSpawnPosition()
        {
            // Priority: assigned _spawnPoint → scene object named "p1" → hardcoded fallback
            if (_spawnPoint != null)
                return _spawnPoint.position;

            GameObject p1 = FindPlayerSpawnByName();
            if (p1 != null)
                return p1.transform.position;

            return new Vector3(-1f, -3f, 0f);
        }

        private static GameObject FindPlayerSpawnByName()
        {
            // Look in _SpawnPoints parent first, then entire hierarchy
            GameObject spawnRoot = GameObject.Find("_SpawnPoints");
            if (spawnRoot != null)
            {
                foreach (Transform child in spawnRoot.transform)
                {
                    if (child.name == "p1")
                        return child.gameObject;
                }
            }

            // Fallback: find by exact name anywhere in scene
            return GameObject.Find("p1");
        }

        private void StartWave()
        {
            Script_02_WaveManager wm = FindFirstObjectByType<Script_02_WaveManager>();
            if (wm == null)
            {
                Debug.LogWarning("[RoomBootstrap] No WaveManager found in scene.");
                return;
            }
            wm.StartWave();
        }

        private void OnGameOver()
        {
            Debug.Log("[RoomBootstrap] Game Over — returning to Main Menu.");
            Invoke(nameof(ReturnToMenu), 2f);
        }

        private void ReturnToMenu() => Script_36_SceneLoader.LoadMainMenu();
    }
}
