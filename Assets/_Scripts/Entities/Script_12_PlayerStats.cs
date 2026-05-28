using System;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// HP, upgrades y estadísticas acumuladas del jugador.
    /// </summary>
    public class Script_12_PlayerStats : MonoBehaviour
    {
        [SerializeField] private float _maxHp = 100f;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _jumpForce = 12f;
        [SerializeField] private float _dashForce = 18f;
        [SerializeField] private float _dashCooldown = 1.2f;

        [SerializeField] private bool _hasDoubleJump;
        [SerializeField] private bool _hasWallJump;

        public float MaxHp => _maxHp;
        public float CurrentHp { get; private set; }
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float DashForce => _dashForce;
        public float DashCooldown => _dashCooldown;
        public bool HasDoubleJump => _hasDoubleJump;
        public bool HasWallJump => _hasWallJump;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        private void Awake() => CurrentHp = _maxHp;

        public void TakeDamage(float amount)
        {
            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            OnHealthChanged?.Invoke(CurrentHp, _maxHp);
            if (CurrentHp <= 0f)
                OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            CurrentHp = Mathf.Min(_maxHp, CurrentHp + amount);
            OnHealthChanged?.Invoke(CurrentHp, _maxHp);
        }
    }
}
