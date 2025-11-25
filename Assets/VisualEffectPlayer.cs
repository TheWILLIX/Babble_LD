using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualEffectPlayer : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField, Range(0, 1)] private float _value;
    [SerializeField] private string _valueName;
    [SerializeField] private bool _active;

    private bool _previous;

    private bool _direction;


    public bool Active { get => _active; set => _active = value; }


    void Update()
    {
        if (_active != _previous)
        {
            _previous = _active;
            if (_active)
                Setup();
            else
                Hide();
        }
        if (_active)
        {
            _material.SetFloat(_valueName, _value);
        }
    }

    private void Setup()
    {
        var allChildren = transform.GetChildrenWithComponentDeep<Transform>();
        for (int i = 0; i < allChildren.Length; i++)
        {
            // if not a canvas
            if (!allChildren[i].TryGetComponent<Canvas>(out _))
            {
                allChildren[i].gameObject.SetActive(true);
            }
        }
    }

    private void Hide()
    {
        var allChildren = transform.GetChildrenWithComponentDeep<Transform>();
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (!allChildren[i].TryGetComponent<Canvas>(out _))
                allChildren[i].gameObject.SetActive(false);
        }
    }

    public void StartTransi(Action callback)
    {
        _active = true;
        ClemCAddons.Utilities.Lerper.ConstantLerp(ref _value, 0.1f, 2, () =>
        {
            ClemCAddons.Utilities.Lerper.ConstantLerp(ref _value, 0.3f, 0.5f, () =>
            {
                ClemCAddons.Utilities.Lerper.ConstantLerp(ref _value, 0, 1, () => { _value = 0; callback.Invoke(); });
            });
        });
    }

    public void StartReverseTransi(Action callback)
    {
        _active = true;
        ClemCAddons.Utilities.Lerper.ConstantLerp(ref _value, 1, 2, () =>
        {
            _value = 1;
            callback.Invoke();
        });
    }
}
