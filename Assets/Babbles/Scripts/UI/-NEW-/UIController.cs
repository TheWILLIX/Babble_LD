using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Player;
using UnityEngine.UI;
using TMPro;
using ClemCAddons.CameraAndNodes;
using ClemCAddons;
using Luminosity.IO;
using Yarn.Unity;
using System.Linq;
using UnityEngine.Video;

public class UIController : MonoBehaviour
{
    #region Fields

    [SerializeField] private TextEffectDialogue _textEffect = null;
    [SerializeField] private VoiceSFXDialogue _voiceSFX = null;

    [Header("Main (!Need Reference!)")]
    [SerializeField] private CharacterMovement _character = null;
    private Animator _characterAnimator;

    [Header("Other References")]
    [SerializeField] private BananeManager _bananePannel = null;
    private bool _inventoryOpen = false;
    private bool _carnetOpen = false;


    private bool _ignoreInputs = false;

    [SerializeField] private GameObject _uiInteractText = null;

    [SerializeField] private GameObject _uIVFXContaineer = null;

    //[SerializeField] private NPCPositionMemory _npcPositionMemory = null; //No Longer Usefull

    [Header("Dialogues References")]
    [SerializeField] private DialogueManager _dialogueManager = null;


    [SerializeField] private Image _dialogueBox = null;
    [SerializeField] private TMP_Text _characterNameText = null;
    [SerializeField] private Image _characterNameView = null;
    [SerializeField] private Image _bulleContour = null;
    private bool _isInTutorialBananeDialogue = false;


    [Header("InteractionTree")]
    [SerializeField] private BabblesOptionsListView _choiceInteraction = null;
    // [SerializeField] private Button _talkChoice = null;
    // [SerializeField] private Button _giveChoice = null;

    private bool _isInGiveSituation = false;
    private bool _isInInteractionChoice = false;
    private bool _isSpeedCheatActive = false;

    [Header("Sign & Feedback")]
    [SerializeField] private HUDKeyBank _hudBank = null;

    [Header("Qu�te Napol�on")] //We Gonna HAVE TO DELETE THAT SHIT
    [SerializeField] private GameObject _napoleonCarteObject = null;
    private bool _napoleonCarteGiven = false;
    private bool _carteIsActive = false;

    [Header("Carnet")]
    [SerializeField] private Paging _paging = null;

    [SerializeField] private ObjectifsUpdate _objectifsUpdate = null;

    [Header("Hotu Challenge")]
    [SerializeField] private TMP_Text _timerTextHotu = null;
    [SerializeField] private TMP_Text _numberOfMeduseHotu = null;
    [SerializeField] private Animator _countDownHotuChallenge = null;

    [Header("Fade")]
    [SerializeField] private Fade _fade = null;

    [Header("For the Demo (Oral) delete this in july and august 2022")]
    [SerializeField] private Item _brassartItem = null;
    [SerializeField] private Item _meduseItem = null;

    private bool _brassartActivated = false;

    [Header("Tutorial Video")]
    [SerializeField] private VideoPlayer _topRightVideoPlayer = null;
    [SerializeField] private VideoPlayer _bottomLeftVideoPlayer = null;

    #endregion Fields


    #region Properties
    public bool InventoryOpen => _inventoryOpen;
    public bool CarnetOpen => _carnetOpen;

    public DialogueManager DialogueManager => _dialogueManager;
    public Image DialogueBox => _dialogueBox;
    public Image CharacterNameView => _characterNameView;
    public Image BulleContour => _bulleContour;

    public Animator CharacterAnimator => _characterAnimator;

    //Others Properties
    public HUDKeyBank HUDBank => _hudBank;
    public GameObject UIVFXContaineer => _uIVFXContaineer;

    public GameObject UIInteractText
    {
        get
        {
            return _uiInteractText;
        }
        set
        {
            _uiInteractText = value;
        }
    }

    public TMP_Text CharacterNameText
    {
        get
        {
            return _characterNameText;
        }
        set
        {
            _characterNameText = value;
        }
    }


    public bool IsInInteractionChoice
    {
        get
        {
           return _isInInteractionChoice;
        }
        set
        {
            _isInInteractionChoice = value;
        }
    }

