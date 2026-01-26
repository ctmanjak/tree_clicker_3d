using System;
using System.Collections;
using UnityEngine;

public class Firewood : MonoBehaviour
{
    [SerializeField] private float _lifetime = 1.5f;

    private Rigidbody _rigidbody;
    private Action<Firewood> _onComplete;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
    }

    public void Initialize(Action<Firewood> returnToPool)
    {
        _onComplete = returnToPool;
    }

    public void Launch(Vector3 position, Vector3 force)
    {
        transform.position = position;
        transform.rotation = UnityEngine.Random.rotation;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);

        StopAllCoroutines();
        StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(_lifetime);
        _onComplete?.Invoke(this);
    }
}
