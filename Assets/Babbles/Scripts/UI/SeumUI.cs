using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;

public class SeumUI : MonoBehaviour
{
    [SerializeField] private float _hideModeDelay = 5;
    [SerializeField] private float _animationSpeed = 1;
    [SerializeField] private RectTransform _targetUI;
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite[] _icons;
    [SerializeField] private UIEffects _effect;

    private E_ThermometerMode _thermometerMode;
    private Vector2 _defaultPos;

    private RectTransform rectTransform;

    private float _baseHeight;

    public E_ThermometerMode ThermometerMode { get => _thermometerMode; set => _thermometerMode = value; }

    private float _save;

    private float _timer;

    private float _state;
    private float _target;

    public enum E_ThermometerMode
    {
        AlwaysVisible,
        Animation,
        AlwaysHidden
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        _save = Seum.Instance.GetSeum();
        _defaultPos = rectTransform.anchoredPosition;
        _baseHeight = _targetUI.sizeDelta.y;
    }
    void Update()
    {
        _targetUI.sizeDelta = _targetUI.sizeDelta.SetY(Seum.Instance.GetSeum() * _baseHeight / Seum.Instance.SeumMax);
        int stage = Mathf.FloorToInt(Seum.Instance.GetSeum() / Seum.Instance.SeumMax * _icons.Length).Min(_icons.Length - 1);
        _icon.sprite = _icons[stage];

        switch (_thermometerMode)
        {
            case E_ThermometerMode.AlwaysHidden:
                rectTransform.anchoredPosition = _defaultPos - rectTransform.rect.size.SetY(0);
                if (_save != Seum.Instance.GetSeum())
                {
                    LaunchEffect();
                    _save = Seum.Instance.GetSeum();
                }
                break;
            case E_ThermometerMode.AlwaysVisible:
                rectTransform.anchoredPosition = _defaultPos;
                if (_save != Seum.Instance.GetSeum())
                {
                    LaunchEffect();
                    _save = Seum.Instance.GetSeum();
                }
                break;
            case E_ThermometerMode.Animation:
                rectTransform.anchoredPosition = _defaultPos
                                                            - (rectTransform.rect.size.SetY(0)
                                                                + Vector2.right * 2 + Vector2.right * _defaultPos.x)
                                                                * (1-_state);
                if(_save != Seum.Instance.GetSeum())
                {
                    LaunchEffect();
                    _save = Seum.Instance.GetSeum();
                    _timer = _hideModeDelay;
                    _target = 1;
                }
                if (_timer > 0)
                    _timer -= Time.deltaTime;
                else
                {
                    _timer = 0;
                    _target = 0;
                }
                var diff = _target - _state;
                _state += (diff.Sign() * _animationSpeed).Min(true, diff) * diff.Sign() * Time.smoothDeltaTime;
                break;
        }
    }

    private void LaunchEffect()
    {
        _effect.Activate();
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, () => { _effect.Desactivate(); });
    }
}
