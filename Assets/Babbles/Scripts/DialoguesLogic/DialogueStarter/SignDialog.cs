using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Luminosity.IO;


public class SignDialog : MonoBehaviour
{

    #region Fields

    [Header("Yarn Spinner Configuration")]
    [SerializeField] private string _nodeName = null;


    [Header("UI Interact Button")]
    [SerializeField] private GameObject _UIInteractPosition = null;

    [SerializeField] private bool _disableMovement = true;

    [Header("Animation")]
    [SerializeField] private Animator[] _panneauAnimator = null;
    private Animator[] _lastPanneauAnimator = null;


    private Collider _player;


    #endregion Fields

    #region Properties
    #endregion Properties

    #region Methods


    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = transform.parent.GetComponent<Collider>();
            if (t.Character == null)
                t.Character = collider.transform;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = "Lire Panneau";
            UIManager.Instance.UIController.UIInteractText.gameObject.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.DialogueInteractionKey;
            _player = collider;

        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (_player == null)
            return;
        if (_player.tag == "Player")
        {

            if (DialogueManager.Instance.IsInDialog == true || UIManager.Instance.UIController.IsInGiveSituation)
            {
                // DialogueManager.Instance
                //Skip Dialogue
                Debug.Log("Already in dialog or give situation");
            }
            else if (InputManager.GetButtonDown("UI_Submit"))
            {
                AudioManager.Start3DSound("S_BruitLecturePanneau", transform);

                _lastPanneauAnimator = _panneauAnimator;

                UIManager.Instance.UIController.DialogueManager.AnimatorInit(_lastPanneauAnimator);

                UIManager.Instance.UIController.StartDialogue(_nodeName, _disableMovement);
                UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);

                var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
                t.Target = transform.GetComponent<Collider>();
            }

            Vector3 interactTextPosition = Camera.allCameras[0].WorldToScreenPoint(_UIInteractPosition.transform.position);
            UIManager.Instance.UIController.UIInteractText.transform.position = interactTextPosition;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);
            _player = null;
        }
    }

    #endregion Methods
}
