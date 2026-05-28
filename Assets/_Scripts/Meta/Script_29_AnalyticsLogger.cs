using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm.Meta
{
    /// <summary>
    /// Registro de datos de partida para balance y telemetría.
    /// </summary>
    public class Script_29_AnalyticsLogger : MonoBehaviour
    {
        private readonly List<string> _sessionLog = new();

        public void LogEvent(string eventName, string payload = "")
        {
            _sessionLog.Add($"{Time.time:F2}|{eventName}|{payload}");
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log($"[Analytics] {eventName}: {payload}");
#endif
        }

        public void FlushSession() => _sessionLog.Clear();
    }
}
