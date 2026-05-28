using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Interfaz para objetos que viven en pool.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
        float PoolLifetime { get; }
    }

    /// <summary>
    /// Pool genérico reutilizable para proyectiles, enemigos y VFX.
    /// </summary>
    public class Script_04_ObjectPool : MonoBehaviour
    {
        public static Script_04_ObjectPool Instance { get; private set; }

        [SerializeField] private List<SO_PoolConfig> _poolConfigs = new();

        private readonly Dictionary<string, Queue<GameObject>> _pools = new();
        private readonly Dictionary<string, int> _activeCounts = new();
        private readonly Dictionary<string, SO_PoolConfig> _configLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (SO_PoolConfig config in _poolConfigs)
            {
                if (config == null || string.IsNullOrEmpty(config.poolKey))
                    continue;

                _configLookup[config.poolKey] = config;
                _pools[config.poolKey] = new Queue<GameObject>();
                _activeCounts[config.poolKey] = 0;
                PreWarm(config.poolKey, config.initialSize);
            }
        }

        public GameObject Get(string poolKey)
        {
            if (!_configLookup.TryGetValue(poolKey, out SO_PoolConfig config))
            {
                Debug.LogWarning($"[ObjectPool] Pool key no encontrada: {poolKey}");
                return null;
            }

            GameObject obj;
            if (_pools[poolKey].Count > 0)
            {
                obj = _pools[poolKey].Dequeue();
            }
            else if (_activeCounts[poolKey] < config.maxSize)
            {
                obj = Instantiate(config.prefab, transform);
                _activeCounts[poolKey]++;
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Max size alcanzado para {poolKey}");
                return null;
            }

            obj.SetActive(true);
            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnSpawn();

            return obj;
        }

        public void Return(string poolKey, GameObject obj)
        {
            if (obj == null || !_pools.ContainsKey(poolKey))
                return;

            if (obj.TryGetComponent(out IPoolable poolable))
                poolable.OnDespawn();

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pools[poolKey].Enqueue(obj);
        }

        public void PreWarm(string poolKey, int count)
        {
            if (!_configLookup.TryGetValue(poolKey, out SO_PoolConfig config))
                return;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(config.prefab, transform);
                obj.SetActive(false);
                _pools[poolKey].Enqueue(obj);
            }
        }

        public void ReturnAll(string poolKey)
        {
            // Implementación completa en PROMPT 08
        }
    }
}
