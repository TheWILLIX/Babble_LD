using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClemCAddons;

public class Virvolta : MonoBehaviour
{
    [SerializeField] private Rigidbody _target;
    [SerializeField] private BezierSpline _targetSpline;
    [SerializeField] private float _speed = 0.05f;
    [SerializeField] private float _gravity = 1;
    [SerializeField] private float _bounceHeight = 2;

    private bool _activated;
    private float _progress;
    private float _y;
    private float _bounce;

    public bool Activated { get => _activated;}

    void Start()
    {
        _y = 0;
    }

    void Awake()
    {
        _y = 0;

    }

    void OnTriggerEnter(Collider other)
    {
        Activate();
    }

    void FixedUpdate()
    {
        if (_target == null)
            return;
        else if (!_activated)
        {
            _target.gameObject.SetActive(false);
            return;
        }
        _target.gameObject.SetActive(true);
        var pos = _targetSpline.GetPoint(_progress) + Vector3.zero.SetY(_y);
        if(_target.SweepTest(_target.position.Direction(pos),out _, _target.position.Distance(pos),QueryTriggerInteraction.Ignore))
        {
            _bounce = _bounceHeight;
        }
        else
        {
            _target.position = pos;
        }
        _target.rotation = _targetSpline.GetDirection(_progress).DirectionToQuaternion();

        _y += _bounce * Time.fixedDeltaTime;

        _bounce -= _gravity * Time.fixedDeltaTime;

        _progress += Time.fixedDeltaTime * _speed;
        if (_progress > 1)
            _activated = false;
    }

    public void Activate()
    {
        _activated = true;
        _progress = 0;
        _y = 0;
        _bounce = 0;
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(Virvolta))]
public class VirvoltaEditor : Editor
{
	private Virvolta virvolta;
    private int selected;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        virvolta = target as Virvolta;
        GUI.enabled = !virvolta.Activated;
        if (GUILayout.Button("Trigger"))
        {
            virvolta.Activate();
        }
        GUI.enabled = true;
    }
}
#endif