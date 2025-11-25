using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoBananeTrigger : MonoBehaviour
{

    [SerializeField] private bool _fleurTutoPart = false;
    private bool _fleurPartTriggered = false;

    [SerializeField] private bool _craftTutoPart = false;
    private bool _craftPartTriggered = false;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {

            if(_fleurTutoPart == true && _fleurPartTriggered == false)
            {
                _fleurPartTriggered = true;
                UIManager.Instance.UIController.BananeManager.InFleurTutoPart = true;

            }

            if (_craftTutoPart == true && _craftPartTriggered == false)
            {
                _craftPartTriggered = true;
                UIManager.Instance.UIController.BananeManager.InCraftTutoPart = true;
            }

            
        }

    }
}
