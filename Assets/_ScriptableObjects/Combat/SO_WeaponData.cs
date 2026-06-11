using UnityEngine;

namespace MutationSwarm.Combat
{
    [CreateAssetMenu(fileName = "SO_WeaponData", menuName = "MutationSwarm/Weapon Data")]
    public class SO_WeaponData : ScriptableObject
    {
        public string weaponId;
        public string displayName;
        [TextArea] public string description;

        [Header("Visual")]
        public Sprite gunSprite;
        public Sprite projectileSprite;
        public Vector2 gunOffset = new(0.35f, 0.05f);

        [Header("Combate")]
        public float fireRate = 0.2f;
        public float damage = 10f;
        public float projectileSpeed = 14f;
        public float projectileLifetime = 3f;
        public string projectilePoolKey = "Projectile_Basic";

        [Header("Tienda")]
        public int materialCost = 25;
        public bool unlockedByDefault;
    }
}
