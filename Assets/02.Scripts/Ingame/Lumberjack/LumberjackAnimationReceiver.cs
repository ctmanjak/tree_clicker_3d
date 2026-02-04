using UnityEngine;

namespace Ingame
{
    public class LumberjackAnimationReceiver : MonoBehaviour
    {
        private LumberjackAnimator _animator;
        private LumberjackController _controller;

        private void Awake()
        {
            _animator = GetComponentInParent<LumberjackAnimator>();
            _controller = GetComponentInParent<LumberjackController>();
        }

        // Animation Event: 공격 시작
        public void OnSwingStartEvent()
        {
            _controller?.OnSwingStart();
        }

        // Animation Event: 타격 시점
        public void OnAttackHitEvent()
        {
            _animator?.OnAttackHitEvent();
        }

        // Animation Event: 공격 애니메이션 종료
        public void OnAttackEndEvent()
        {
            _controller?.OnAttackAnimationEnd();
        }
    }
}
