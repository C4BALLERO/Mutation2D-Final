using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// HUD en tiempo real: HP, oleada, enemigos, dash, recursos, evolución.
    /// </summary>
    public class Script_25_HUDController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private void OnEnable()
        {
            Script_03_EventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
            Script_03_EventBus.Subscribe<WaveEndedEvent>(OnWaveEnded);
            Script_03_EventBus.Subscribe<EvolutionPhaseEvent>(OnEvolutionPhase);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            Script_03_EventBus.Unsubscribe<WaveEndedEvent>(OnWaveEnded);
            Script_03_EventBus.Unsubscribe<EvolutionPhaseEvent>(OnEvolutionPhase);
        }

        private void OnWaveStarted(WaveStartedEvent e) { }
        private void OnWaveEnded(WaveEndedEvent e) { }
        private void OnEvolutionPhase(EvolutionPhaseEvent e) { }
    }
}
