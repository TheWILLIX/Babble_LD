using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    #region Fields
    private UIController _uiController = null;
    #endregion Fields


    #region Property
    
    public UIController UIController
    {

        get
        {
            if (_uiController == null)
                Debug.LogError("UI Controller is missing in the scene");
            return _uiController;
        }
        set
        {
            if (_uiController == null)
            {
                _uiController = value;
            }
        }

    }
    #endregion Property

    /*
    #region Methods

    public void Initialize()
    {
        base.Start();
    }


    #endregion Methods
    */
}
