using UnityEngine;

namespace MutationSwarm.Evolution
{
    /// <summary>
    /// Counter-strategy: boosts enemy traits that counter the player's dominant behavior.
    /// Called after crossover/mutation to steer the next generation.
    /// </summary>
    public static class Script_10_AdaptivePressure
    {
        private const float PressureStrength = 0.15f;

        public static void Apply(PlayerBehaviorData playerData, Genome genome)
        {
            if (genome == null) return;

            // Counter ranged weapons → more Armadura and RangoVision
            if (playerData.rangedDamageTotal > playerData.meleeDamageTotal)
            {
                genome.Armadura    = Mathf.Min(Genome.ArmaduraMax,  genome.Armadura    + PressureStrength * 0.5f);
                genome.RangoVision = Mathf.Min(Genome.RangoVisionMax, genome.RangoVision + PressureStrength * 0.3f);
            }

            // Counter melee weapons → more Espinas and Velocidad (punish melee, escape quickly)
            if (playerData.meleeDamageTotal > playerData.rangedDamageTotal)
            {
                genome.Espinas    = Mathf.Min(Genome.EspinasMax,   genome.Espinas    + PressureStrength * 0.5f);
                genome.Velocidad  = Mathf.Min(Genome.VelocidadMax, genome.Velocidad  + PressureStrength * 0.3f);
            }

            // Player stays high → enemies get better Salto to follow
            if (playerData.avgYPosition > 2f)
                genome.Salto = Mathf.Min(Genome.SaltoMax, genome.Salto + PressureStrength * 0.4f);

            // Turret usage → boost ComportamientoGrupal (swarm to overwhelm)
            if (playerData.mostUsedStructure == "Turret")
                genome.ComportamientoGrupal = Mathf.Min(
                    Genome.ComportamientoGrupalMax,
                    genome.ComportamientoGrupal + PressureStrength * 0.6f);
        }
    }
}
