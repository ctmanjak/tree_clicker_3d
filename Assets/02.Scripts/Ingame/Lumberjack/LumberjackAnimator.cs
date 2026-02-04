using System;
using UnityEngine;

namespace Ingame
{
    public class LumberjackAnimator : MonoBehaviour
    {
        private static readonly int BlendParam = Animator.StringToHash("Blend");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        [SerializeField] private Animator _animator;

        public event Action OnAttackHit;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        public void PlayAttack()
        {
            if (_animator == null) return;
            _animator.SetTrigger(AttackTrigger);
        }

        public void SetAnimationSpeed(float speed)
        {
            if (_animator == null) return;
            _animator.SetFloat(SpeedParam, speed);
        }

        public void SetBlendValue(float value)
        {
            if (_animator == null) return;
            _animator.SetFloat(BlendParam, Mathf.Clamp01(value));
        }

        // Animation Event에서 호출
        public void OnAttackHitEvent()
        {
            OnAttackHit?.Invoke();
        }
    }
}
