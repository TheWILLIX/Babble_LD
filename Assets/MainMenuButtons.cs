using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{
    [SerializeField] protected OptionsMenu _optionsMenu = null;
    [SerializeField] private CreditsMenu _creditsMenu = null;

    [SerializeField] protected Selectable _defaultSelection;
    [SerializeField] protected Selectable _firstOption = null;

    public virtual void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            _defaultSelection.Select();
        }
    }

    public void Play()
    {
        AudioManager.Start2DSound("S_MenuValidation");

        Debug.Log("Show Loading Screen");
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            SceneManager.LoadScene("Level 1 WIL");
        });

    }

    public virtual void Leave()
    {
        AudioManager.Start2DSound("S_MenuValidation");
        Application.Quit();
    }

    public virtual void Credits()
    {
        AudioManager.Start2DSound("S_MenuValidation");
        _creditsMenu.Show();
        _firstOption.Select();

    }

    public virtual void Options()
    {
        AudioManager.Start2DSound("S_Saut");
        AudioManager.Start2DSound("S_MenuYes");
        _optionsMenu.Show();
        _firstOption.Select();
    }

    public virtual void HideOptions()
    {
        AudioManager.Start2DSound("S_MenuNo");
        _optionsMenu.Hide();
    }
}
