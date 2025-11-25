using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Utilities;

public class ScaredyCat : MonoBehaviour
{
    [SerializeField] private float _distanceDown = 2;
    [SerializeField] private float _scareDuration = 5;
    [SerializeField] private float _getDownDuration = 0.5f;
    [SerializeField] private float _getUpDuration = 3f;
    private Vector3 _basePos;
    private bool _scared;
    private float _delay;

    void Start()
    {
        _basePos = transform.position;
    }

    void Update()
    {
        if (!_scared)
        {
            ReturnToBasePos();
            return;
        }
        _delay -= Time.deltaTime;
        if(_delay <= 0)
        {
            _scared = false;
        }
    }


    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }
    void OnTriggerEnter(Collider other)
    {
        _delay = _scareDuration;
        if (_scared)
            return;
        _scared = true;
        Lerper.ConstantLerp(transform.position, _basePos + Vector3.down * _distanceDown, _getDownDuration, (v) => { transform.position = v; _delay = _scareDuration; });
    }


    private void ReturnToBasePos()
    {
        var dist = _basePos.y - transform.position.y;
        transform.position += Vector3.up * Mathf.Min(dist * Time.deltaTime, _distanceDown / _getUpDuration * Time.deltaTime);
    }
}
