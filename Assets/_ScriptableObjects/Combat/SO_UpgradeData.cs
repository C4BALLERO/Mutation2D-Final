using UnityEngine;

namespace MutationSwarm.UI
{
    [CreateAssetMenu(fileName = "SO_UpgradeData", menuName = "MutationSwarm/Upgrade Data")]
    public class SO_UpgradeData : ScriptableObject
    {
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;
        public float numericEffect;
        public string effectId;
    }
}
