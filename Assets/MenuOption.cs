using ClemCAddons;
using Luminosity.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MenuOption : MonoBehaviour
{
    [Header("Refreshing")]
    [SerializeField] private bool _editMode = true;
    [Header("LiveSetting")]
    [SerializeField] private Setting _setting;
    [SerializeField] private SettingType _settingType;
    [SerializeField] private float _defaultValue;
    [Header("Button")]
    [SerializeField] private Selectable _settingButton;
    [Header("Background")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Color _backgroundColor;
    [SerializeField] private Sprite _backgroundSprite;
    [Header("Text")]
    [SerializeField] private TMPro.TMP_Text _text;
    [SerializeField] private string _textContent;
    [SerializeField] private bool _freeText = false;
    [Header("CheckMark"), SerializeField, DrawIf("_settingType", SettingType.Boolean, ComparisonType.Equals)] private Image _checkmarkImage;
    [SerializeField, DrawIf("_settingType", SettingType.Boolean, ComparisonType.Equals)] private Color _checkmarkFalseColor;
    [SerializeField, DrawIf("_settingType", SettingType.Boolean, ComparisonType.Equals)] private Color _checkmarkTrueColor;
    [SerializeField, DrawIf("_settingType", SettingType.Boolean, ComparisonType.Equals)] private Sprite _checkmarkFalseSprite;
    [SerializeField, DrawIf("_settingType", SettingType.Boolean, ComparisonType.Equals)] private Sprite _checkmarkTrueSprite;
    [Header("Selection"), SerializeField, DrawIf("_settingType", SettingType.Selection, ComparisonType.Equals)] private TMPro.TMP_Dropdown _dropdown;
    [SerializeField, DrawIf("_settingType", SettingType.Selection, ComparisonType.Equals)] private List<string> _choices;
    [Header("Slider"), SerializeField, DrawIf("_settingType", SettingType.NumericalValue, ComparisonType.Equals)] private CustomSlider _slider;
    [SerializeField, DrawIf("_settingType", SettingType.NumericalValue, ComparisonType.Equals)] private bool _sliderInt;
    [SerializeField, DrawIf("_settingType", SettingType.NumericalValue, ComparisonType.Equals)] private float _sliderMinValue;
    [SerializeField, DrawIf("_settingType", SettingType.NumericalValue, ComparisonType.Equals)] private float _sliderMaxValue;
    [SerializeField, DrawIf("_settingType", SettingType.NumericalValue, ComparisonType.Equals)] private TMPro.TMP_Text _sliderValueText;
    [Header("Calls")]
    [SerializeField] private OptionFloat _applyFloat;
    [SerializeField] private OptionInt _applySelection;
    [SerializeField] private OptionBool _applyBool;


    [Serializable]
    public class OptionBool : UnityEvent<bool> { }

    [Serializable]
    public class OptionFloat : UnityEvent<float> { }

    [Serializable] 
    public class OptionInt : UnityEvent<int> { }


    private bool _dirty = true;

    public enum Setting // all boolean types at the top
    {
        YAxis,
        MouseKeyboardMode,
        ThermometerVisibility,
        FOV,
        JumpInputMode,
        Language
    }

    public enum SettingType
    {
        Boolean,
        Selection,
        NumericalValue
    }

    void Start()
    {
        if(_editMode || Application.isPlaying)
            DefaultSetup();
    }

    void Update()
    {
        if (!_editMode && !Application.isPlaying)
            return;
        if (!Application.isPlaying)
        {
            DefaultSetup();
            return;
        }
        if (_settingType == SettingType.Selection && _dropdown != null // settings are right
            && EventSystem.current.currentSelectedGameObject == _dropdown.gameObject && !_dropdown.IsExpanded) // dropdown is selected but not expanded
            _dropdown.onValueChanged.Invoke(_dropdown.value); // trigger ourselves, avoids not doing anything when value is validated unchanged

        if (!_dirty && ClemCAddons.Utilities.Timer.MinimumDelay("menuoptionRefresh".GetHashCode(),100))
            return;
        
        var settingValue = GetSetting(_setting, _settingType);
        switch (_settingType)
        {
            case SettingType.Boolean:
                _checkmarkImage.color = settingValue == 1 ? _checkmarkTrueColor : _checkmarkFalseColor;
                _checkmarkImage.sprite = settingValue == 1 ? _checkmarkTrueSprite : _checkmarkFalseSprite;
                break;
            case SettingType.NumericalValue:
                _slider.value = settingValue;
                _slider.Redraw();
                if(_sliderValueText != null)
                    _sliderValueText.text = settingValue.ToString();
                break;
            case SettingType.Selection:
                _dropdown.SetValueWithoutNotify((int)settingValue);
                break;
        }
        _dirty = false;
    }

    private void DefaultSetup()
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = _backgroundColor;
            _backgroundImage.sprite = _backgroundSprite;
        }
        if (_text != null && !_freeText)
            _text.text = _textContent;
        if(_slider != null)
        {
            _slider.value = _defaultValue;
            _slider.minValue = _sliderMinValue;
            _slider.maxValue = _sliderMaxValue;
            _slider.wholeNumbers = _sliderInt;
            if (_sliderValueText != null)
                _sliderValueText.text = _defaultValue.ToString();
        }
        if(_checkmarkImage != null)
        {
            _checkmarkImage.color = _defaultValue == 1 ? _checkmarkTrueColor : _checkmarkFalseColor;
            _checkmarkImage.sprite = _defaultValue == 1 ? _checkmarkTrueSprite : _checkmarkFalseSprite;
        }
        if(_dropdown != null)
        {
            _dropdown.ClearOptions();
            _dropdown.AddOptions(_choices);
            _dropdown.value = (int)_defaultValue;
        }
    }

    private float GetSetting(Setting setting, SettingType settingType)
    {
        if (!PlayerPrefs.HasKey("Setting" + setting.ToString()))
        {
            UpdateSetting(_defaultValue);
        }
        if (settingType == SettingType.NumericalValue)
        {
            return PlayerPrefs.GetFloat("Setting" + setting.ToString());
        }
        return PlayerPrefs.GetInt("Setting" + setting.ToString());
    }

    private void SetDirty()
    {
        _dirty = true;
    }

    public void UpdateSetting(float value)
    {
        AudioManager.Start2DSound("S_MenuYes");
        PlayerPrefs.SetFloat("Setting" + _setting.ToString(), value.Clamp(_sliderMinValue, _sliderMaxValue));
        PlayerPrefs.Save();
        SetDirty();
        _applyFloat.Invoke(value);
        ApplySettings();
    }

    public void UpdateSetting(int value)
    {
        AudioManager.Start2DSound("S_MenuYes");
        PlayerPrefs.SetInt("Setting" + _setting.ToString(), _settingType != SettingType.NumericalValue ? value : Mathf.Clamp(value, _sliderMinValue.Round(), _sliderMaxValue.Round()));
        PlayerPrefs.Save();
        SetDirty();
        _applySelection.Invoke(value);
        ApplySettings();
    }

    public void UpdateSetting(bool value)
    {
        AudioManager.Start2DSound("S_MenuYes");
        PlayerPrefs.SetInt("Setting" + _setting.ToString(), value.ToInt());
        PlayerPrefs.Save();
        SetDirty();
        _applyBool.Invoke(value);
        ApplySettings();
    }

    public void FlipSetting()
    {
        AudioManager.Start2DSound("S_MenuYes");
        var value = !GetSetting(_setting, _settingType).Round().ToBool();
        PlayerPrefs.SetInt("Setting" + _setting.ToString(), value.ToInt());
        PlayerPrefs.Save();
        SetDirty();
        ApplySettings();
    }

    public void DelayedReselect(int delay)
    {
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(delay, () => { _settingButton.Select(); });
    }

    public void ApplySettings()
    {
        AudioManager.Start2DSound("S_MenuValidation");
        SettingsApplier.ApplyAll();
    }
}
