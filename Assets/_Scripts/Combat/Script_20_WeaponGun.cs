using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Arma del pack Guns V1.01 con sprite y proyectil configurables.
    /// </summary>
    public class Script_20_WeaponGun : Script_20_WeaponBase
    {
        [SerializeField] private SO_WeaponData _data;
        [SerializeField] private SpriteRenderer _gunRenderer;

        public SO_WeaponData Data => _data;

        private void Awake()
        {
            if (_firePoint == null)
            {
                Transform player = transform.parent;
                if (player != null)
                {
                    Transform fp = player.Find("FirePoint");
                    if (fp != null)
                        _firePoint = fp;
                }
            }

            if (_gunRenderer == null)
                _gunRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(SO_WeaponData data)
        {
            _data = data;
            if (_data == null)
                return;

            _projectilePoolKey = _data.projectilePoolKey;
            _fireRate = _data.fireRate;

            if (_gunRenderer == null)
                _gunRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_gunRenderer != null)
            {
                _gunRenderer.sprite = _data.gunSprite;
                _gunRenderer.transform.localPosition = _data.gunOffset;
            }
        }

        public override void Fire(Vector2 direction)
        {
            if (_data == null || Time.time < _nextFireTime)
                return;

            _nextFireTime = Time.time + _fireRate;

            if (MutationSwarm.Core.Script_04_ObjectPool.Instance == null)
                return;

            GameObject proj = MutationSwarm.Core.Script_04_ObjectPool.Instance.Get(_projectilePoolKey);
            if (proj == null)
                return;

            proj.transform.position = _firePoint != null ? _firePoint.position : transform.position;
            if (proj.TryGetComponent(out Script_19_Projectile projectile))
            {
                projectile.Configure(_data.damage, _data.projectileSpeed, _data.projectileLifetime, _data.projectileSprite);
                projectile.Launch(direction);
            }
        }
    }
}
