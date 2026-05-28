using UnityEngine;

namespace MutationSwarm.Building
{
    public enum StructureType
    {
        TurretBasic,
        Barricade,
        FloorTrap,
        Mine,
        TemporaryPlatform
    }

    [CreateAssetMenu(fileName = "SO_StructureData", menuName = "MutationSwarm/Structure Data")]
    public class SO_StructureData : ScriptableObject
    {
        public StructureType structureType;
        public string structureName;
        public GameObject prefab;
        public int materialCost = 5;
        public float maxHp = 100f;
        public float lifetime = 30f;
        public float fireRate = 1f;
        public float range = 5f;
        public float arcAngle = 180f;
        public string projectilePoolKey = "Projectile_Basic";
        public float trapSlowPercent = 0.5f;
        public float mineRadius = 2f;
        public bool trapExplodes;
        public Vector2 footprint = new(1f, 1f);
    }
}
