using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonNavigationSound : MonoBehaviour, ISelectHandler
{
   

    public void OnSelect(BaseEventData eventData)
    {

        AudioManager.Start2DSound("S_MenuSelect");
    }


}
