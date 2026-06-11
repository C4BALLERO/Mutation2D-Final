using UnityEngine;

namespace MutationSwarm.Evolution
{
    /// <summary>
    /// Configuración de spawn de enemigos por nivel/oleada.
    /// Permite asignar múltiples prefabs de enemigos y definir qué spawn points usan cada uno.
    /// </summary>
    [CreateAssetMenu(menuName = "MutationSwarm/Enemy Spawn Config", fileName = "SO_EnemySpawnConfig")]
    public class SO_EnemySpawnConfig : ScriptableObject
    {
        [System.Serializable]
        public class EnemySpawnEntry
        {
            [Tooltip("Prefab del enemigo a spawnar")]
            public GameObject enemyPrefab;

            [Tooltip("Nombre del spawn point o patrón (ej: sp_left*, sp_right*) - si vacío, usa cualquiera")]
            public string spawnPointPattern = "";

            [Tooltip("Peso relativo para random selection")]
            [Range(0.1f, 10f)]
            public float weight = 1f;

            [Tooltip("% de oleadas donde este enemigo aparece")]
            [Range(0f, 100f)]
            public float appearanceChance = 100f;
        }

        [Header("Enemigos disponibles en esta oleada")]
        [SerializeField] private EnemySpawnEntry[] _enemyEntries = new EnemySpawnEntry[1];

        [Header("Distribución")]
        [SerializeField] private bool _randomizeOrder = true;
        [SerializeField] private float _minSpawnDelay = 0.5f;
        [SerializeField] private float _maxSpawnDelay = 1.5f;

        public EnemySpawnEntry[] EnemyEntries => _enemyEntries;
        public bool RandomizeOrder => _randomizeOrder;
        public float MinSpawnDelay => _minSpawnDelay;
        public float MaxSpawnDelay => _maxSpawnDelay;

        /// <summary>
        /// Obtiene un prefab de enemigo aleatorio basado en pesos y probabilidades.
        /// </summary>
        public GameObject GetRandomEnemyPrefab()
        {
            if (_enemyEntries == null || _enemyEntries.Length == 0)
                return null;

            // Filtrar entradas que cumplen con la probabilidad
            System.Collections.Generic.List<EnemySpawnEntry> validEntries = new();
            foreach (var entry in _enemyEntries)
            {
                if (entry.enemyPrefab != null && Random.value * 100f <= entry.appearanceChance)
                    validEntries.Add(entry);
            }

            if (validEntries.Count == 0)
                return _enemyEntries[0]?.enemyPrefab;

            // Selección ponderada
            float totalWeight = 0f;
            foreach (var entry in validEntries)
                totalWeight += entry.weight;

            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;

            foreach (var entry in validEntries)
            {
                currentWeight += entry.weight;
                if (randomValue <= currentWeight)
                    return entry.enemyPrefab;
            }

            return validEntries[validEntries.Count - 1].enemyPrefab;
        }

        /// <summary>
        /// Obtiene el prefab para un patrón específico de spawn point.
        /// Si no hay coincidencia exacta, devuelve un prefab aleatorio.
        /// </summary>
        public GameObject GetEnemyPrefabForSpawnPoint(string spawnPointName)
        {
            if (_enemyEntries == null || _enemyEntries.Length == 0)
                return null;

            // Buscar entrada que coincida con el patrón
            foreach (var entry in _enemyEntries)
            {
                if (string.IsNullOrEmpty(entry.spawnPointPattern))
                    continue;

                if (spawnPointName.StartsWith(entry.spawnPointPattern.Replace("*", "")))
                {
                    if (Random.value * 100f <= entry.appearanceChance)
                        return entry.enemyPrefab;
                }
            }

            // Fallback: devolver aleatorio
            return GetRandomEnemyPrefab();
        }

        /// <summary>
        /// Obtiene el siguiente delay entre spawns (interpolado entre min/max).
        /// </summary>
        public float GetNextSpawnDelay()
        {
            return Random.Range(_minSpawnDelay, _maxSpawnDelay);
        }
    }
}
