using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Clase base de arma: dispara proyectiles desde ObjectPool.
    /// </summary>
    public abstract class Script_20_WeaponBase : MonoBehaviour
    {
        [SerializeField] protected string _projectilePoolKey = "Projectile_Basic";
        [SerializeField] protected float _fireRate = 0.2f;
        [SerializeField] protected Transform _firePoint;

        protected float _nextFireTime;

        public virtual void Fire(Vector2 direction)
        {
            if (Time.time < _nextFireTime)
                return;

            _nextFireTime = Time.time + _fireRate;

            if (Script_04_ObjectPool.Instance == null)
                return;

            GameObject proj = Script_04_ObjectPool.Instance.Get(_projectilePoolKey);
            if (proj == null)
                return;

            proj.transform.position = _firePoint != null ? _firePoint.position : transform.position;
            if (proj.TryGetComponent(out Script_19_Projectile projectile))
                projectile.Launch(direction);
        }
    }
}
