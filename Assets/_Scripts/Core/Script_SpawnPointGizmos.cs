using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Manager de spawn points del nivel.
    /// Define qué enemigos aparecen en este nivel y los distribuye entre los spawn points hijos.
    /// </summary>
    public class Script_SpawnPointGizmos : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField] private Color _arenaBoundsColor = new(0.2f, 0.8f, 1f, 0.5f);
        [SerializeField] private float _arenaHalfWidth = 9.5f;
        [SerializeField] private float _arenaHalfHeight = 4.5f;

        [Header("Enemigos del Nivel")]
        [Tooltip("Todos los prefabs de enemigos que pueden aparecer en este nivel.")]
        [SerializeField] private List<GameObject> _levelEnemyPrefabs = new();

        public IReadOnlyList<GameObject> LevelEnemyPrefabs => _levelEnemyPrefabs;

        /// <summary>Returns a random enemy prefab from the level list, or null if empty.</summary>
        public GameObject GetRandomLevelEnemy()
        {
            if (_levelEnemyPrefabs == null || _levelEnemyPrefabs.Count == 0)
                return null;
            return _levelEnemyPrefabs[Random.Range(0, _levelEnemyPrefabs.Count)];
        }

        /// <summary>
        /// Assigns enemy prefabs to all enemy spawn points in round-robin order from the level list.
        /// </summary>
        public void DistributeRoundRobin()
        {
            if (_levelEnemyPrefabs == null || _levelEnemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[SpawnPointGizmos] No hay enemigos en la lista del nivel.");
                return;
            }

            SpawnPointChild[] points = GetEnemySpawnPoints();
            for (int i = 0; i < points.Length; i++)
            {
                points[i].PrefabToSpawn = _levelEnemyPrefabs[i % _levelEnemyPrefabs.Count];
            }

            Debug.Log($"[SpawnPointGizmos] Distribuidos {_levelEnemyPrefabs.Count} tipos de enemigo en {points.Length} spawn points (orden).");
        }

        /// <summary>
        /// Assigns enemy prefabs randomly from the level list to all enemy spawn points.
        /// </summary>
        public void DistributeRandom()
        {
            if (_levelEnemyPrefabs == null || _levelEnemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[SpawnPointGizmos] No hay enemigos en la lista del nivel.");
                return;
            }

            SpawnPointChild[] points = GetEnemySpawnPoints();
            foreach (var sp in points)
                sp.PrefabToSpawn = _levelEnemyPrefabs[Random.Range(0, _levelEnemyPrefabs.Count)];

            Debug.Log($"[SpawnPointGizmos] Distribuidos aleatoriamente en {points.Length} spawn points.");
        }

        // ── Runtime spawning ─────────────────────────────────────────────────

        /// <summary>Spawns each child's assigned prefab.</summary>
        public GameObject[] SpawnAll()
        {
            SpawnPointChild[] spawnPoints = GetComponentsInChildren<SpawnPointChild>();
            var instances = new GameObject[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
                instances[i] = spawnPoints[i].Spawn();
            return instances;
        }

        /// <summary>Spawns a specific prefab at every child spawn point.</summary>
        public GameObject[] SpawnAll(GameObject prefab)
        {
            SpawnPointChild[] spawnPoints = GetComponentsInChildren<SpawnPointChild>();
            var instances = new GameObject[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
                instances[i] = spawnPoints[i].SpawnWithPrefab(prefab);
            return instances;
        }

        /// <summary>Spawns only enemy (non-player) spawn points using each child's assigned prefab.</summary>
        public GameObject[] SpawnAllEnemies()
        {
            var instances = new List<GameObject>();
            foreach (var sp in GetEnemySpawnPoints())
                instances.Add(sp.Spawn());
            return instances.ToArray();
        }

        /// <summary>Spawns only player spawn points using the given prefab.</summary>
        public GameObject[] SpawnAllPlayers(GameObject prefab)
        {
            var instances = new List<GameObject>();
            foreach (var sp in GetComponentsInChildren<SpawnPointChild>())
            {
                if (sp.IsPlayerSpawn)
                    instances.Add(sp.SpawnWithPrefab(prefab));
            }
            return instances.ToArray();
        }

        public SpawnPointChild GetSpawnPoint(string spawnName)
        {
            foreach (var child in GetComponentsInChildren<SpawnPointChild>())
            {
                if (child.gameObject.name == spawnName)
                    return child;
            }
            return null;
        }

        public SpawnPointChild[] GetAllSpawnPoints() => GetComponentsInChildren<SpawnPointChild>();

        private SpawnPointChild[] GetEnemySpawnPoints()
        {
            var all = GetComponentsInChildren<SpawnPointChild>();
            var result = new List<SpawnPointChild>();
            foreach (var sp in all)
            {
                if (!sp.IsPlayerSpawn)
                    result.Add(sp);
            }
            return result.ToArray();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _arenaBoundsColor;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_arenaHalfWidth * 2f, _arenaHalfHeight * 2f, 0f));
        }
    }
}
