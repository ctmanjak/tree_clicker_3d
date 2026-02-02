using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LumberjackRootMotion : MonoBehaviour
{
    private const float Gravity = -9.81f;

    private Animator _animator;
    private LumberjackController _controller;
    private CharacterController _characterController;
    private float _verticalVelocity;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponentInParent<LumberjackController>();
        _characterController = GetComponentInParent<CharacterController>();
    }

    private void OnAnimatorMove()
    {
        if (_controller == null) return;

        Vector3 movement = Vector3.zero;

        if (_controller.IsMoving)
        {
            movement = _animator.deltaPosition;
        }

        if (_characterController != null)
        {
            if (_characterController.isGrounded)
            {
                _verticalVelocity = -0.5f;
            }
            else
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            movement.y = _verticalVelocity * Time.deltaTime;
            _characterController.Move(movement);
        }
        else
        {
            _controller.transform.position += movement;
        }
    }
}
