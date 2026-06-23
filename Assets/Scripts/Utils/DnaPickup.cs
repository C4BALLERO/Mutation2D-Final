using UnityEngine;

namespace MutationSwarm
{
    public class DnaPickup : MonoBehaviour
    {
        public float Amount = 5f;
        float _life = 8f;
        SpriteRenderer _sr;
        float _t;

        void Start()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite       = SpriteFactory.Instance.DiamondSprite;
            _sr.color        = new Color(0f, 1f, 0.8f);
            _sr.sortingOrder = 3;
            transform.localScale = Vector3.one * 0.4f;
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
            tag = "DNAPickup";
        }

        void Update()
        {
            _t += Time.deltaTime;
            _life -= Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, 0, _t * 90f);
            if (_sr) _sr.color = new Color(0f, 1f, 0.8f, Mathf.Min(1f, _life));

            if (_life <= 0f) { Destroy(gameObject); return; }

            // Attract to player
            if (PlayerController.Instance == null) return;
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist < 2.5f)
                transform.position = Vector2.MoveTowards(transform.position,
                    PlayerController.Instance.transform.position, 8f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.GetComponent<PlayerController>() != null)
            {
                PlayerStats.Instance.AddDna(Amount);
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(0f,1f,0.8f), 4, 3f);
                Destroy(gameObject);
            }
        }
    }
}
