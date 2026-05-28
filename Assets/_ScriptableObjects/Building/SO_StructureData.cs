using UnityEngine;

namespace MutationSwarm.Building
{
    [CreateAssetMenu(fileName = "SO_StructureData", menuName = "MutationSwarm/Structure Data")]
    public class SO_StructureData : ScriptableObject
    {
        public string structureName;
        public GameObject prefab;
        public int materialCost = 5;
        public float maxHp = 100f;
        public float lifetime;
        public float fireRate = 1f;
        public float range = 5f;
    }
}
