using UnityEngine;

public class LumberjackController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _woodPerSecond = 1f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _attackInterval = 1f;
    [SerializeField] private float _attackRange = 2f;

    private enum State { Idle, Moving, Attacking }
    private State _currentState = State.Idle;

    private float _attackTimer;
    private float _woodAccumulator;
    private Vector3 _targetPosition;
    private TreeController _treeController;

    private void Start()
    {
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

        if (direction.magnitude < 0.1f)
        {
            _currentState = State.Attacking;
            LookAtTree();
            return;
        }

        transform.position += direction.normalized * _moveSpeed * Time.deltaTime;

        if (direction.magnitude > 0.1f)
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

        _attackTimer += Time.deltaTime;

        if (_attackTimer < _attackInterval) return;

        _attackTimer = 0;

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

        if (lookDir.magnitude > 0.1f)
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
