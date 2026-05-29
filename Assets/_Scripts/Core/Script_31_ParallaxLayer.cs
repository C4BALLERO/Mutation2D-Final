using UnityEngine;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Desplaza una capa de fondo a distinta velocidad para efecto parallax.
    /// </summary>
    public class Script_31_ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float _parallaxFactor = 0.3f;
        [SerializeField] private bool _infiniteHorizontal = true;
        [SerializeField] private float _spriteWidth = 20f;

        private Transform _cameraTransform;
        private Vector3 _startPosition;
        private float _cameraStartX;

        private void Start()
        {
            _startPosition = transform.position;
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _cameraStartX = _cameraTransform.position.x;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
                return;

            float deltaX = _cameraTransform.position.x - _cameraStartX;
            transform.position = _startPosition + Vector3.right * (deltaX * _parallaxFactor);

            if (!_infiniteHorizontal || _spriteWidth <= 0f)
                return;

            float offset = transform.position.x - _startPosition.x;
            if (offset > _spriteWidth)
                _startPosition.x += _spriteWidth;
            else if (offset < -_spriteWidth)
                _startPosition.x -= _spriteWidth;
        }
    }
}
