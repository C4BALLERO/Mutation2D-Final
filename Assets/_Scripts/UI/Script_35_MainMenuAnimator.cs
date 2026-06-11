using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Adds ambient animations to the Main Menu:
    /// - Title color pulse (white ↔ cyan)
    /// - Status dot blink
    /// - Record wave loaded from SaveManager
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class Script_35_MainMenuAnimator : MonoBehaviour
    {
        private Label         _titleLabel;
        private Label         _recordWaveLabel;
        private Label         _cornerStatusLabel;
        private VisualElement _statusDot;

        private float _blinkTimer;
        private bool  _dotVisible = true;

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _titleLabel        = root.Q<Label>("MenuTitle");
            _recordWaveLabel   = root.Q<Label>("RecordWave");
            _cornerStatusLabel = root.Q<Label>("CornerStatus");
            _statusDot         = root.Q<VisualElement>(className: "status-dot--green");

            LoadRecordFromSave();
        }

        private void Update()
        {
            AnimateTitle();
            AnimateStatusDot();
        }

        private void AnimateTitle()
        {
            if (_titleLabel == null) return;

            // Slow pulse: white ↔ soft cyan
            float t = (Mathf.Sin(Time.time * 0.8f) + 1f) * 0.5f;
            float r = Mathf.Lerp(228f, 200f, t);
            float g = Mathf.Lerp(235f, 240f, t);
            float b = Mathf.Lerp(245f, 255f, t);
            _titleLabel.style.color = new Color(r / 255f, g / 255f, b / 255f);
        }

        private void AnimateStatusDot()
        {
            _blinkTimer += Time.deltaTime;
            if (_blinkTimer < 1.2f) return;

            _blinkTimer = 0f;
            _dotVisible = !_dotVisible;

            if (_statusDot != null)
                _statusDot.style.opacity = _dotVisible ? 1f : 0.2f;
        }

        private void LoadRecordFromSave()
        {
            if (_recordWaveLabel == null) return;

            int best = 0;
            var sm = Script_05_SaveManager.Instance;
            if (sm?.CurrentSave != null)
                best = sm.CurrentSave.maxWaveReached;

            _recordWaveLabel.text = best > 0 ? best.ToString() : "—";
        }
    }
}
