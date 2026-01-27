using UnityEngine;

public class LumberjackController : MonoBehaviour
{
    private const float StopDistance = 0.1f;
    private const float TreeSearchInterval = 1f;
    private const string SfxSwing = "lumberjack_swing";
    private const string SfxHit = "lumberjack_hit";

    [Header("Stats")]
    [SerializeField] private float _woodPerSecond = 1f;
    [SerializeField] private float _attackCooldown = 0.5f;
    [SerializeField] private float _attackRange = 2f;

    [Header("Animation")]
    [SerializeField] private float _blendDampTime = 0.1f;
    [SerializeField] private float _rotationSpeed = 10f;

    private enum State { Idle, Moving, Attacking }
    private State _currentState = State.Idle;

    public bool IsMoving => _currentState == State.Moving;

    private bool _isAttackAnimPlaying;
    private float _attackCooldownTimer;
    private float _woodAccumulator;
    private float _treeSearchTimer = TreeSearchInterval;
    private float _currentBlendValue;
    private Vector3 _targetPosition;
    private TreeController _treeController;
    private AudioManager _audioManager;
    private LumberjackAnimator _animator;

    private void Awake()
    {
        _animator = GetComponent<LumberjackAnimator>();
    }

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);

        if (_animator != null)
        {
            _animator.OnAttackHit += OnSwingHit;
        }

        FindTree();
    }

    private void OnDestroy()
    {
        if (_animator != null)
        {
            _animator.OnAttackHit -= OnSwingHit;
        }
    }

    private void Update()
    {
        float targetBlend = 0f;

        switch (_currentState)
        {
            case State.Idle:
                FindTree();
                break;
            case State.Moving:
                targetBlend = 1f;
                MoveToTree();
                break;
            case State.Attacking:
                Attack();
                break;
        }

        _currentBlendValue = Mathf.MoveTowards(_currentBlendValue, targetBlend, Time.deltaTime / _blendDampTime);
        _animator?.SetBlendValue(_currentBlendValue);
    }

    private void FindTree()
    {
        if (_treeController == null)
        {
            _treeSearchTimer += Time.deltaTime;
            if (_treeSearchTimer < TreeSearchInterval) return;
            _treeSearchTimer = 0f;

            ServiceLocator.TryGet(out _treeController);
        }

        if (_treeController == null) return;

        _targetPosition = _treeController.Position + GetRandomOffset();
        _currentState = State.Moving;
    }

    private void MoveToTree()
    {
        if (_treeController == null)
        {
            _currentState = State.Idle;
            return;
        }

        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;

        if (direction.magnitude < StopDistance)
        {
            _currentState = State.Attacking;
            _attackCooldownTimer = 0f;
            LookAtTree();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void Attack()
    {
        if (_treeController == null)
        {
            _currentState = State.Idle;
            return;
        }

        if (_isAttackAnimPlaying) return;

        _attackCooldownTimer += Time.deltaTime;
        if (_attackCooldownTimer < _attackCooldown) return;

        _attackCooldownTimer = 0f;
        _isAttackAnimPlaying = true;
        _animator?.PlayAttack();
    }

    public void OnSwingStart()
    {
        _audioManager?.PlaySFX(SfxSwing);
    }

    public void OnSwingHit()
    {
        if (_treeController == null) return;

        _audioManager?.PlaySFX(SfxHit);
        _woodAccumulator += _woodPerSecond;

        if (_woodAccumulator >= 1f)
        {
            long woodToAdd = (long)_woodAccumulator;
            _treeController.Hit(woodToAdd, transform.position);
            _woodAccumulator -= woodToAdd;
        }
    }

    // Animation Event 또는 LumberjackAnimationReceiver에서 호출
    public void OnAttackAnimationEnd()
    {
        _isAttackAnimPlaying = false;
    }

    private void LookAtTree()
    {
        if (_treeController == null) return;

        Vector3 lookDir = _treeController.Position - transform.position;
        lookDir.y = 0;

        if (lookDir.magnitude > StopDistance)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private Vector3 GetRandomOffset()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(_attackRange * 0.8f, _attackRange);
        return new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);
    }

    public void SetStats(float woodPerSecond, float animationSpeed)
    {
        _woodPerSecond = woodPerSecond;
        _animator?.SetAnimationSpeed(animationSpeed);
    }
}
