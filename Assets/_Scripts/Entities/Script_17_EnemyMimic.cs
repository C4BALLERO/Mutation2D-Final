using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Mimic: copies the movement scale of the nearest player,
    /// making it harder to outrun or predict.
    /// Uses LateUpdate so the base class Update (AI tick) still runs normally.
    /// </summary>
    public class Script_17_EnemyMimic : Script_13_EnemyBase
    {
        [SerializeField] private float _copyUpdateInterval = 2f;

        private float _copyTimer;
        private bool _mimicEventFired;

        private void LateUpdate()
        {
            if (Genome == null) return;

            _copyTimer -= Time.deltaTime;
            if (_copyTimer > 0f) return;

            _copyTimer = _copyUpdateInterval;
            TryCopyNearestPlayer();
        }

        private void TryCopyNearestPlayer()
        {
            Transform player = GetNearestPlayer();
            if (player == null) return;

            if (!player.TryGetComponent(out Script_12_PlayerStats stats)) return;

            // Scale visual size relative to player speed (faster player → larger mimic threat)
            float targetScale = Mathf.Clamp(stats.MoveSpeed / 5f, 0.5f, 2.5f);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, 0.3f);

            if (!_mimicEventFired)
            {
                _mimicEventFired = true;
                Script_03_EventBus.Publish(new MimicActivatedEvent { mimic = this, target = player });
            }
        }
    }
}
