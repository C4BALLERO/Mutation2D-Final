using UnityEngine;

namespace MutationSwarm.Core
{
    [CreateAssetMenu(fileName = "SO_PoolConfig", menuName = "MutationSwarm/Pool Config")]
    public class SO_PoolConfig : ScriptableObject
    {
        public string poolKey;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
    }
}
