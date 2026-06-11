using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Punto de spawn parametrizable que puede instanciar prefabs de enemigos o jugadores.
    /// Cada habitación puede asignar diferentes prefabs por spawn point.
    /// </summary>
    public class Script_39_SpawnPoint : MonoBehaviour
    {
        public enum SpawnType
        {
            Enemy,
            Player
        }

        [SerializeField] private SpawnType _spawnType = SpawnType.Enemy;
        [SerializeField] private GameObject _prefabToSpawn;
        [SerializeField] private bool _showGizmo = true;

        public SpawnType Type => _spawnType;
        public Vector3 SpawnPosition => transform.position;
        public GameObject PrefabToSpawn
        {
            get => _prefabToSpawn;
            set => _prefabToSpawn = value;
        }

        /// <summary>
        /// Instancia el prefab asignado en la posición del spawn point.
        /// </summary>
        public GameObject Spawn()
        {
            if (_prefabToSpawn == null)
            {
                Debug.LogWarning($"[SpawnPoint] {gameObject.name} no tiene prefab asignado.");
                return null;
            }

            GameObject instance = Instantiate(_prefabToSpawn, transform.position, Quaternion.identity);
            return instance;
        }

        /// <summary>
        /// Instancia el prefab asignado con un prefab específico (sobrescribe el asignado).
        /// </summary>
        public GameObject SpawnWithPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[SpawnPoint] {gameObject.name} recibió un prefab nulo.");
                return null;
            }

            GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity);
            return instance;
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmo)
                return;

            // Color según tipo
            Gizmos.color = _spawnType == SpawnType.Enemy ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            // Ícono
            Gizmos.DrawLine(transform.position + Vector3.left * 0.15f, transform.position + Vector3.right * 0.15f);
            Gizmos.DrawLine(transform.position + Vector3.down * 0.15f, transform.position + Vector3.up * 0.15f);
        }

        public void OnValidate()
        {
            // Asegurar que el nombre refleje el tipo de spawn
            if (gameObject.name.StartsWith("sp_") || gameObject.name.StartsWith("p"))
                return; // Mantener el nombre existente
        }
    }
}
