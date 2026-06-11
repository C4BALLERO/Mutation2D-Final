using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Evolution
{
    /// <summary>
    /// Tournament selection and elitism over a genome population.
    /// </summary>
    public static class Script_09_SelectionAlgorithm
    {
        /// <summary>
        /// Picks <paramref name="selectCount"/> genomes using tournament selection.
        /// Each round: randomly sample <paramref name="tournamentSize"/> candidates
        /// and keep the one with the highest fitness.
        /// </summary>
        public static List<Genome> TournamentSelect(
            List<(Genome genome, float fitness)> population,
            int tournamentSize,
            int selectCount)
        {
            if (population == null || population.Count == 0)
                return new List<Genome>();

            List<Genome> selected = new(selectCount);
            tournamentSize = Mathf.Clamp(tournamentSize, 1, population.Count);

            for (int i = 0; i < selectCount; i++)
            {
                // Sample candidates without replacement within the tournament
                int bestIdx = Random.Range(0, population.Count);
                for (int k = 1; k < tournamentSize; k++)
                {
                    int candidate = Random.Range(0, population.Count);
                    if (population[candidate].fitness > population[bestIdx].fitness)
                        bestIdx = candidate;
                }
                selected.Add(population[bestIdx].genome.Clone());
            }

            return selected;
        }

        /// <summary>
        /// Returns clones of the top <paramref name="eliteCount"/> genomes by fitness.
        /// </summary>
        public static List<Genome> ApplyElitism(
            List<(Genome genome, float fitness)> population,
            int eliteCount)
        {
            if (population == null || population.Count == 0)
                return new List<Genome>();

            eliteCount = Mathf.Min(eliteCount, population.Count);
            var sorted = new List<(Genome genome, float fitness)>(population);
            sorted.Sort((a, b) => b.fitness.CompareTo(a.fitness));

            List<Genome> elites = new(eliteCount);
            for (int i = 0; i < eliteCount; i++)
                elites.Add(sorted[i].genome.Clone());

            return elites;
        }
    }
}
