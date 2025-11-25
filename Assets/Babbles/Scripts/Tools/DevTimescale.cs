using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using Luminosity.IO;
using ClemCAddons.Utilities;

public class DevTimescale : MonoBehaviour
{
    [SerializeField] private bool SetText;
    [SerializeField] private List<float> _modes = new List<float> { 1, 3, 10, 0.5f };
    [SerializeField] private float _currentMode = 1; // for display purposes
    private int _currentModeID = 0;

    void Start()
    {
        Time.timeScale = _currentMode;
        _currentModeID = _modes.FindIndex(t => t == _currentMode);
        if (_currentModeID == -1)
        {
            _modes.Add(_currentMode);
            _currentModeID = _modes.Count - 1;
        }
    }
    void Update()
    {
        if (InputManager.GetButtonDown("MoveCheat"))
        {
            _currentModeID = _currentModeID.LoopAround(_modes.Count);
            _currentMode = _modes[_currentModeID];
            Time.timeScale = _currentMode;
            if (!SetText)
                return;
            var txt = GetComponent<TMPro.TMP_Text>();
            txt.text = "x"+_currentMode.ToString();
            txt.color = Color.white;
            Lerper.ConstantLerp(1, 0, 2, (f) => { txt.color = Color.white.SetA(f); });
        }
    }
}
