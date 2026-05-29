using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Arma básica concreta para asignar en Prefab_Player.
    /// </summary>
    public class Script_20_WeaponBasic : Script_20_WeaponBase
    {
        [SerializeField] private string _weaponDisplayName = "Rifle";

        public string WeaponDisplayName => _weaponDisplayName;
    }
}
