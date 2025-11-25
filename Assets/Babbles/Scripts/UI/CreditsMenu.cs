using ClemCAddons;
using Luminosity.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private bool _moveOnly;
    [SerializeField] private Selectable _creditsButton;
    [SerializeField] private float _speed = 1;
    [SerializeField] private Image _background;
    [SerializeField] private float _backgroundSpeed = 1;
    [SerializeField] private float _backgroundDefault = 0.67f;
    private RectTransform rectTransform;
    private float _basePos;
    private float _target;

    private float _colorTarget = 0;
    private static bool _lock;
    public static bool Lock { get => _lock; set => _lock = value; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        _basePos = rectTransform.anchoredPosition.y;
        _target = _basePos;
    }

    void Update()
    {
        if (_target == 0 && InputManager.GetButtonDown("UI_Cancel") && !_lock)
        {
            if (!_moveOnly)
                _creditsButton.Select();
            Hide();
        }
        var diffColor = _colorTarget - _background.color.a;
        if (diffColor != 0)
        {
            _background.color += Color.clear.SetA(Mathf.Min(_backgroundSpeed * Time.deltaTime, diffColor.Abs()) * diffColor.Sign());
        }
        var difference = _target - rectTransform.anchoredPosition.y;
        if (difference == 0)
            return;
        rectTransform.anchoredPosition += Vector2.zero.SetY(Mathf.Min(_speed * Time.deltaTime, difference.Abs()) * difference.Sign());
    }

    public void Show()
    {
        _target = 0;
       // _colorTarget = 180f;
        _colorTarget = _backgroundDefault;
    }

    public void Hide()
    {
        _target = _basePos;
        _colorTarget = 0;
    }

    public void SetFOV(float fov)
    {
        foreach (var cam in Camera.allCameras)
        {
            cam.fieldOfView = fov;
        }
    }

    public void Unlock()
    {
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () => _lock = false);
    }

}
