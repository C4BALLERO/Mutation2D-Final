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

        [Header("Escalado de dificultad")]
        public float speedMultiplierPerWave       = 0.08f;  // +8% velocidad por oleada
        public float hpMultiplierPerWave          = 0.12f;  // +12% HP por oleada
        public float damageMultiplierPerWave      = 0.10f;  // +10% daño por oleada
        public float attackRangeMultiplierPerWave = 0.02f;  // +2% rango por oleada
        public int   maxEnemiesCap                = 80;

        [Header("Referencias de escena")]
        public Transform[] spawnPoints;
    }
}
