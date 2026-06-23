using UnityEngine;

namespace MutationSwarm
{
    public class PoisonCloud : MonoBehaviour
    {
        float _life = 5f;
        SpriteRenderer _sr;

        void Start()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite        = SpriteFactory.Instance.CircleSprite;
            _sr.color         = new Color(0.2f, 1f, 0.2f, 0.35f);
            _sr.sortingOrder  = 2;
            transform.localScale = Vector3.one * 1.5f;
        }

        void Update()
        {
            _life -= Time.deltaTime;
            if (_sr) _sr.color = new Color(0.2f, 1f, 0.2f, 0.35f * (_life / 5f));

            if (_life <= 0f) { Destroy(gameObject); return; }

            if (PlayerController.Instance == null) return;
            float d = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (d < 1.2f)
            {
                PlayerStats.Instance?.TakeDamage(3f * Time.deltaTime);
                if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.poisonDmgTaken += 3f * Time.deltaTime;
            }
        }
    }
}
