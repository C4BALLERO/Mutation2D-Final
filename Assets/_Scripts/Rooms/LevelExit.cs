using System.Collections;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Rooms
{
    /// <summary>
    /// Portal that loads the next scene when a player enters its trigger zone.
    /// Can be locked until all enemies are defeated.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        [SerializeField] private string  _nextScene;
        [SerializeField] private bool    _requireWaveClear = true;
        [SerializeField] private Color   _lockedColor   = new(0.5f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color   _openColor     = new(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private float   _transitionDelay = 0.8f;

        private SpriteRenderer _sr;
        private bool _open;
        private bool _loading;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            GetComponent<Collider2D>().isTrigger = true;

            // Open immediately if we don't require wave clear
            if (!_requireWaveClear)
                Open();
            else
                UpdateColor();
        }

        private void OnEnable()
        {
            Script_03_EventBus.Subscribe<WaveEndedEvent>(OnWaveEnded);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<WaveEndedEvent>(OnWaveEnded);
        }

        private void OnWaveEnded(WaveEndedEvent ev) => Open();

        private void Open()
        {
            _open = true;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_sr == null) return;
            _sr.color = _open ? _openColor : _lockedColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_open || _loading) return;
            if (!other.TryGetComponent<Entities.Script_11_PlayerController>(out _)) return;

            StartCoroutine(LoadNext());
        }

        private IEnumerator LoadNext()
        {
            _loading = true;
            yield return new WaitForSeconds(_transitionDelay);

            if (!string.IsNullOrEmpty(_nextScene))
                SceneManager.LoadScene(_nextScene);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _open ? _openColor : _lockedColor;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
