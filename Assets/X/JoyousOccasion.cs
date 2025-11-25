using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class JoyousOccasion : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _distance = 5;
    [SerializeField] private float _amount = 5;
    private Vector3 _basePos;

    void Start()
    {
        _basePos = transform.position;
    }
    void Update()
    {
        if(_target.Distance(transform) <= _distance)
        {
            transform.position = Vector3.Lerp(transform.position, _basePos + Vector3.up * _amount, Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _basePos, Time.deltaTime);
        }
    }
}
