using MutationSwarm.Combat;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Movimiento 2D: salto, dash, wall jump y combate por jugador (0–3).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Script_12_PlayerStats))]
    public class Script_11_PlayerController : MonoBehaviour
    {
        [SerializeField] private int _playerIndex;
        [SerializeField] private Script_20_WeaponBase _primaryWeapon;

        private Script_12_PlayerStats _stats;
        private Rigidbody2D _rb;
        private Animator _animator;

        public int PlayerIndex => _playerIndex;

        private void Awake()
        {
            _stats = GetComponent<Script_12_PlayerStats>();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Implementación completa en PROMPT 04
        }
    }
}
