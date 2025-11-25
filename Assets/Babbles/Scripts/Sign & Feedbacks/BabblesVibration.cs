using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons;
using System.Linq;

public static class BabblesVibration
{

    private static GamepadVibration _noVibration;
    private static bool _vibrationActive = false;
    private static float _ongoingConstantVibration;
    private static List<float> _ongoingVibrations = new List<float>();

    private static void Start()
    {
        _noVibration = new GamepadVibration(0f, 0f, 0f, 0f);
        GamepadState.SetVibration(_noVibration, GamepadIndex.GamepadOne);
    }

    public static void CustomVibration(float timer, float strength)
    {
        if (_vibrationActive && _ongoingConstantVibration > strength)
            return;
        _ongoingVibrations.Add(strength);
        var highest = _ongoingVibrations.Max();
        GamepadVibration vibrationTest = new GamepadVibration(highest, highest, highest, highest);
        GamepadState.SetVibration(vibrationTest, GamepadIndex.GamepadOne);
        _ = ClemCAddons.Utilities.GameTools.DelayedCall((timer * 1000).Round(), () =>
        {
            _ongoingVibrations.Remove(strength);
            if (_ongoingVibrations.Count == 0 && !_vibrationActive)
                StopVibration();
            else if (_vibrationActive)
            {
                highest = _ongoingVibrations.Max().Max(_ongoingConstantVibration);
                GamepadVibration vibration = new GamepadVibration(highest, highest, highest, highest);
                GamepadState.SetVibration(vibration, GamepadIndex.GamepadOne);
            }
            else
            {
                highest = _ongoingVibrations.Max();
                GamepadVibration vibration = new GamepadVibration(highest, highest, highest, highest);
                GamepadState.SetVibration(vibration, GamepadIndex.GamepadOne);
            }
        });
    }


    public static void StartConstantVibration(float strength, bool overrideOngoing = false)
    {
        if (overrideOngoing && strength == 0)
        {
            StopConstantVibration();
            return;
        }
        if(_vibrationActive == false || strength > _ongoingConstantVibration || overrideOngoing)
        {
            _ongoingConstantVibration = strength;
            GamepadVibration vibrationTest = new GamepadVibration(strength, strength, strength, strength);
            GamepadState.SetVibration(vibrationTest, GamepadIndex.GamepadOne);
            _vibrationActive = true;
        }
    }

    public static void StopConstantVibration()
    {
        GamepadState.SetVibration(_noVibration, GamepadIndex.GamepadOne);
        _vibrationActive = false;
    }


    

    private static void StopVibration()
    {
        GamepadState.SetVibration(_noVibration, GamepadIndex.GamepadOne);
    }

}
