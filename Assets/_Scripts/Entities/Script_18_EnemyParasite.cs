using System.Collections.Generic;
using MutationSwarm.Core;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Parasite: attaches to nearby allies and propagates mutations to them,
    /// boosting their stats. Each parasite can infect up to MaxHosts allies.
    /// </summary>
    public class Script_18_EnemyParasite : Script_13_EnemyBase
    {
        [SerializeField] private float _infectionRadius  = 2.5f;
        [SerializeField] private float _infectionInterval = 5f;
        [SerializeField] private int   _maxHosts         = 3;

        private float _infectionTimer;
        private readonly HashSet<Script_13_EnemyBase> _infectedHosts = new();

        private void LateUpdate()
        {
            if (_infectionTimer > 0f)
            {
                _infectionTimer -= Time.deltaTime;
                return;
            }

            _infectionTimer = _infectionInterval;
            TryInfectAllies();
        }

        private void TryInfectAllies()
        {
            if (_infectedHosts.Count >= _maxHosts)
                return;

            List<Script_13_EnemyBase> nearby = GetNearbyAllies(_infectionRadius);
            foreach (Script_13_EnemyBase ally in nearby)
            {
                if (_infectedHosts.Contains(ally)) continue;
                if (ally is Script_18_EnemyParasite)   continue; // don't infect other parasites

                PropagateMutation(ally);
                _infectedHosts.Add(ally);

                if (_infectedHosts.Count >= _maxHosts) break;
            }
        }

        private void PropagateMutation(Script_13_EnemyBase host)
        {
            if (host.Genome == null || Genome == null) return;

            var merged = host.Genome.Crossover(this.Genome);
            merged.Mutate(0.1f);
            host.Initialize(merged);

            Script_03_EventBus.Publish(new ParasiteInfectedEvent { parasite = this, host = host });
        }
    }
}
