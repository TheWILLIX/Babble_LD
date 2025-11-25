using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Luminosity.IO;
using ClemCAddons;

public class PauseMenuButtons : MainMenuButtons
{
    [SerializeField] private float _hiddenTarget;
    private static int _currentState = 0;
    private UIController _uicontroller;

    private float _basePos;

    public static int CurrentState { get => _currentState; set => _currentState = value; }

    void Awake()
    {
        _uicontroller = transform.GetComponentInParent<UIController>();
        _currentState = 0;
        _basePos = transform.parent.position.x;
        transform.parent.position = transform.parent.position.SetX(_hiddenTarget);
    }

    private void MoveTo(float target, int newState)
    {
        _currentState = -1;
        ClemCAddons.Utilities.Lerper.ConstantLerp(transform.parent.position.x, target, 0.5f, (f) =>
        {
            transform.parent.position = transform.parent.position.SetX(f);
        }, () => {
            transform.parent.position = transform.parent.position.SetX(target);
            _currentState = newState;
            if(newState == 1)
                _defaultSelection.Select();
        });
    }

    public override void Update()
    {
        if (InputManager.GetButtonDown("Start") && UIController.CanPause())
        {
            SwitchState();
        }
    }

    private void SwitchState()
    {
        switch (_currentState)
        {
            case 0:
                if(_uicontroller.TryPause(true))
                    MoveTo(_basePos, 1);
                break;
            case 1:
                if (_uicontroller.TryPause(false))
                {
                    MoveTo(_hiddenTarget, 0);
                    EventSystem.current.SetSelectedGameObject(null);
                }
                break;
            default:
                break;
        }
    }

    public void Feedback()
    {
        Application.OpenURL("https://forms.gle/gSfxKCSaNEEfv8k88");
    }

    public void Resume()
    {
        AudioManager.Start2DSound("S_MenuValidation");
        SwitchState();
    }

    public override void Leave()
    {
        AudioManager.Start2DSound("S_MenuValidation");
        SceneManager.LoadScene("ClemCa MainMenu");
    }

    public override void Options()
    {
        AudioManager.Start2DSound("S_Saut");
        AudioManager.Start2DSound("S_MenuYes");
        _optionsMenu.Show();
        _currentState = -1;
        _firstOption.Select();
    }

    public override void HideOptions()
    {
        AudioManager.Start2DSound("S_MenuNo");
        _optionsMenu.Hide();
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
        {
            _currentState = 1;
        });
    }
}
