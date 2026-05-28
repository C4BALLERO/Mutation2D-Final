using System;
using UnityEngine;

namespace MutationSwarm.Evolution
{
    /// <summary>
    /// Genoma serializable con todos los genes del enemigo.
    /// </summary>
    [Serializable]
    public class Genome
    {
        public const float VelocidadMin = 0.5f, VelocidadMax = 4.0f;
        public const float TamañoMin = 0.5f, TamañoMax = 3.0f;
        public const float SaltoMin = 0.0f, SaltoMax = 1.0f;
        public const float ArmaduraMin = 0.0f, ArmaduraMax = 0.8f;
        public const float VenenoMin = 0.0f, VenenoMax = 1.0f;
        public const float ExplosionMin = 0.0f, ExplosionMax = 1.0f;
        public const float RegeneracionMin = 0.0f, RegeneracionMax = 0.5f;
        public const float RangoVisionMin = 0.1f, RangoVisionMax = 1.0f;
        public const float ComportamientoGrupalMin = 0.0f, ComportamientoGrupalMax = 1.0f;
        public const float ResistenciaFuegoMin = 0.0f, ResistenciaFuegoMax = 1.0f;
        public const float ResistenciaElectricaMin = 0.0f, ResistenciaElectricaMax = 1.0f;
        public const float EspinasMin = 0.0f, EspinasMax = 1.0f;

        public float Velocidad = 1.0f;
        public float Tamaño = 1.0f;
        public float Salto = 0.0f;
        public float Armadura = 0.0f;
        public float Veneno = 0.0f;
        public float ExplosionAlMorir = 0.0f;
        public float Regeneracion = 0.0f;
        public float RangoVision = 0.5f;
        public float ComportamientoGrupal = 0.0f;
        public float ResistenciaFuego = 0.0f;
        public float ResistenciaElectrica = 0.0f;
        public float Espinas = 0.0f;

        public event Action<string, float, float> OnMutationOccurred;

        public string GetDominantGene() { return "Velocidad"; }
        public Color GetMutationColor() => Color.red;
        public float GetFitnessModifier() => 1.0f;
        public void Mutate(float intensity = 0.2f) { }
        public static Genome Crossover(Genome parentA, Genome parentB) => parentA?.Clone() ?? new Genome();
        public string ToJson() => JsonUtility.ToJson(this);
        public static Genome FromJson(string json) => JsonUtility.FromJson<Genome>(json) ?? new Genome();
        public Genome Clone() => FromJson(ToJson());
    }
}
