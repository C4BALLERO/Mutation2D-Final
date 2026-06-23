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

        // ── Combo / multiplier ──────────────────────────────────────────
        public int   Combo      { get; private set; }
        public float ComboTimer { get; private set; }
        public int   Multiplier => Combo >= 20 ? 4 : Combo >= 10 ? 3 : Combo >= 5 ? 2 : 1;
        const float COMBO_WINDOW = 3.5f;

        // ── Overdrive / Fury ────────────────────────────────────────────
        public float Fury           { get; private set; }   // 0..100
        public bool  Overdrive      { get; private set; }
        public float OverdriveTimer { get; private set; }
        const float OVERDRIVE_DUR = 5f;

        // ── Mutations ───────────────────────────────────────────────────
        public List<string> Mutations { get; private set; } = new();
        public int   Mutagen { get; private set; }
        public const int MUTAGEN_PER_MUTATION = 3;
        public string LastMutationMsg      { get; private set; }
        public float  LastMutationMsgTimer { get; private set; }

        static readonly string[] AllMutations = { "toxicBlood", "lifesteal", "berserk", "volatile" };

        float _nextHurtSound;

        public bool HasUpgrade(string id)  => Upgrades.Contains(id);
        public bool HasMutation(string id) => Mutations.Contains(id);

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;

            if (ComboTimer > 0f)
            {
                ComboTimer -= Time.deltaTime;
                if (ComboTimer <= 0f) Combo = 0;
            }
            if (Overdrive)
            {
                OverdriveTimer -= Time.deltaTime;
                if (OverdriveTimer <= 0f) Overdrive = false;
            }
            if (LastMutationMsgTimer > 0f) LastMutationMsgTimer -= Time.deltaTime;
        }

        public void TakeDamage(float amount)
        {
            if (Overdrive) return; // invulnerable while in Fury mode

            Hp = Mathf.Max(0f, Hp - amount);
            Combo = 0; ComboTimer = 0f; // taking a hit breaks the combo

            // Contact damage calls this every frame — throttle the hurt sound.
            if (amount > 0.01f && Hp > 0f && Time.unscaledTime >= _nextHurtSound)
            {
                AudioManager.Instance?.PlayHurt();
                _nextHurtSound = Time.unscaledTime + 0.4f;
            }
            if (Hp <= 0f) GameManager.Instance.PlayerDied();
        }

        // Called from EnemyBase.Die when an enemy is killed.
        public void OnEnemyKilled(bool isBoss)
        {
            Combo++;
            ComboTimer = COMBO_WINDOW;
            Fury = Mathf.Min(100f, Fury + (isBoss ? 60f : 7f));
            if (HasMutation("lifesteal")) Heal(isBoss ? 25f : 2.5f);
        }

        // ── Overdrive ──
        public bool CanOverdrive => Fury >= 100f && !Overdrive;
        public void ActivateOverdrive()
        {
            if (!CanOverdrive) return;
            Overdrive = true; OverdriveTimer = OVERDRIVE_DUR; Fury = 0f;
            AudioManager.Instance?.PlayWave();
            CameraFollow.Instance?.Shake(0.4f, 0.3f);
            // Shockwave: push & damage nearby enemies.
            if (WaveManager.Instance != null)
                foreach (var e in WaveManager.Instance.ActiveEnemies.ToArray())
                {
                    if (e == null) continue;
                    float d = Vector2.Distance(transform.position, e.transform.position);
                    if (d < 6f) e.TakeDamage(40f);
                }
            ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f, 0.4f, 0.1f), 30, 10f);
        }

        // ── Mutations ──
        public void AddMutagen(int n = 1)
        {
            Mutagen += n;
            while (Mutagen >= MUTAGEN_PER_MUTATION && Mutations.Count < AllMutations.Length)
            {
                Mutagen -= MUTAGEN_PER_MUTATION;
                GrantRandomMutation();
            }
        }

        void GrantRandomMutation()
        {
            var avail = new List<string>();
            foreach (var m in AllMutations) if (!Mutations.Contains(m)) avail.Add(m);
            if (avail.Count == 0) return;
            string pick = avail[Random.Range(0, avail.Count)];
            Mutations.Add(pick);
            LastMutationMsg = MutationName(pick);
            LastMutationMsgTimer = 4.5f;
            AudioManager.Instance?.PlayUpgrade();
            ParticleManager.Instance?.SpawnBurst(transform.position, new Color(0.6f, 1f, 0.2f), 20, 7f);
        }

        public static string MutationName(string id) => id switch
        {
            "toxicBlood" => "SANGRE TÓXICA",
            "lifesteal"  => "ROBO DE VIDA",
            "berserk"    => "FRENESÍ",
            "volatile"   => "VOLÁTIL",
            _ => id,
        };

        public static string MutationDesc(string id) => id switch
        {
            "toxicBlood" => "Los enemigos se dañan al tocarte",
            "lifesteal"  => "Te curas al matar enemigos",
            "berserk"    => "+60% de daño con poca vida",
            "volatile"   => "Los enemigos explotan al morir",
            _ => id,
        };

        public void Heal(float amount) => Hp = Mathf.Min(MaxHp, Hp + amount);
        public void AddDna(float amount) => Dna += amount * Multiplier;
        public bool SpendDna(float amount)
        {
            if (Dna < amount) return false;
            Dna -= amount; return true;
        }
        public void AddUpgrade(string id) => Upgrades.Add(id);
    }
}