    public bool IsInGiveSituation
    {
        get
        {
           return _isInGiveSituation;
        }
        set
        {
            _isInGiveSituation = value;
        }
    }

    public BananeManager BananeManager { get => _bananePannel;}

    public CharacterMovement Character
    {
        get
        {
            return _character;
        }
        set
        {
            _character = value;
        }
    }

    public Paging Paging => _paging;

    public ObjectifsUpdate ObjectifsUpdate { get => _objectifsUpdate; }
    public TMP_Text TimerTextHotu { get => _timerTextHotu; set => _timerTextHotu = value; }
    public TMP_Text NumberOfMeduseHotu { get => _numberOfMeduseHotu; set => _numberOfMeduseHotu = value; }
    public Animator CountDownHotuChallenge { get => _countDownHotuChallenge;  }
    public Fade Fade { get => _fade; set => _fade = value; }
    public bool IsInTutorialDialogue { get => _isInTutorialBananeDialogue; set => _isInTutorialBananeDialogue = value; }
    public VideoPlayer TopRightVideoPlayer { get => _topRightVideoPlayer; set => _topRightVideoPlayer = value; }
    public VideoPlayer BottomLeftVideoPlayer { get => _bottomLeftVideoPlayer; set => _bottomLeftVideoPlayer = value; }
    #endregion Properties

    #region Methods

    #region Start, Awake & Update
    void Awake()
    {
         UIManager.Instance.UIController = this;
        _characterAnimator = FindObjectsOfType<Animator>().First(t => t.gameObject.name == "Animator_Bulle");
    }

    void Update()
    {

        if (InputManager.GetButtonDown("UI_Cancel"))
        {
            
            if (_carteIsActive == true)
            {
                MapOpening(false);
            }

            if(_inventoryOpen == true && !BananeManager.LockExit)
            {
                CloseInventory(true);
            }
        }

        if (InputManager.GetButtonDown("Carnet")) //Open Carnet
        {
            if (_carteIsActive == true)
            {
                MapOpening(true);
            }
            else if (_carteIsActive == false && _napoleonCarteGiven == true)
            {
                MapOpening(false);
            }
        }


        if (InputManager.GetButtonDown("InventoryInterface")) //Open Inventory
        {
            if (CanOpenInventory())
            {
                _ignoreInputs = true;

                if (Notification.CurrentStep(0) != Notification.NotificationSteps.Ready && Notification.CurrentStep(0) != Notification.NotificationSteps.Closed)
                    return;

                if (_inventoryOpen == false)
                {
                    CharacterMovementDisable();

                    //Audio :
                    AudioManager.Start2DSound("S_OuvertureInventaire");

                    Transition.StartTransition(OpenInventory);
                   // BabblesVibration.CustomVibration(0.2f,0.5f); //Pas Agréable !!
                }
                else if (!BananeManager.LockExit)
                {
                    //BabblesVibration.CustomVibration(0.2f,0.5f); //Pas Agréable !!
                    Transition.StartTransition(new Transition.TransitionCallback[] { CloseInventory, CharacterMovementEnable });
                    if(BananeManager.IsSelectionMode == true)
                    {
                        DialogueManager.ExitInteractionChoice();
                    }
                }
            }
        }

        //Will be different, need changes
        if (_isInInteractionChoice == true)
        {
            if(InputManager.GetButtonDown("UI_Cancel") || InputManager.GetButtonDown("Carnet"))
            {
                Debug.Log("EXIT");
                DialogueManager.ExitInteractionChoice();
            }
            else if(InputManager.GetButtonDown("InventoryInterface"))
            {
                //Object Give
                Debug.Log("Give Object");
            }
        }

        if (InputManager.GetButtonDown("MoveCheat") == true && false) // deactivated
        {
            //Alexis please stop using this as a debug it's desactivated !!!

            if (_isSpeedCheatActive == false)
            {
                Time.timeScale = 2;
                _isSpeedCheatActive = true;
            }
            else
            {
                Time.timeScale = 1;
                _isSpeedCheatActive = false;
            }
        }

        if(InputManager.GetButtonDown("PoulpeCoussinCheat") == true)
        {
            InventoryManager.Instance.PlaceItem(InventoryManager.Instance.GetItemDataByString("PoulpeCoussin"));
            Fade.FadeIn();
        }

        if (InputManager.GetButtonDown("UnlockPlayer"))
        {
            UIManager.Instance.UIController.Character.SetCanMove(true);
            Fade.FadeOut();
        }

        if (InputManager.GetButtonDown("UnlockRecette"))
        {
            Debug.Log("Unlock Recette Cheat");
            InventoryManager.Instance.UnlockRecipeByString("REC_ChapeauHelice1");
        }

   /*     if(InputManager.GetButtonDown("CheatBrassart")) //Not usefull anymore (Was usefull for the oral presentation)
        {
            if(_brassartActivated == false)
            {
                Debug.Log("Cheat Brassart Activation");
                BananeManager.MeduseItemInteraction.RessourceType = _brassartItem;
                _brassartActivated = true;
            }
            else
            {
                Debug.Log("Cheat Brassart Desactivation");
                BananeManager.MeduseItemInteraction.RessourceType = _meduseItem;
                _brassartActivated = false;
            }

        }*/


    }
    #endregion Start, Awake & Update

