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
    /// Evolution engine: selection, crossover, mutation, and adaptive pressure.
    /// Processes wave combat data and returns the next-generation genome pool.
    /// </summary>
    public class Script_07_EvolutionEngine : MonoBehaviour
    {
        [SerializeField] private int _populationSize  = 20;
        [SerializeField] private int _eliteCount      = 4;
        [SerializeField] private int _tournamentSize  = 3;
        [SerializeField] private float _mutationRate  = 0.25f;

        private int _generation;

        public event Action<EvolutionSummary> OnEvolutionComplete;

        public List<Genome> ProcessWave(List<EnemyCombatData> waveData, PlayerBehaviorData playerData)
        {
            if (waveData == null || waveData.Count == 0)
                return SeedGeneration(_populationSize);

            // 1. Build scored population
            var population = ScorePopulation(waveData);

            // 2. Elite preservation
            List<Genome> nextGen = Script_09_SelectionAlgorithm.ApplyElitism(population, _eliteCount);

            // 3. Fill remainder via tournament selection + crossover + mutation
            List<Genome> parents = Script_09_SelectionAlgorithm.TournamentSelect(
                population, _tournamentSize, _populationSize - _eliteCount);

            for (int i = 0; i < parents.Count - 1; i += 2)
            {
                Genome child = Genome.Crossover(parents[i], parents[i + 1]);
                if (UnityEngine.Random.value < _mutationRate)
                    child.Mutate(0.2f);
                nextGen.Add(child);
            }
            // Handle odd one out
            if (nextGen.Count < _populationSize)
            {
                Genome extra = parents[^1].Clone();
                extra.Mutate(_mutationRate);
                nextGen.Add(extra);
            }

            // 4. Apply adaptive pressure counter-strategy
            foreach (Genome g in nextGen)
                Script_10_AdaptivePressure.Apply(playerData, g);

            // 5. Fire summary event
            _generation++;
            float avgFitness = 0f;
            foreach (Genome g in nextGen) avgFitness += g.GetFitnessModifier();
            avgFitness /= nextGen.Count;

            string dominant = nextGen.Count > 0 ? nextGen[0].GetDominantGene() : "Velocidad";
            OnEvolutionComplete?.Invoke(new EvolutionSummary
            {
                dominantGene    = dominant,
                avgFitness      = avgFitness,
                newMutations    = new List<string>(),
                generationNumber = _generation,
            });

            return nextGen;
        }

        private static List<(Genome genome, float fitness)> ScorePopulation(List<EnemyCombatData> data)
        {
            var result = new List<(Genome, float)>(data.Count);
            foreach (EnemyCombatData d in data)
            {
                if (d.genome == null) continue;
                // Fitness = base genome modifier + combat performance bonus
                float fitness = d.genome.GetFitnessModifier()
                    + d.damageDone * 0.01f
                    + d.timeAlive  * 0.005f
                    + (d.survived  ? 0.5f : 0f);
                result.Add((d.genome, fitness));
            }
            return result;
        }

        private static List<Genome> SeedGeneration(int count)
        {
            var seed = new List<Genome>(count);
            for (int i = 0; i < count; i++)
            {
                Genome g = new();
                g.Mutate(0.15f);
                seed.Add(g);
            }
            return seed;
        }
    }
}
