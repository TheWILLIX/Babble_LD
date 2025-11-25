using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using Luminosity.IO;
using ClemCAddons.CameraAndNodes;
using ClemCAddons;

public class TriggerDialogue : MonoBehaviour
{
    #region Fields

    [Header("Yarn Spinner Configuration")]
    [SerializeField] private string _yarnStartNode = string.Empty;

    [SerializeField] private bool _repeatOnTrigger = false;
    private bool _triggerEnabled = false;

    [Header("Others")]

    [SerializeField] private bool _desactivateSomeStuff = false;
    [SerializeField] private GameObject[] _stuffToDesactivate = null;

    [SerializeField] private bool _disableMovement = false;

    [Header("Target")]
    [SerializeField] private Transform _npcTarget = null;
    [SerializeField] private Collider _npcCollider = null;
    [SerializeField] private bool _cameraWork = false;

    [SerializeField] private int _cameraSwitchDuration = 2000;

    [SerializeField] private bool _tutorialDialogue = false;

    #endregion Fields



    #region Methods



    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
           if(_tutorialDialogue == true && TutorialManager.Instance.TutorialDone == true)
           {
                return;
           }
           else
           {
                if (_repeatOnTrigger == true || _triggerEnabled == false)
                {
                    if (_desactivateSomeStuff == true && _stuffToDesactivate.Length >= 1)
                    {
                        foreach (GameObject gameObject in _stuffToDesactivate)
                        {
                            gameObject.SetActive(false);
                        }
                    }
                    UIManager.Instance.UIController.StartDialogue(_yarnStartNode, _disableMovement);
                    _triggerEnabled = true;

                    if (_npcCollider != null)
                    {
                        var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
                        t.Target = _npcCollider;
                        if (t.Character == null)
                            t.Character = collider.transform;
                    }

                    if (_cameraWork == true)
                    {
                        UIManager.Instance.UIController.Character.TpsCamera.LookAtTransform(2000, 1000, _npcTarget, StartCountdown);
                        // UIManager.Instance.UIController.Character
                    }
                }
            }

          


        }
        
    }
    private void StartCountdown()
    {

    }

    #endregion Methods


}
