using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Combat
{
    public enum StatusEffectType
    {
        Poison,
        Freeze,
        Burn,
        Stun
    }

    /// <summary>
    /// Veneno, congelación, quemadura y stun.
    /// </summary>
    public class Script_22_StatusEffects : MonoBehaviour
    {
        private readonly Dictionary<StatusEffectType, float> _activeEffects = new();

        public void Apply(StatusEffectType type, float duration, float intensity)
        {
            _activeEffects[type] = Mathf.Max(_activeEffects.GetValueOrDefault(type), duration);
        }

        public bool HasEffect(StatusEffectType type) =>
            _activeEffects.TryGetValue(type, out float t) && t > 0f;

        private void Update()
        {
            var keys = new List<StatusEffectType>(_activeEffects.Keys);
            foreach (StatusEffectType key in keys)
            {
                _activeEffects[key] -= Time.deltaTime;
                if (_activeEffects[key] <= 0f)
                    _activeEffects.Remove(key);
            }
        }
    }
}
