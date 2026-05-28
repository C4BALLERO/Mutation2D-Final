using UnityEngine;

namespace MutationSwarm.Building
{
    /// <summary>
    /// Clase base para torretas, barricadas, trampas, etc.
    /// </summary>
    public abstract class Script_24_StructureBase : MonoBehaviour
    {
        [SerializeField] protected SO_StructureData _data;
        [SerializeField] protected float _currentHp;
        protected float _lifetimeRemaining;

        public virtual void Initialize(SO_StructureData data, Vector2 position)
        {
            _data = data;
            _currentHp = data.maxHp;
            _lifetimeRemaining = data.lifetime;
            transform.position = position;
        }

        protected virtual void Update()
        {
            if (_data != null && _lifetimeRemaining > 0f)
            {
                _lifetimeRemaining -= Time.deltaTime;
                if (_lifetimeRemaining <= 0f)
                    DestroyStructure();
            }
        }

        public virtual void TakeDamage(float amount)
        {
            _currentHp -= amount;
            if (_currentHp <= 0f)
                DestroyStructure();
        }

        protected virtual void DestroyStructure() => gameObject.SetActive(false);
    }
}
