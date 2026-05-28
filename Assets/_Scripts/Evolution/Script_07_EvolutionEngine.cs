using System;
using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Evolution
{
    [Serializable]
    public struct EnemyCombatData
    {
        public Genome genome;
        public float timeAlive;
        public float damageDone;
        public bool survived;
    }

    [Serializable]
    public struct PlayerBehaviorData
    {
        public string mostUsedWeapon;
        public float avgYPosition;
        public float meleeDamageTotal;
        public float rangedDamageTotal;
        public string mostUsedStructure;
    }

    [Serializable]
    public struct EvolutionSummary
    {
        public string dominantGene;
        public float avgFitness;
        public List<string> newMutations;
        public int generationNumber;
    }

    /// <summary>
    /// Motor evolutivo: selección, crossover, mutación y presión adaptativa.
    /// </summary>
    public class Script_07_EvolutionEngine : MonoBehaviour
    {
        public event Action<EvolutionSummary> OnEvolutionComplete;

        public List<Genome> ProcessWave(List<EnemyCombatData> waveData, PlayerBehaviorData playerData)
        {
            // Implementación completa en PROMPT 02
            return new List<Genome> { new Genome() };
        }
    }
}
