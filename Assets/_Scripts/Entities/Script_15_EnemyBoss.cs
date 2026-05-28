using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Jefe procedural con 3 fases basadas en genomas top del historial.
    /// </summary>
    public class Script_15_EnemyBoss : Script_13_EnemyBase
    {
        [SerializeField] private int _currentPhase = 1;

        public void InitializeBoss(Genome mergedGenome)
        {
            Initialize(mergedGenome);
        }
    }
}
