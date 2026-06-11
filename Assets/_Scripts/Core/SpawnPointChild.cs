using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Componente individual para cada spawn point.
    /// Añade esto a cada child de _SpawnPoints para asignar prefabs individuales.
    /// El inspector muestra los prefabs de forma clara.
    /// </summary>
    public class SpawnPointChild : MonoBehaviour
    {
        [SerializeField] private GameObject _prefabToSpawn;
        [SerializeField] private bool _isPlayerSpawn = false;

        public GameObject PrefabToSpawn
        {
            get => _prefabToSpawn;
            set => _prefabToSpawn = value;
        }

        public bool IsPlayerSpawn => _isPlayerSpawn;
        public Vector3 SpawnPosition => transform.position;

        /// <summary>
        /// Spawna el prefab asignado en esta posición.
        /// </summary>
        public GameObject Spawn()
        {
            if (_prefabToSpawn == null)
            {
                Debug.LogWarning($"[SpawnPointChild] {gameObject.name} no tiene prefab asignado.");
                return null;
            }

            GameObject instance = Instantiate(_prefabToSpawn, transform.position, Quaternion.identity);
            return instance;
        }

        /// <summary>
        /// Spawna con un prefab específico (sobrescribe el asignado).
        /// </summary>
        public GameObject SpawnWithPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;

            GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity);
            return instance;
        }

        private void OnDrawGizmosSelected()
        {
            // Gizmo visual en el editor
            if (_prefabToSpawn != null)
            {
                Gizmos.color = _isPlayerSpawn ? Color.cyan : Color.yellow;
            }
            else
            {
                Gizmos.color = _isPlayerSpawn ? Color.green : Color.red;
            }

            Gizmos.DrawWireSphere(transform.position, _isPlayerSpawn ? 0.25f : 0.3f);
        }

        private void OnDrawGizmos()
        {
            // Gizmo permanente
            Gizmos.color = _isPlayerSpawn ? Color.green : Color.red;
            Gizmos.DrawSphere(transform.position, _isPlayerSpawn ? 0.15f : 0.2f);
        }
    }
}
