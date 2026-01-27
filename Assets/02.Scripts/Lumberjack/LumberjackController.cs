using UnityEngine;

public class LumberjackController : MonoBehaviour
{
    private const float StopDistance = 0.1f;
    private const float TreeSearchInterval = 1f;
    private const string SFX_SWING = "lumberjack_swing";
    private const string SFX_HIT = "lumberjack_hit";

    [Header("Stats")]
    [SerializeField] private float _woodPerSecond = 1f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _attackRange = 2f;

    [Header("Animation")]
    [SerializeField] private bool _useAnimationEvents;

    private enum State { Idle, Moving, Attacking }
    private State _currentState = State.Idle;

    private float _attackTimer;
    private float _woodAccumulator;
    private float _treeSearchTimer = TreeSearchInterval;
    private Vector3 _targetPosition;
    private TreeController _treeController;
    private AudioManager _audioManager;

    private void Start()
    {
        ServiceLocator.TryGet(out _audioManager);
        FindTree();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Idle:
                FindTree();
                break;
            case State.Moving:
                MoveToTree();
                break;
            case State.Attacking:
                Attack();
                break;
        }
    }

    private void FindTree()
    {
        if (_treeController == null)
        {
            _treeSearchTimer += Time.deltaTime;
            if (_treeSearchTimer < TreeSearchInterval) return;
            _treeSearchTimer = 0f;

            GameObject treeObj = GameObject.FindWithTag("Tree");
            if (treeObj != null)
            {
                _treeController = treeObj.GetComponent<TreeController>();
            }
        }

        if (_treeController == null) return;

        _targetPosition = _treeController.transform.position + GetRandomOffset();
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
            LookAtTree();
            return;
        }

        transform.position += direction.normalized * _moveSpeed * Time.deltaTime;

        if (direction.magnitude > StopDistance)
        {
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }

    private void Attack()
    {
        if (_treeController == null)
        {
            _currentState = State.Idle;
            return;
        }

        if (_useAnimationEvents) return;

        _attackTimer += Time.deltaTime;
        if (_attackTimer < _attackInterval) return;

        _attackTimer = 0;
        OnSwingStart();
        OnSwingHit();
    }

    /// <summary>
    /// 도끼 휘두르기 시작 - Animation Event에서 호출 가능
    /// </summary>
    public void OnSwingStart()
    {
        _audioManager?.PlaySFX(SFX_SWING);
    }

    /// <summary>
    /// 도끼 타격 시점 - Animation Event에서 호출 가능
    /// </summary>
    public void OnSwingHit()
    {
        if (_treeController == null) return;

        _audioManager?.PlaySFX(SFX_HIT);
        _woodAccumulator += _woodPerSecond * _attackInterval;

        if (_woodAccumulator >= 1f)
        {
            long woodToAdd = (long)_woodAccumulator;
            _treeController.Hit(woodToAdd, transform.position);
            _woodAccumulator -= woodToAdd;
        }
    }

    private void LookAtTree()
    {
        if (_treeController == null) return;

        Vector3 lookDir = _treeController.transform.position - transform.position;
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

    public void SetStats(float woodPerSecond, float speed)
    {
        _woodPerSecond = woodPerSecond;
        _moveSpeed = speed;
    }
}
