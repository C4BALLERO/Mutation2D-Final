using UnityEngine;

namespace MutationSwarm
{
    public class PlayerShooter : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public Transform  muzzle;
        public float      bulletSpeed = 18f;
        public float      baseCooldown = 0.2f;

        float _cd;
        int   _electricCounter;
        Camera _cam;

        void Start() => _cam = Camera.main;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;
            _cd -= Time.deltaTime;

            float cd = baseCooldown;
            if (PlayerStats.Instance.HasUpgrade("fasterReload")) cd *= 0.45f;
            if (PlayerStats.Instance.Overdrive) cd *= 0.35f; // rapid fire during Fury

            if (Input.GetMouseButton(0) && _cd <= 0f)
            {
                Shoot(cd);
                _cd = cd;
            }
        }

        void Shoot(float cd)
        {
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 dir = ((Vector2)(mouseWorld - muzzle.position)).normalized;

            float dmg = 12f + (PlayerStats.Instance.HasUpgrade("moreDamage") ? 8f : 0f);
            // Mutación FRENESÍ: +60% de daño con poca vida.
            if (PlayerStats.Instance.HasMutation("berserk") && PlayerStats.Instance.Hp < PlayerStats.Instance.MaxHp * 0.3f)
                dmg *= 1.6f;
            if (PlayerStats.Instance.Overdrive) dmg *= 1.5f; // Furia potencia el daño
            int piercing = PlayerStats.Instance.HasUpgrade("piercing") ? 2 : 0;

            _electricCounter++;
            bool isElectric = PlayerStats.Instance.HasUpgrade("electric") && _electricCounter % 5 == 0;

            var go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            go.SetActive(true);
            var b  = go.GetComponent<Bullet>();
            b.Init(dir, bulletSpeed, dmg, piercing, isElectric, fromPlayer: true);

            if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.bulletsShot++;
            AudioManager.Instance?.PlayShoot();
            ParticleManager.Instance?.SpawnBurst(muzzle.position, new Color(1f,1f,0.5f), 3, 5f);

            // Face direction of mouse
            var pc = PlayerController.Instance;
            if (pc != null) transform.localScale = new Vector3(mouseWorld.x < transform.position.x ? -1 : 1, 1, 1);
        }
    }
}