    #region Inventory
    public void OpenInventory()
    {
        if (!CanOpenInventory())
            return;
        _bananePannel.gameObject.SetActive(true);
        _inventoryOpen = true;

        _ignoreInputs = false;

        _bananePannel.OpeningBanane();
        _bananePannel.UpdateBanane();

    }
    public void OpenInventory(string defaultSelection = "")
    {
        if (!CanOpenInventory())
            return;
        _bananePannel.gameObject.SetActive(true);
        _inventoryOpen = true;

        _ignoreInputs = false;

        _bananePannel.OpeningBanane(defaultSelection: defaultSelection);
        _bananePannel.UpdateBanane();

    }

    public static bool CanOpenInventory()
    {
        return !DialogueManager.Instance.IsInDialog && PauseMenuButtons.CurrentState == 0;
    }

    public static bool CanPause()
    {
        return !DialogueManager.Instance.IsInDialog && UIManager.Instance.UIController.InventoryOpen == false;
    }

    public void OpenInventory(bool transition = false, bool selectionMode = false, string defaultSelection = "")
    {
        if (_inventoryOpen)
        {
            _bananePannel.SetSelection(defaultSelection);
            return;
        }
        if (!CanOpenInventory() && !selectionMode)
            return;

        if (transition)
        {
            if(selectionMode)
                Transition.StartTransition(new Transition.TransitionCallback[] { () => OpenInventorySelection(defaultSelection), CharacterMovementDisable,  });
            else
                Transition.StartTransition(new Transition.TransitionCallback[] { () => OpenInventory(defaultSelection), CharacterMovementDisable });
            return;
        }
        _bananePannel.gameObject.SetActive(true);
        _inventoryOpen = true;

        _ignoreInputs = false;

        _bananePannel.OpeningBanane(defaultSelection: defaultSelection);
        _bananePannel.UpdateBanane();

    }

    private void OpenInventorySelection(string defaultSelection = "")
    {
        if (!CanOpenInventory())
            return;
        Debug.Log("OpenInventorySelection");
        _bananePannel.gameObject.SetActive(true);
        _inventoryOpen = true;

        _ignoreInputs = false;

        IsInGiveSituation = true;

        _isInInteractionChoice = false; //Had to put this here as a safety against YarnSpinner triggering the Choice(true) command a second time for no reason.

        _bananePannel.OpeningBanane(true, defaultSelection);
        _bananePannel.UpdateBanane();
    }

    public void CloseInventory(bool transition = false)
    {

        TutorialManager.Instance.HideTutoVideo();


        if (_inventoryOpen == false)
            return;
        if (transition)
        {
            Transition.StartTransition(new Transition.TransitionCallback[] { CloseInventory, CharacterMovementEnable });
            return;
        }
        _bananePannel.gameObject.SetActive(false);
        _inventoryOpen = false;
        CharacterMovementEnable();
        _bananePannel.ClosingBanane();
        IsInGiveSituation = false;
    }

