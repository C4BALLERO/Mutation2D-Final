using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Entrada unificada para 1–4 gamepads y teclado/ratón.
    /// </summary>
    public class Script_06_InputManager : MonoBehaviour
    {
        public static Script_06_InputManager Instance { get; private set; }

        [SerializeField] private InputActionAsset _inputActions;

        public event Action<int> OnPlayerJoined;
        public event Action<int> OnPlayerLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Obtiene el movimiento del jugador indicado (0–3).
        /// </summary>
        public Vector2 GetMoveInput(int playerIndex) => Vector2.zero;

        public bool GetJumpPressed(int playerIndex) => false;
        public bool GetDashPressed(int playerIndex) => false;
        public bool GetFirePressed(int playerIndex) => false;
    }
}
