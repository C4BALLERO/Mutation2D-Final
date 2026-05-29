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
            EnsurePlayer();
            if (_autoStartWave)
            {
                Script_02_WaveManager wave = FindFirstObjectByType<Script_02_WaveManager>();
                wave?.StartWave();
            }
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
