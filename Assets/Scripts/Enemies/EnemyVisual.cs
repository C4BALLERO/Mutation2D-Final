using UnityEngine;

namespace MutationSwarm
{
    // Library of per-creature animation frames (baked at edit time by SceneSetup).
    // EnemyBase picks a creature index by gene; this component drives the FrameAnimator
    // based on the enemy's current state (walk / attack / death).
    [RequireComponent(typeof(FrameAnimator))]
    public class EnemyVisual : MonoBehaviour
    {
        [System.Serializable]
        public class CreatureFrames
        {
            public Sprite[] walk;
            public Sprite[] attack;
            public Sprite[] death;
        }

        // Index order: 0 = Dino, 1 = Mono, 2 = enemy3, 3 = Diablito
        public CreatureFrames[] creatures = new CreatureFrames[4];

        FrameAnimator _anim;
        EnemyBase _eb;

        void Awake()
        {
            _anim = GetComponent<FrameAnimator>();
            _eb = GetComponentInParent<EnemyBase>();
        }

        public CreatureFrames Current =>
            (_eb != null && creatures != null && _eb.CreatureIndex >= 0 && _eb.CreatureIndex < creatures.Length)
                ? creatures[_eb.CreatureIndex] : null;

        void LateUpdate()
        {
            if (_anim == null || _eb == null) return;
            var c = Current;
            if (c == null) return;

            if (_eb.IsDying)
            {
                if (c.death != null && c.death.Length > 0) _anim.Play(c.death, false);
            }
            else if (_eb.IsAttacking)
            {
                if (c.attack != null && c.attack.Length > 0) _anim.Play(c.attack, true);
            }
            else
            {
                if (c.walk != null && c.walk.Length > 0) _anim.Play(c.walk, true);
            }
        }

        public bool DeathFinished => _anim != null && _anim.IsDone;
    }
}
