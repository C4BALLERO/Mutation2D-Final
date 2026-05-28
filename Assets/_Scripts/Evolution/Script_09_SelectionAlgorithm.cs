using System.Collections.Generic;

namespace MutationSwarm.Evolution
{
    /// <summary>
    /// Selección por torneo y elitismo sobre población de genomas.
    /// </summary>
    public static class Script_09_SelectionAlgorithm
    {
        public static List<Genome> TournamentSelect(
            List<(Genome genome, float fitness)> population,
            int tournamentSize,
            int selectCount)
        {
            return new List<Genome>();
        }

        public static List<Genome> ApplyElitism(
            List<(Genome genome, float fitness)> population,
            int eliteCount)
        {
            return new List<Genome>();
        }
    }
}