    public void CloseInventory()
    {

        TutorialManager.Instance.HideTutoVideo();

        _bananePannel.gameObject.SetActive(false);
        _inventoryOpen = false;
        _bananePannel.ClosingBanane();
    }

    #endregion Inventory

    #region Settings
    private bool settingsState;
    public bool TryPause(bool open)
    {
        if (open)
        {
            if (_inventoryOpen || _isInInteractionChoice)
                return false;
            CharacterMovementDisable();
        }
        else
        {
            CharacterMovementEnable();
        }
        settingsState = !settingsState;
        return true;
    }
    #endregion Settings

    public void CharacterMovementEnable()
    {
        var r = Camera.allCameras[0].GetComponent<TPSCameraWithNodeSupport>();
        if (r != null)
            r.BlockCam = false;
        _character.SetCanMove(true);
        _ignoreInputs = false;
    }

    public void CharacterMovementDisable()
    {
        var r = Camera.allCameras[0].GetComponent<TPSCameraWithNodeSupport>();
        if (r != null)
            r.BlockCam = true;
        _character.SetCanMove(false);
    }


    #region Dialogue
    //-- Start Dialogues & End Dialogues
    public void StartDialogue(string dialogueToStart, bool lockMovement)
    {

        if (DialogueManager.IsInDialog == true || IsInGiveSituation == true)
        {
            Debug.LogError("Already In Dialog or in giveSituation");
            return;
        }

        if (lockMovement == true) //A Mode to disable Input (Used when talking to normal and important NPC)
        {
            _character.SetCanMove(false); //Animation has to be canceled or replaced

            _dialogueManager.DialogueRunner.StartDialogue(dialogueToStart);
            _dialogueManager.OnNodeStart();

            //There will probably be stuff with the camera
        }
        else
        {
            _dialogueManager.DialogueRunner.StartDialogue(dialogueToStart);
            _dialogueManager.OnNodeStart();
        }
    }

    public void StartDialogueOverride(string dialogueToStart, bool lockMovement)
    {
        if ( IsInGiveSituation == true)
        {
            Debug.LogError("Already in giveSituation");
            return;
        }


        if(DialogueManager.IsInDialog == true)
        {
            DialogueStopped();
            _dialogueManager.DialogueRunner.Stop();
        }

        if (lockMovement == true) //A Mode to disable Input (Used when talking to normal and important NPC)
        {
            _character.SetCanMove(false); //Animation has to be canceled or replaced

            _dialogueManager.DialogueRunner.StartDialogue(dialogueToStart);
            _dialogueManager.OnNodeStart();

            //There will probably be stuff with the camera
        }
        else
        {
            _dialogueManager.DialogueRunner.StartDialogue(dialogueToStart);
            _dialogueManager.OnNodeStart();
        }
    }

    public void DialogueStopped() 
    {
        _dialogueManager.OnDialogueComplete();

        if (UIManager.Instance.UIController.IsInTutorialDialogue != true)
        {
            _character.SetCanMove(true);
        }
        else
        {
            UIManager.Instance.UIController.IsInTutorialDialogue = false;
        }
        NPCCam.Hide();
        _dialogueManager.IsInLeaveDialog = false;

        //var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
        // t.Target = null;
    }

    public void InteractionChoice(bool condition)
    {
        Debug.Log("InteractionChoice = " + condition);
        _isInInteractionChoice = condition;
    }


  


    #endregion Dialogue

    #region Sign & Feedback
    public void NapoleonQuestSetup()
    {
        _napoleonCarteGiven = true;
        _napoleonCarteObject.SetActive(true);
        StartCoroutine(NapoleonCarteStart());
    }

    private void MapOpening(bool condition)
    {
        //En attendant d'avoir un vrai carnet j'ai juste parametrer la carte au tr�sor de napol�on
        if (condition == false)
        {
            _napoleonCarteObject.SetActive(true);
            _carteIsActive = true;
        }
        else
        {
            _napoleonCarteObject.SetActive(false);
            _carteIsActive = false;
        }
    }

    IEnumerator NapoleonCarteStart()
    {
        yield return new WaitForSeconds(5f);
        _napoleonCarteObject.SetActive(false);
    }


    #endregion Sign & Feedback


    #endregion Methods

}
