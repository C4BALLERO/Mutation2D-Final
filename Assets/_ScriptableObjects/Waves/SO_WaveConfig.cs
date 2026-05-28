using UnityEngine;

namespace MutationSwarm.Core
{
    [CreateAssetMenu(fileName = "SO_WaveConfig", menuName = "MutationSwarm/Wave Config")]
    public class SO_WaveConfig : ScriptableObject
    {
        [Header("Cantidad de enemigos")]
        public int baseEnemiesPerWave = 10;
        public int enemiesScalingPerWave = 3;

        [Header("Spawning")]
        public float baseTimeBetweenSpawns = 1.5f;
        public float minTimeBetweenSpawns = 0.3f;
        public float spawnReductionPerWave = 0.05f;

        [Header("Fases")]
        public float evolutionPhaseDuration = 3f;

        [Header("Referencias de escena")]
        public Transform[] spawnPoints;
    }
}
