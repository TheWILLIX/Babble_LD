using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Luminosity.IO;
using ClemCAddons;
using ClemCAddons.Player;

public class InteractNPC : MonoBehaviour
{
    #region Fields

    [Header("Yarn Spinner Configuration")]
    [SerializeField] private SpeakerData _NPC = null;
    [SerializeField] private ELevelType _NPCLevelPosition = ELevelType.NONE;


    [Header("UI Interact Button")]
    [SerializeField] private bool _hideNPCName = false;
    [SerializeField] private GameObject _UIInteractPosition = null;
    [Header("Camera")]
    [SerializeField] private Transform _PlayerTargetPosition = null;
    [Header("Sign & Feedback")]
    [SerializeField] private GameObject _dialogueBubbleVFX = null;

    [SerializeField] private Animator[] _npcAnimator = null;

    private bool _interactionGate = false;

    private Collider _player;

    //Pour les Jeunes Ablette == 0, Gumpy == 1 , Dumbo == 2
    private Animator[] _lastNPCAnimator = null;

    #endregion Fields


    #region Properties

    public SpeakerData NPCData => _NPC;

    public GameObject UIInteractPosition => _UIInteractPosition;

    #endregion Properties


    #region Methods




    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            if (t.Character == null)
                t.Character = collider.transform;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
            if(_hideNPCName == true)
            {
                UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = "";
            }
            else
            {
                UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = _NPC.CharacterName;
            }
            UIManager.Instance.UIController.UIInteractText.gameObject.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.DialogueInteractionKey;
            _player = collider;

        }
    }

    void Update()
    {
        if (_player == null)
            return;
        if (_player.tag == "Player")
        {
            if (InputManager.GetButtonDown("UI_Submit"))
            {

                if (DialogueManager.Instance.IsInDialog == true)
                {
                    // DialogueManager.Instance
                    //Skip Dialogue
                    Debug.Log("Already in dialog ");
                    return;
                }
                else if(UIManager.Instance.UIController.IsInGiveSituation == true || (EpaveCustomBehaviour.HasInstance == true && EpaveCustomBehaviour.Instance?.IsInChallenge == true))
                {
                    Debug.Log("Already in give situation or In Challenge");
                    return;
                }

                _lastNPCAnimator = _npcAnimator;

                UIManager.Instance.UIController.DialogueManager.AnimatorInit(_lastNPCAnimator);
               

                if (_NPC.IsPNJClassic == true && Seum.Instance.GetSeum() >= 66.6f && _NPC.SeumHasInfluence == true)
                {
                    UIManager.Instance.UIController.StartDialogue(_NPC.CharacterName + "_Seum", true);
                    UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);

                }
                else
                {
                    switch (_NPC.name)
                    {
                        case "Jeunes":
                            if(AudioManager.Instance.NPCThemePlaying == false)
                            {
                                AudioManager.Instance?.StartNPCThemeMusic("M_JeunesTheme");
                            }

                            break;

                        case "Marline":
                            if (AudioManager.Instance.NPCThemePlaying == false)
                            {
                                AudioManager.Instance?.StartNPCThemeMusic("M_MarlineTheme");
                            }
                            break;

                        case "Urfe":
                            if (AudioManager.Instance.NPCThemePlaying == false)
                            {
                                AudioManager.Instance?.StartNPCThemeMusic("M_UrfeTheme");
                            }
                            break;
                    }

                    Debug.Log("Start Dialogue");
                    //  NPCCam.Show(transform, _PlayerTargetPosition.position.SetY(_player.transform.position.y) + Vector3.up * (3 - ClemCAddons.Utilities.GameTools.FindGround(_PlayerTargetPosition.position + Vector3.up * 3, 0, 3, LayerMask.GetMask("Default"))));
                    UIManager.Instance.UIController.StartDialogue(_NPC.CharacterName + "_Intro_" + _NPCLevelPosition, true);
                    UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);
                    Instantiate(_dialogueBubbleVFX, _UIInteractPosition.transform.position, Quaternion.Euler(-90, 0, 0), UIManager.Instance.UIController.UIVFXContaineer.transform);  //Pour des raisons obscur les FX quand ils spawnent se dirigent en positif sur l'axe z aux lieu de l'axe y comme sur les prefabs. J'ai pas trouver d'autres solutions donc un ptit -90 en mode gros sale

                    var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
                    t.Target = transform.GetComponent<Collider>();
                }

               
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
