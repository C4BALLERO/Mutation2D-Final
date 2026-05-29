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

        public string GetDominantGene()
        {
            float best = -1f;
            string name = "Velocidad";
            Evaluate("Velocidad", Velocidad, VelocidadMin, VelocidadMax, ref best, ref name);
            Evaluate("Tamaño", Tamaño, TamañoMin, TamañoMax, ref best, ref name);
            Evaluate("Salto", Salto, SaltoMin, SaltoMax, ref best, ref name);
            Evaluate("Armadura", Armadura, ArmaduraMin, ArmaduraMax, ref best, ref name);
            Evaluate("Veneno", Veneno, VenenoMin, VenenoMax, ref best, ref name);
            Evaluate("ExplosionAlMorir", ExplosionAlMorir, ExplosionMin, ExplosionMax, ref best, ref name);
            Evaluate("Regeneracion", Regeneracion, RegeneracionMin, RegeneracionMax, ref best, ref name);
            Evaluate("RangoVision", RangoVision, RangoVisionMin, RangoVisionMax, ref best, ref name);
            Evaluate("ComportamientoGrupal", ComportamientoGrupal, ComportamientoGrupalMin, ComportamientoGrupalMax, ref best, ref name);
            Evaluate("ResistenciaFuego", ResistenciaFuego, ResistenciaFuegoMin, ResistenciaFuegoMax, ref best, ref name);
            Evaluate("ResistenciaElectrica", ResistenciaElectrica, ResistenciaElectricaMin, ResistenciaElectricaMax, ref best, ref name);
            Evaluate("Espinas", Espinas, EspinasMin, EspinasMax, ref best, ref name);
            return name;
        }

        public Color GetMutationColor()
        {
            return GetDominantGene() switch
            {
                "Veneno" => new Color(0.2f, 0.9f, 0.2f),
                "Velocidad" => new Color(0.95f, 0.2f, 0.2f),
                "Armadura" or "ResistenciaFuego" or "ResistenciaElectrica" => new Color(0.2f, 0.45f, 0.95f),
                "Salto" or "ComportamientoGrupal" => new Color(0.95f, 0.9f, 0.2f),
                "Regeneracion" => new Color(0.65f, 0.2f, 0.9f),
                "ExplosionAlMorir" or "Espinas" => new Color(0.1f, 0.1f, 0.1f),
                _ => new Color(0.9f, 0.3f, 0.3f)
            };
        }

        public float GetFitnessModifier()
        {
            float avg = (Velocidad + Armadura + Veneno + Regeneracion + Espinas) / 5f;
            return Mathf.Clamp(0.5f + avg * 0.5f, 0.5f, 2f);
        }

        private static void Evaluate(string geneName, float value, float min, float max, ref float best, ref string bestName)
        {
            float normalized = max <= min ? 0f : (value - min) / (max - min);
            if (normalized > best)
            {
                best = normalized;
                bestName = geneName;
            }
        }
        public void Mutate(float intensity = 0.2f) { }
        public static Genome Crossover(Genome parentA, Genome parentB) => parentA?.Clone() ?? new Genome();
        public string ToJson() => JsonUtility.ToJson(this);
        public static Genome FromJson(string json) => JsonUtility.FromJson<Genome>(json) ?? new Genome();
        public Genome Clone() => FromJson(ToJson());
    }
}
