using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Selectable _defaultSelection;
    [SerializeField] private OptionsMenu _optionsMenu;
    void OnEnable()
    {
        _defaultSelection.Select();
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(10, () => { _optionsMenu.Show(); });
    }
    public void Hide(Action callback)
    {
        _optionsMenu.Hide();
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, () => { callback.Invoke(); });
    }

    public void ShowBack()
    {
        _defaultSelection.Select();
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(10, () => { _optionsMenu.Show(); });
    }
}
