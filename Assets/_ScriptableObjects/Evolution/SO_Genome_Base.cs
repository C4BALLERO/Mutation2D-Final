using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Evolution
{
    [CreateAssetMenu(fileName = "SO_Genome_Base", menuName = "MutationSwarm/Genome Base")]
    public class SO_Genome_Base : ScriptableObject
    {
        public Genome baseGenome = new();
    }
}
