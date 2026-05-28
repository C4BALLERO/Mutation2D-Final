using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Menú roguelite entre oleadas: 3 cartas, timer 15s, coop independiente.
    /// </summary>
    public class Script_26_UpgradeMenuUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private List<SO_UpgradeData> _upgradePool = new();
        [SerializeField] private float _selectionTimeout = 15f;

        public void ShowForPlayer(int playerIndex) { }
    }
}
