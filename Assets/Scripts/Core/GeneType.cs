using UnityEngine;

namespace MutationSwarm
{
    public enum GeneType { None, Poison, Speed, Spiny, Armored, Psychic, Corrupt }

    public static class GeneData
    {
        public static Color GetColor(GeneType g) => g switch
        {
            GeneType.Poison  => new Color(0.27f, 1f,   0.27f),
            GeneType.Speed   => new Color(1f,    0.93f, 0f),
            GeneType.Spiny   => new Color(1f,    0.2f,  0.13f),
            GeneType.Armored => new Color(0.27f, 0.53f, 1f),
            GeneType.Psychic => new Color(0.8f,  0.27f, 1f),
            GeneType.Corrupt => new Color(0.07f, 0.07f, 0.07f),
            _                => new Color(0.63f, 0.63f, 0.69f),
        };

        public static string GetName(GeneType g) => g switch
        {
            GeneType.Poison  => "VENENO",
            GeneType.Speed   => "VELOCIDAD",
            GeneType.Spiny   => "ESPINAS",
            GeneType.Armored => "ARMADURA",
            GeneType.Psychic => "PSIQUICO",
            GeneType.Corrupt => "CORRUPTO",
            _                => "NINGUNA",
        };
    }
}
