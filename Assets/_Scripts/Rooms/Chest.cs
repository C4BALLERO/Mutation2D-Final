using System.Collections;
using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Rooms
{
    /// <summary>
    /// Collectible chest. Opens when a player interacts or enters range.
    /// Publishes ChestOpenedEvent and plays a scale-bounce animation.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Chest : MonoBehaviour
    {
        [SerializeField] private float   _healAmount = 25f;
        [SerializeField] private Color   _closedColor = new(0.85f, 0.65f, 0.1f, 1f);
        [SerializeField] private Color   _openedColor = new(0.6f, 0.45f, 0.05f, 1f);

        private SpriteRenderer _sr;
        private bool _opened;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _sr.color = _closedColor;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_opened) return;
            if (!other.TryGetComponent(out Entities.Script_12_PlayerStats stats)) return;

            Open(stats);
        }

        private void Open(Entities.Script_12_PlayerStats player)
        {
            _opened = true;
            if (_sr != null) _sr.color = _openedColor;

            // Heal the player
            player.Heal(_healAmount);

            Script_03_EventBus.Publish(new ChestOpenedEvent
            {
                position   = transform.position,
                healAmount = _healAmount
            });

            StartCoroutine(BounceAnimation());
        }

        private IEnumerator BounceAnimation()
        {
            Vector3 origin = transform.localScale;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 6f;
                float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                transform.localScale = origin * s;
                yield return null;
            }
            transform.localScale = origin;
        }
    }
}
