using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Rooms
{
    /// <summary>
    /// Single-use checkpoint. When a player enters the trigger zone the
    /// spawn position is saved globally so the next death respawns here.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointController : MonoBehaviour
    {
        /// <summary>Current active respawn position across all scenes.</summary>
        public static Vector2 LastCheckpoint { get; private set; }

        [SerializeField] private Color _inactiveColor = new(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _activeColor   = new(0.2f, 1f, 0.4f, 1f);

        private SpriteRenderer _sr;
        private bool _activated;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _sr.color = _inactiveColor;

            // Mark trigger
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_activated) return;
            if (!other.TryGetComponent<Entities.Script_11_PlayerController>(out _)) return;

            Activate();
        }

        private void Activate()
        {
            _activated = true;
            LastCheckpoint = transform.position;

            if (_sr != null)
                _sr.color = _activeColor;

            Script_03_EventBus.Publish(new CheckpointActivatedEvent
            {
                position = transform.position
            });
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _activated ? _activeColor : _inactiveColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 2f, 0.1f));
        }
    }
}
