using UnityEngine;

namespace MutationSwarm.Building
{
    /// <summary>
    /// Barricada física con vida y barra world-space opcional.
    /// </summary>
    public class BarricadeStructure : Script_24_StructureBase
    {
        [SerializeField] private GameObject _crackVfx;

        public override void TakeDamage(float amount)
        {
            base.TakeDamage(amount);
            if (_currentHp <= 0f && _crackVfx != null)
                Instantiate(_crackVfx, transform.position, Quaternion.identity);
        }
    }
}
