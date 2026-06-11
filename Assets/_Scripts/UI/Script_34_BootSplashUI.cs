using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Animates the boot splash screen:
    /// - Fills the loading bar over _totalDuration seconds
    /// - Cycles through mock initialization messages
    /// - Invokes OnSplashComplete when done
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class Script_34_BootSplashUI : MonoBehaviour
    {
        [SerializeField] private float _totalDuration = 2.8f;

        public event Action OnSplashComplete;

        private VisualElement _barFill;
        private Label         _statusLabel;
        private Label         _detailLabel;
        private Label         _percentLabel;
        private Label         _titleLabel;

        private float _elapsed;
        private bool  _done;
        private int   _lastMessageIndex = -1;

        private static readonly string[] Messages =
        {
            "INICIALIZANDO SISTEMAS...",
            "genome_engine.dll ... OK",
            "wave_manager.dll ... OK",
            "evolution_protocol.dll ... OK",
            "ai_swarm_v3.dll ... OK",
            "CARGANDO DATOS DE MUTACIÓN...",
            "audio_system.dll ... OK",
            "save_manager.dll ... OK",
            "PREPARANDO ENJAMBRE...",
        };

        private static readonly string[] Details =
        {
            "cargando módulos principales...",
            "genome_engine.dll ... OK",
            "wave_manager.dll ... OK",
            "evolution_protocol.dll ... OK",
            "ai_swarm_v3.dll ... OK",
            "leyendo 1,247 registros genéticos",
            "audio_system.dll ... OK",
            "save_manager.dll ... OK",
            "SISTEMA LISTO",
        };

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _barFill      = root.Q<VisualElement>("LoadingBarFill");
            _statusLabel  = root.Q<Label>("LoadingStatus");
            _detailLabel  = root.Q<Label>("LoadingDetail");
            _percentLabel = root.Q<Label>("LoadingPercent");
            _titleLabel   = root.Q<Label>("SplashTitle");

            _elapsed = 0f;
            _done    = false;
        }

        private void Update()
        {
            if (_done) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _totalDuration);

            // Fill the bar
            if (_barFill != null)
                _barFill.style.width = new StyleLength(new Length(t * 100f, LengthUnit.Percent));

            // Update percent label
            int pct = Mathf.RoundToInt(t * 100f);
            if (_percentLabel != null)
                _percentLabel.text = $"{pct}%";

            // Cycle messages by progress segment
            int msgIndex = Mathf.FloorToInt(t * (Messages.Length - 1));
            if (msgIndex != _lastMessageIndex)
            {
                _lastMessageIndex = msgIndex;
                if (_statusLabel != null && msgIndex < Messages.Length)
                    _statusLabel.text = Messages[msgIndex];
                if (_detailLabel != null && msgIndex < Details.Length)
                    _detailLabel.text = Details[msgIndex];
            }

            // Pulse title color: white ↔ cyan (StyleColor requires Color, not Color32)
            if (_titleLabel != null)
            {
                float pulse = Mathf.Sin(Time.time * 1.2f) * 0.5f + 0.5f;
                float g = Mathf.Lerp(232f / 255f, 1f, pulse);
                float b = Mathf.Lerp(238f / 255f, 1f, pulse);
                _titleLabel.style.color = new Color(228f / 255f, g, b);
            }

            if (t >= 1f && !_done)
            {
                _done = true;
                OnSplashComplete?.Invoke();
            }
        }
    }
}
