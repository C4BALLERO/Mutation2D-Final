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

        public virtual void Initialize(SO_StructureData data, Vector2 position)
        {
            _data = data;
            _currentHp = data.maxHp;
            transform.position = position;
        }

        protected virtual void Update()
        {
            if (_data != null && _data.lifetime > 0f)
            {
                _data.lifetime -= Time.deltaTime;
                if (_data.lifetime <= 0f)
                    DestroyStructure();
            }
        }

        protected virtual void DestroyStructure() => Destroy(gameObject);
    }
}
