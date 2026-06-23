using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        public float Hp      { get; private set; } = 100f;
        public float MaxHp   { get; set; } = 100f;
        public float Dna     { get; private set; }
        public List<string> Upgrades { get; private set; } = new();

        public bool HasUpgrade(string id) => Upgrades.Contains(id);

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        float _nextHurtSound;

        public void TakeDamage(float amount)
        {
            Hp = Mathf.Max(0f, Hp - amount);
            // Contact damage calls this every frame — throttle the hurt sound.
            if (amount > 0.01f && Hp > 0f && Time.unscaledTime >= _nextHurtSound)
            {
                AudioManager.Instance?.PlayHurt();
                _nextHurtSound = Time.unscaledTime + 0.4f;
            }
            if (Hp <= 0f) GameManager.Instance.PlayerDied();
        }

        public void Heal(float amount) => Hp = Mathf.Min(MaxHp, Hp + amount);
        public void AddDna(float amount) => Dna += amount;
        public bool SpendDna(float amount)
        {
            if (Dna < amount) return false;
            Dna -= amount; return true;
        }
        public void AddUpgrade(string id) => Upgrades.Add(id);
    }
}
