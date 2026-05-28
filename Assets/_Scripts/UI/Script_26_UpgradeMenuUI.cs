using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

        private readonly Dictionary<int, List<SO_UpgradeData>> _currentOptions = new();
        private readonly Dictionary<int, int> _currentSelections = new();
        private float _timer;
        private bool _isVisible;

        public void ShowForPlayer(int playerIndex)
        {
            EnsurePlayerOptions(playerIndex);
            _timer = _selectionTimeout;
            _isVisible = true;
            if (_uiDocument != null)
                _uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            RenderCards(playerIndex);
            StartCoroutine(SelectionTimerRoutine(playerIndex));
        }

        private void Update()
        {
            if (!_isVisible)
                return;

            for (int playerIndex = 0; playerIndex < 4; playerIndex++)
                HandlePlayerInput(playerIndex);
        }

        private void HandlePlayerInput(int playerIndex)
        {
            bool left = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;
            bool right = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            bool confirm = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

            if (playerIndex < Gamepad.all.Count)
            {
                Gamepad pad = Gamepad.all[playerIndex];
                left |= pad.leftTrigger.wasPressedThisFrame;
                right |= pad.rightTrigger.wasPressedThisFrame;
                confirm |= pad.buttonSouth.wasPressedThisFrame;
            }

            if (!_currentSelections.ContainsKey(playerIndex))
                return;

            if (left)
                _currentSelections[playerIndex] = (_currentSelections[playerIndex] + 2) % 3;
            else if (right)
                _currentSelections[playerIndex] = (_currentSelections[playerIndex] + 1) % 3;

            if (left || right)
                RenderCards(playerIndex);

            if (confirm)
                ConfirmSelection(playerIndex);
        }

        private void EnsurePlayerOptions(int playerIndex)
        {
            if (_upgradePool.Count < 3)
                return;

            if (!_currentOptions.ContainsKey(playerIndex))
                _currentOptions[playerIndex] = new List<SO_UpgradeData>(3);
            _currentOptions[playerIndex].Clear();

            List<int> used = new();
            while (_currentOptions[playerIndex].Count < 3)
            {
                int idx = Random.Range(0, _upgradePool.Count);
                if (used.Contains(idx))
                    continue;
                used.Add(idx);
                _currentOptions[playerIndex].Add(_upgradePool[idx]);
            }

            _currentSelections[playerIndex] = 0;
        }

        private void RenderCards(int playerIndex)
        {
            VisualElement root = _uiDocument?.rootVisualElement;
            if (root == null || !_currentOptions.ContainsKey(playerIndex))
                return;

            List<SO_UpgradeData> options = _currentOptions[playerIndex];
            for (int i = 0; i < 3; i++)
            {
                VisualElement card = root.Q<VisualElement>($"UpgradeCard_{i + 1}");
                Label title = root.Q<Label>($"UpgradeCard_{i + 1}_Title");
                Label desc = root.Q<Label>($"UpgradeCard_{i + 1}_Desc");
                Label value = root.Q<Label>($"UpgradeCard_{i + 1}_Value");

                if (i < options.Count && title != null && desc != null && value != null && card != null)
                {
                    title.text = options[i].upgradeName;
                    desc.text = options[i].description;
                    value.text = $"+{options[i].numericEffect:0.##}";
                    card.EnableInClassList("upgrade-card--selected", i == _currentSelections[playerIndex]);
                }
            }
        }

        private void ConfirmSelection(int playerIndex)
        {
            if (!_currentOptions.ContainsKey(playerIndex))
                return;

            SO_UpgradeData selected = _currentOptions[playerIndex][_currentSelections[playerIndex]];
            Debug.Log($"Player {playerIndex + 1} seleccionó upgrade: {selected.upgradeName}");
            _isVisible = false;
            if (_uiDocument != null)
                _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }

        private IEnumerator SelectionTimerRoutine(int playerIndex)
        {
            while (_timer > 0f && _isVisible)
            {
                _timer -= Time.deltaTime;
                Label timerLabel = _uiDocument?.rootVisualElement.Q<Label>("UpgradeTimerLabel");
                if (timerLabel != null)
                    timerLabel.text = $"Tiempo: {Mathf.CeilToInt(_timer)}";

                yield return null;
            }

            if (_isVisible)
                ConfirmSelection(playerIndex);
        }
    }
}
