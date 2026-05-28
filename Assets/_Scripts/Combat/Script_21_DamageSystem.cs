using MutationSwarm.Entities;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Combat
{
    public enum DamageType
    {
        Physical,
        Fire,
        Electric,
        Poison
    }

    /// <summary>
    /// Cálculo de daño con resistencias según Genome.
    /// </summary>
    public static class Script_21_DamageSystem
    {
        public static float CalculateDamage(
            float baseDamage,
            DamageType type,
            Genome targetGenome)
        {
            if (targetGenome == null)
                return baseDamage;

            float resistance = type switch
            {
                DamageType.Fire => targetGenome.ResistenciaFuego,
                DamageType.Electric => targetGenome.ResistenciaElectrica,
                _ => targetGenome.Armadura
            };

            return baseDamage * (1f - resistance);
        }

        public static void ApplyToEnemy(Script_13_EnemyBase enemy, float damage, DamageType type)
        {
            // Integración con HP del enemigo en implementación completa
        }

        public static void ApplyToPlayer(Script_12_PlayerStats stats, float damage)
        {
            stats?.TakeDamage(damage);
        }
    }
}
