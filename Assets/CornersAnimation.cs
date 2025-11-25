using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornersAnimation : MonoBehaviour
{
    [SerializeField] private float _moveRange = 5;
    [SerializeField] private float _speed = 2;
    private RectTransform _rectTransform;
    private float _value = 0;
    private bool _direction = true;
    private Vector2 _baseSize;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _baseSize = _rectTransform.sizeDelta;
    }

    void OnEnable()
    {
        _value = 0;
        _direction = true;
    }

    void Update()
    {
        if (_direction)
            _value += Time.deltaTime * _speed;
        else
            _value -= Time.deltaTime * _speed;
        if (_value > 1)
        {
            _value = 1;
            _direction = false;
        }
        if(_value < 0)
        {
            _value = 0;
            _direction = true;
        }
        _rectTransform.sizeDelta = _baseSize + _moveRange * Vector2.one * _value;
    }
}
