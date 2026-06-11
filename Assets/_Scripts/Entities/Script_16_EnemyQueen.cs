using System.Collections;
using MutationSwarm.Core;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Queen: spawns drone-genome offspring every N seconds.
    /// Spawn rate scales with Regeneracion (used as fertility proxy).
    /// Coroutine waits until the genome is initialized before spawning.
    /// </summary>
    public class Script_16_EnemyQueen : Script_13_EnemyBase
    {
        [SerializeField] private GameObject _dronePrefab;
        [SerializeField] private float _baseSpawnInterval = 8f;
        [SerializeField] private int _maxSpawnedDrones = 4;

        private int _activeDrones;

        private void Start()
        {
            if (_dronePrefab != null)
                StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            // Wait until Initialize() provides the Genome
            while (Genome == null)
                yield return null;

            while (true)
            {
                float interval = _baseSpawnInterval * (1f - Genome.Regeneracion * 0.5f);
                yield return new WaitForSeconds(Mathf.Max(2f, interval));

                if (_activeDrones < _maxSpawnedDrones)
                    SpawnDrone();
            }
        }

        private void SpawnDrone()
        {
            if (_dronePrefab == null) return;

            Vector2 offset = Random.insideUnitCircle * 1.5f;
            GameObject droneGo = Instantiate(_dronePrefab,
                (Vector2)transform.position + offset, Quaternion.identity);

            if (droneGo.TryGetComponent(out Script_13_EnemyBase drone))
            {
                Genome offspringGenome = Genome.Clone();
                offspringGenome.Mutate(0.15f);
                drone.Initialize(offspringGenome);
            }

            _activeDrones++;
            Script_03_EventBus.Subscribe<EnemyDiedEvent>(OnDroneDied);
        }

        private void OnDroneDied(EnemyDiedEvent ev)
        {
            if (_activeDrones > 0)
                _activeDrones--;
        }

        private void OnDestroy()
        {
            Script_03_EventBus.Unsubscribe<EnemyDiedEvent>(OnDroneDied);
        }
    }
}
