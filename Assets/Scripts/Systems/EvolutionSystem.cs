using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm
{
    public class EvolutionSystem : MonoBehaviour
    {
        public static EvolutionSystem Instance { get; private set; }

        readonly Dictionary<GeneType, float> _weights = new()
        {
            { GeneType.Poison,  1f },
            { GeneType.Speed,   1f },
            { GeneType.Spiny,   1f },
            { GeneType.Armored, 1f },
            { GeneType.Psychic, 1f },
            { GeneType.Corrupt, 0.2f },
        };

        public GeneType DominantGene { get; private set; } = GeneType.None;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public GeneType RollGene()
        {
            float total = 0f;
            foreach (var w in _weights.Values) total += w;
            float r = Random.value * total;
            foreach (var kv in _weights)
            {
                r -= kv.Value;
                if (r <= 0f) return kv.Key;
            }
            return GeneType.None;
        }

        public void Evolve(WaveStats s, int waveNum)
        {
            if (s.Accuracy > 0.55f)       Bump(GeneType.Armored,  1.5f);
            if (s.dashUsed > 4)            Bump(GeneType.Speed,    1.2f);
            if (s.HighRatio > 0.35f)       Bump(GeneType.Psychic,  1.3f);
            if (s.defensesBuilt > 1)       Bump(GeneType.Spiny,    1.4f);
            if (s.poisonDmgTaken > 10f)    Bump(GeneType.Poison,   0.8f);
            if (s.contactHitsFromSpeed > 2) Bump(GeneType.Speed,   0.5f);
            if (s.defKilled > 0)           Bump(GeneType.Spiny,    0.8f);
            if (s.dashUsed < 2 && s.bulletsHit > 10) Bump(GeneType.Speed, 0.6f);

            if (waveNum % 5 == 0) Bump(GeneType.Corrupt, 0.8f);

            // Natural drift
            var keys = new List<GeneType>(_weights.Keys);
            foreach (var k in keys)
            {
                float v = _weights[k] * 0.95f + 0.1f;
                _weights[k] = k == GeneType.Corrupt ? Mathf.Min(6f, v) : Mathf.Clamp(v, 0.3f, 10f);
            }
        }

        void Bump(GeneType g, float amount)
        {
            float cap = g == GeneType.Corrupt ? 6f : 10f;
            _weights[g] = Mathf.Min(cap, _weights[g] + amount);
        }

        public void UpdateDominant(List<EnemyBase> enemies)
        {
            var counts = new Dictionary<GeneType, int>();
            foreach (var e in enemies)
            {
                if (!counts.ContainsKey(e.Gene)) counts[e.Gene] = 0;
                counts[e.Gene]++;
            }
            GeneType dom = GeneType.None; int max = 0;
            foreach (var kv in counts)
                if (kv.Value > max) { max = kv.Value; dom = kv.Key; }
            DominantGene = dom;
        }

        public Dictionary<GeneType, float> GetWeightsCopy()
        {
            return new Dictionary<GeneType, float>(_weights);
        }
    }
}
