using MutationSwarm.Core;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Boss: 3 combat phases based on HP thresholds.
    /// Phase 1 (100-66% HP): standard behavior.
    /// Phase 2 (65-33% HP): increased speed + vision, spawns a minion wave.
    /// Phase 3 (32-0% HP):  enrage — max speed, attack range extended, spines active.
    /// </summary>
    public class Script_15_EnemyBoss : Script_13_EnemyBase
    {
        [SerializeField] private GameObject _minionPrefab;
        [SerializeField] private int        _minionsPerPhase = 3;

        private int _currentPhase = 1;

        /// <summary>Current combat phase (1-3). Used by external systems and BossPhaseChangedEvent.</summary>
        public int CurrentPhase => _currentPhase;
        private bool _phase2Triggered;
        private bool _phase3Triggered;

        public void InitializeBoss(Genome mergedGenome)
        {
            Initialize(mergedGenome);
        }

        private void LateUpdate()
        {
            CheckPhaseTransitions();
        }

        private void CheckPhaseTransitions()
        {
            if (MaxHp <= 0f) return;

            float hpPercent = CurrentHp / MaxHp;

            if (!_phase2Triggered && hpPercent <= 0.66f)
            {
                _phase2Triggered = true;
                _currentPhase    = 2;
                EnterPhase2();
            }

            if (!_phase3Triggered && hpPercent <= 0.33f)
            {
                _phase3Triggered = true;
                _currentPhase    = 3;
                EnterPhase3();
            }
        }

        private void EnterPhase2()
        {
            Evolution.Genome g = Genome;
            if (g != null)
            {
                g.Velocidad   = Mathf.Min(4.0f, g.Velocidad * 1.4f);   // VelocidadMax = 4.0f
                g.RangoVision = Mathf.Min(1.0f, g.RangoVision * 1.3f); // RangoVisionMax = 1.0f
            }

            SpawnMinions();
            Script_03_EventBus.Publish(new BossPhaseChangedEvent { boss = this, phase = 2 });
        }

        private void EnterPhase3()
        {
            Evolution.Genome g = Genome;
            if (g != null)
            {
                g.Velocidad = 4.0f;                                    // VelocidadMax
                g.Espinas   = Mathf.Min(1.0f, g.Espinas + 0.4f);      // EspinasMax = 1.0f
            }

            SpawnMinions();
            Script_03_EventBus.Publish(new BossPhaseChangedEvent { boss = this, phase = 3 });
        }

        private void SpawnMinions()
        {
            if (_minionPrefab == null) return;

            for (int i = 0; i < _minionsPerPhase; i++)
            {
                Vector2 spawnOffset = Random.insideUnitCircle * 2.5f;
                GameObject minionGo = Instantiate(
                    _minionPrefab,
                    (Vector2)transform.position + spawnOffset,
                    Quaternion.identity);

                if (minionGo.TryGetComponent(out Script_13_EnemyBase minion))
                {
                    Evolution.Genome minionGenome = Genome.Clone();
                    minionGenome.Mutate(0.2f);
                    minionGenome.Tamaño = Mathf.Lerp(0.5f, 3.0f, 0.3f); // TamañoMin=0.5, TamañoMax=3.0
                    minion.Initialize(minionGenome);
                }
            }
        }
    }
}
