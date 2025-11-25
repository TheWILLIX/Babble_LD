using ClemCAddons;
using Luminosity.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private bool _moveOnly;
    [SerializeField] private Selectable _optionsButton;
    [SerializeField] private float _speed = 1;
    [SerializeField] private Image _background;
    [SerializeField] private float _backgroundSpeed = 1;
    [SerializeField] private float _backgroundDefault = 0.67f;
    [SerializeField] private bool _gameMode;
    private RectTransform rectTransform;
    private float _basePos;
    private float _target;

    private float _colorTarget;
    private static bool _lock;
    public static bool Lock { get => _lock; set => _lock = value; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        _basePos = rectTransform.anchoredPosition.x;
        _target = _basePos;

        if (_gameMode)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }
    }

    void Update()
    {
        if(_target == 0 && InputManager.GetButtonDown("UI_Cancel") && !_lock)
        {
            if(!_moveOnly)
                _optionsButton.Select();
            Hide();
        }
        var diffColor = _colorTarget - _background.color.a;
        if(diffColor != 0)
        {
            _background.color += Color.clear.SetA(Mathf.Min(_backgroundSpeed * Time.deltaTime, diffColor.Abs()) * diffColor.Sign());
        }
        var difference = _target - rectTransform.anchoredPosition.x;
        if (difference == 0)
        {
            if(_target != 0 && _gameMode)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                foreach(var r in renderers)
                {
                    r.enabled = false;
                }
            }
            return;
        }
        rectTransform.anchoredPosition += Vector2.zero.SetX(Mathf.Min(_speed * Time.deltaTime, difference.Abs()) * difference.Sign());
    }

    public void Show()
    {
        _target = 0;
        _colorTarget = _backgroundDefault;
        if(_gameMode)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }
    }

    public void Hide()
    {
        _target = _basePos;
        _colorTarget = 0;
    }

    public void SetFOV(float fov)
    {
        foreach(var cam in Camera.allCameras)
        {
            cam.fieldOfView = fov;
        }
    }

    public void Unlock()
    {
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () => _lock = false);
    }

}
