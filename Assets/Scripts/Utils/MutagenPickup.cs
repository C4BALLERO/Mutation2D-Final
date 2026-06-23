using UnityEngine;

namespace MutationSwarm
{
    // Dropped by bosses (always) and corrupt enemies (sometimes). Collecting enough
    // grants the player a random mutation. Glowing green orb that homes toward you.
    public class MutagenPickup : MonoBehaviour
    {
        public int Amount = 1;
        float _life = 12f;
        SpriteRenderer _sr;
        float _t;

        void Start()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite       = SpriteFactory.Instance != null ? SpriteFactory.Instance.CircleSprite : null;
            _sr.color        = new Color(0.6f, 1f, 0.15f);
            _sr.sortingOrder = 4;
            transform.localScale = Vector3.one * 0.45f;
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        }

        void Update()
        {
            _t += Time.deltaTime;
            _life -= Time.deltaTime;
            float p = 0.45f + 0.09f * Mathf.Sin(_t * 8f);
            transform.localScale = Vector3.one * p;
            if (_sr) _sr.color = new Color(0.6f, 1f, 0.15f, Mathf.Min(1f, _life));
            if (_life <= 0f) { Destroy(gameObject); return; }

            if (PlayerController.Instance == null) return;
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist < 3f)
                transform.position = Vector2.MoveTowards(transform.position,
                    PlayerController.Instance.transform.position, 7f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.GetComponent<PlayerController>() != null)
            {
                PlayerStats.Instance?.AddMutagen(Amount);
                AudioManager.Instance?.PlayPickup();
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(0.6f, 1f, 0.2f), 8, 5f);
                Destroy(gameObject);
            }
        }
    }
}
