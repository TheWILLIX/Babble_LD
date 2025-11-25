using ClemCAddons;
using Luminosity.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomSlider : Selectable, IMoveHandler, ISubmitHandler, ICancelHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private bool _runtimeUpdate;
    [SerializeField] private Sprite _validSprite;
    [SerializeField] private Sprite _invalidSprite;
    [SerializeField] private Sprite _plusButton;
    [SerializeField] private Sprite _minusButton;
    [SerializeField] private TMPro.TMP_FontAsset _font;
    [SerializeField] private Color _fontColor = Color.white;
    [SerializeField] private float _textOffset;

    [SerializeField] private int _spriteCount = 7;
    [SerializeField] private bool _inverseDirection;

    [SerializeField, LabelOverride("Value", rangeMin: "_minValue", rangeMax: "_maxValue", fallbackIntProperty: "_intValue", forceInt: "_wholeNumbers")] private float _value;
    [SerializeField, HideInInspector] private int _intValue;
    [SerializeField, DrawIf("_wholeNumbers", false, ComparisonType.Equals)] private float _minValue;
    [SerializeField, DrawIf("_wholeNumbers", false, ComparisonType.Equals)] private float _maxValue;
    [SerializeField, DrawIf("_wholeNumbers", true, ComparisonType.Equals)] private float m_minValue;
    [SerializeField, DrawIf("_wholeNumbers", true, ComparisonType.Equals)] private float m_maxValue;
    [SerializeField] private bool _wholeNumbers;
    [SerializeField] private float _minStep = 1;
    [SerializeField] private float _maxStep = 10;
    private float _step = 1;
    [Space]
    [SerializeField] private SliderEvent _onValueChanged = new SliderEvent();


    private bool _down;
    [Serializable]
    public class SliderEvent : UnityEvent<float> { }

    private float _previousValue;

    private float _starterValue;

    public float value
    {
        get => _wholeNumbers ? _intValue : _value; set
        {
            if (_wholeNumbers)
            {
                _intValue = (int)value;
            }
            else
            {
                _value = value;
            }
        }
    }
    public float minValue { get => _wholeNumbers ? m_minValue : _minValue; set
        {
            if (_wholeNumbers) {
                m_minValue = value;
            }
            else {
                _minValue = value;
            } } }
    public float maxValue
    {
        get => _wholeNumbers ? m_maxValue : _maxValue; set
        {
            if (_wholeNumbers)
            {
                m_maxValue = value;
            }
            else
            {
                _maxValue = value;
            }
        }
    }
    public bool wholeNumbers { get => _wholeNumbers; set => _wholeNumbers = value; }

    public override void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
        {
            if (_down)
                _step = (_step + 1).Clamp(_minStep, _maxStep);
            else
                _step = _minStep;
            value += eventData.moveVector.x * _step * (_inverseDirection ? -1 : 1);
            _onValueChanged.Invoke(value);
        } else
        {
            base.OnMove(eventData);
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        value = _starterValue;
        _onValueChanged.Invoke(_starterValue);
        transform.FindParentWithComponent(typeof(MenuOption)).GetComponentInChildren<Button>().Select();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        transform.FindParentWithComponent(typeof(MenuOption)).GetComponentInChildren<Button>().Select();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        OptionsMenu.Lock = true;
        _starterValue = value;
        base.OnSelect(eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
        OptionsMenu.Lock = false);
        base.OnDeselect(eventData);
    }

    void Update()
    {
        if (!Application.isPlaying && !_runtimeUpdate)
            return;
        if(Application.isPlaying)
            _down = InputManager.GetAxis("UI_Left") > 0.2f || InputManager.GetAxis("UI_Right") > 0.2f;
        if (!_down)
            _step = _minStep;

        if (_intValue != _value.Round())
        {
            if (_wholeNumbers)
                _value = _intValue;
            else
                _intValue = _value.Round();
        }
        if(_minValue.Round() != m_minValue || _maxValue.Round() != m_maxValue)
        {
            if (_wholeNumbers)
            {
                _minValue = m_minValue;
                _maxValue = m_maxValue;
            }
            else
            {
                m_minValue = _minValue.Round();
                m_maxValue = _maxValue.Round();
            }
        }
        if(_previousValue != _value)
        {
            _step = _step.Clamp(_minStep, _maxStep);
            Redraw();
            _previousValue = _value;
        }
    }

    public void Redraw()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        var perc = (_value-_minValue) / (_maxValue - _minValue);
        perc = _inverseDirection ? 1 - perc : perc;
        var halfStep = (_spriteCount - 1) * 0.5f;
        var targetStep = ((_spriteCount) * perc - 1).Max(-1+0.00001f);
        var reducedStep = Mathf.CeilToInt(targetStep);
        var stepSize = GetComponent<RectTransform>().rect.size.x / _spriteCount;
        var stepHeight = GetComponent<RectTransform>().rect.size.y / _spriteCount;
        var parentS = new GameObject("Sprites", typeof(RectTransform)).transform;
        parentS.localPosition = Vector3.zero;
        parentS.SetParent(transform, false);
        float totalSize = 0;
        for(int i = 0; i < _spriteCount; i++)
        {
            var tH = ((_inverseDirection ? _spriteCount - 1 - i : i) + halfStep) / 2f;
            totalSize += stepHeight * tH;
        }
        float dist = GetComponent<RectTransform>().rect.size.x - stepSize - totalSize;
        var offsetSize = dist / _spriteCount;
        var sizeSoFar = 0f;
        for (int i = _spriteCount - 1; i >= 0; i--)
        {
            var tH = ((_inverseDirection ? _spriteCount - i : i) + halfStep) / 2f;
            var pos = (((i - halfStep) * offsetSize - sizeSoFar) + (totalSize / 2f) - (stepSize/2f));
            sizeSoFar += (stepHeight * tH);
            if (i == reducedStep)
                CreateMiddleSprite(pos, 1 - (reducedStep - targetStep).Abs(), transform, stepHeight * tH);
            else
                CreateSprite(pos, parentS, stepHeight * tH, i > targetStep == _inverseDirection);
        }
    }
    private void CreateSprite(float position, Transform parent, float size, bool valid)
    {
        // GENERATE HIERARCHY
        var go = new GameObject("MiddleSprite", typeof(RectTransform));
        go.transform.localPosition = Vector3.zero;
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = Vector2.one * size;
        var s = Instantiate(go, go.transform);
        s.AddComponent(typeof(Image));
        
        // RENAME
        s.name = "s";

        // SET SIZE & POSITION
        go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero.SetX(position);

        // SET SPRITE
        s.GetComponent<Image>().sprite = valid ? _validSprite : _invalidSprite;
    }
    private void CreateMiddleSprite(float position, float split, Transform parent, float size)
    {
        // GENERATE HIERARCHY
        var go = new GameObject("MiddleSprite", typeof(RectTransform));
        go.transform.localPosition = Vector3.zero;
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = Vector2.one * size;
        var s1 = Instantiate(go, go.transform);
        s1.AddComponent(typeof(RectMask2D));
        var s1sub = Instantiate(go, s1.transform);
        s1sub.AddComponent(typeof(Image));
        var s2 = Instantiate(s1, go.transform);
        var s2sub = s2.transform.GetChild(0).gameObject;
        Destroy(s1sub.transform.GetChild(0).gameObject);
        Destroy(s2sub.transform.GetChild(0).gameObject);
        var txt = new GameObject("text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        txt.transform.localPosition = Vector3.zero;
        txt.transform.SetParent(parent, false);
        
        // RENAME
        s1.name = "s1";
        s1sub.name = "s1sub";
        s2.name = "s2";
        s2sub.name = "s2sub";

        // SET SIZES & POSITIONS
        go.GetComponent<RectTransform>().anchoredPosition = Vector2.zero.SetX(position);

        var direction = _inverseDirection.ToInt();
        s1sub.GetComponent<RectTransform>().anchorMin = new Vector2(direction, 0.5f);
        s1sub.GetComponent<RectTransform>().anchorMax = new Vector2(direction, 0.5f);
        s2sub.GetComponent<RectTransform>().anchorMin = new Vector2(1 - direction, 0.5f);
        s2sub.GetComponent<RectTransform>().anchorMax = new Vector2(1 - direction, 0.5f);


        s1sub.GetComponent<RectTransform>().pivot = new Vector2(direction, 0.5f);
        s2sub.GetComponent<RectTransform>().pivot = new Vector2(1 - direction, 0.5f);
        s1sub.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        s2sub.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);



        s1.GetComponent<RectTransform>().anchorMin = new Vector2(direction, 0.5f);
        s1.GetComponent<RectTransform>().anchorMax = new Vector2(direction, 0.5f);
        s2.GetComponent<RectTransform>().anchorMin = new Vector2(1 - direction, 0.5f);
        s2.GetComponent<RectTransform>().anchorMax = new Vector2(1 - direction, 0.5f);

        s1.GetComponent<RectTransform>().pivot = new Vector2(direction, 0.5f);
        s2.GetComponent<RectTransform>().pivot = new Vector2(1 - direction, 0.5f);

        txt.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        txt.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        txt.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        txt.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().rect.size;
        txt.GetComponent<RectTransform>().localPosition = new Vector2(_textOffset, 0);


        if (_inverseDirection)
        {
            s1.GetComponent<RectTransform>().sizeDelta = new Vector2(size * (1 - split), size);
            s2.GetComponent<RectTransform>().sizeDelta = new Vector2(size * split, size);
        }
        else
        {
            s1.GetComponent<RectTransform>().sizeDelta = new Vector2(size * split, size);
            s2.GetComponent<RectTransform>().sizeDelta = new Vector2(size * (1 - split), size);
        }

        // SET SPRITES
        s1sub.GetComponent<Image>().sprite = _validSprite;
        s2sub.GetComponent<Image>().sprite = _invalidSprite;

        // SET CONTENT
        txt.GetComponent<TMPro.TMP_Text>().text = value.ToString();
        txt.GetComponent<TMPro.TMP_Text>().enableAutoSizing = true;
        txt.GetComponent<TMPro.TMP_Text>().fontSizeMin = 2;
        txt.GetComponent<TMPro.TMP_Text>().font = _font;
        txt.GetComponent<TMPro.TMP_Text>().color = _fontColor;
        txt.GetComponent<TMPro.TMP_Text>().alignment = TMPro.TextAlignmentOptions.Midline;
    }

    private void Destroy(GameObject gameObject)
    {
        if (Application.isPlaying)
            GameObject.Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }

}
