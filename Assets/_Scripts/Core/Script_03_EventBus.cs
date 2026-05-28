using System;
using System.Collections.Generic;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Bus de eventos global tipado. Evita acoplamiento directo entre sistemas.
    /// </summary>
    public static class Script_03_EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out Delegate existing))
                _handlers[key] = Delegate.Combine(existing, handler);
            else
                _handlers[key] = handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out Delegate existing))
                return;

            Delegate result = Delegate.Remove(existing, handler);
            if (result == null)
                _handlers.Remove(key);
            else
                _handlers[key] = result;
        }

        public static void Publish<T>(T payload)
        {
            if (_handlers.TryGetValue(typeof(T), out Delegate del))
                (del as Action<T>)?.Invoke(payload);
        }

        public static void Clear() => _handlers.Clear();
    }

    // Payloads de eventos comunes
    public struct WaveStartedEvent { public int waveNumber; }
    public struct WaveEndedEvent { public int waveNumber; public WaveSummary summary; }
    public struct EnemySpawnedEvent { public GameObject enemy; }
    public struct EvolutionPhaseEvent { public EvolutionSummary summary; }
    public struct UpgradePhaseEvent { }
    public struct PlayerHitEvent { public int playerIndex; public float damage; }
    public struct BossSpawnEvent { public BossData bossData; }
}
