using Luminosity.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ClemCAddons.Player;
using ClemCAddons.CameraAndNodes;
using ClemCAddons;
/*
public class UIControllerOld : SerializedMonoBehaviour
{
    #region Fields


    #region Main

    [Header("Player Data")]
    [SerializeField] private CharacterMovement _characterMovement = null;


    [Header("Inventory Manager")]
    [SerializeField] private InventoryManager _inventoryManager = null;


    [Header("Main Craft")]
    [SerializeField] private GameObject _craftPanel = null;

    [SerializeField] private List<BeginDragRes> _buttonCraftList = null;
    [SerializeField] private Button[] _buttonCraftRessources = null;
    [SerializeField] private TMP_Text[] _textCraftRessources = null;

    [SerializeField] private Canvas _mainCanvas = null;
    [SerializeField] private UICraftRessourceInteract _craftInteract = null;



    private bool _craftOpen = false;

    [Header("Main Inventory")]
    [SerializeField] private GameObject _inventoryPanel = null;
    private bool _inventoryOpen = false;

    [Header("Main Contacts")]
    [SerializeField] private GameObject _contactsPanel = null;
    private bool _contactsOpen = false;

    [Header("Main Inventory Ressources")]
    //RESSOURCES
    [SerializeField] private Button[] _buttonRessources = null;
    private List<int> _ressourceNumberTempList = null;
    [SerializeField] private int _numberOfRessourcesMaxInventory = 6;


    [Header("Main Inventory Objects")]
    //OBJECTs
    [SerializeField] private Button[] _buttonObjects = null;
    private List<int> _objectNumberTempList = null;
    [SerializeField] private int _numberOfObjectsMaxInventory = 3;

    [Header("Main Book")]
    [SerializeField] private GameObject _contactPanel = null;
    [SerializeField] private GameObject _decouvertePanel = null;
    [SerializeField] private GameObject _recettePanel = null;
    private int _contactNumberPageMax = 2;
    private int _contactCurrentPage = 0;
    [SerializeField] private GameObject[] _contactPageObjects = null;
    [SerializeField] private GameObject[] _recettesPageObjects = null;
    [SerializeField] private GameObject[] _decouvertePageObjects = null;




    [Header("Cameras")]
    [SerializeField] private Camera _mainCamera = null;
    [SerializeField] private Camera _inventoryCamera = null;
    [SerializeField] private Transform _craftPosition = null;
    [SerializeField] private Transform _contactsPosition = null;
    [SerializeField] private Transform _inventoryPosition = null;
    [SerializeField, LabelOverride("No transition (debug)")] private bool _noTransition = false;
    private bool _ignoreInputs = false;

    [Header("Others")]
    [SerializeField] private GameObject _uIVFXContaineer = null;
    [SerializeField] private GameObject _algueMur = null;
    #endregion Main


    #region Dialogues
    [Header("Dialogues")]
    [SerializeField] private NPCPositionMemory _npcPositionMemory = null;
    [SerializeField] private Yarn.Unity.DialogueRunner _dialogueRunner = null;

    [SerializeField] private RawImage _dialogueBox = null;
    [SerializeField] private TMP_Text _speakerNameText = null;
    [SerializeField] private Image _speakerColor = null;
    [SerializeField] private TMP_Text _uiInteractText = null;

    #endregion Dialogues


    #endregion Fields


    #region Properties
    public Yarn.Unity.DialogueRunner DialogueRunner => _dialogueRunner;
    public InventoryManager InventoryManager => _inventoryManager;
    public GameObject UIVFXContaineer => _uIVFXContaineer;
    public bool InventoryOpen => _inventoryOpen;
    public bool ContactsOpen => _contactsOpen;
    public bool CraftOpen => _craftOpen;
    public Canvas MainCanvas => _mainCanvas;
    public UICraftRessourceInteract CraftInteract => _craftInteract;


    public List<BeginDragRes> ButtonCraftList
    {
        get
        {
            return _buttonCraftList;
        }
        set
        {
            _buttonCraftList = value;
        }
    }

    public TMP_Text UIInteractText
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

    public CharacterMovement CharacterMovement
    {
        get
        {
            return _characterMovement;
        }
    }
    public NPCPositionMemory NPCPositionMemory => _npcPositionMemory;
    public RawImage DialogueBox => _dialogueBox;
    public TMP_Text SpeakerNameText
    {
        get
        {
            return _speakerNameText;
        }
        set
        {
            _speakerNameText = value;
        }
    }
    public Image SpeakerColor => _speakerColor;


    #endregion Properties


    #region Methods
    #region Awake, Start & Update
    void Awake()
    {
        UIManager.Instance.UIController = this;
    }

    void Start()
    {
        _ressourceNumberTempList = new List<int>();
        for (int i = 0; i <= _numberOfRessourcesMaxInventory - 1; i++)
        {
            _ressourceNumberTempList.Add(0);
        }

        _objectNumberTempList = new List<int>();
        for (int i = 0; i <= _numberOfObjectsMaxInventory - 1; i++)
        {
            _objectNumberTempList.Add(0);
        }

        _buttonCraftList = new List<BeginDragRes>();

        for (int i = 0; i <= _buttonCraftRessources.Length - 1; i++)
        {
            _buttonCraftList.Add(_buttonCraftRessources[i].GetComponent<BeginDragRes>());
        }
    }
    void Update()
    {
       
        if (DialogueManager.Instance.IsInTreeInteraction)
        {
            if (InputManager.GetAxis("Horizontal") != 0 || InputManager.GetAxis("Vertical") != 0 || InputManager.GetButtonDown("InventoryInterface") || InputManager.GetButtonDown("CraftInterface"))
            {
                DialogueManager.Instance.HideTreeInteraction();
                DialogueManager.Instance.IsInGiveSituation = false;
                DialogueManager.Instance.transform.FindDeep("CanvasDialogue").GetComponent<CanvasGroup>().blocksRaycasts = true;
            }
        }
        if (!_ignoreInputs)
        {
            if (InputManager.GetButtonDown("InventoryInterface"))
            {
                if (DialogueManager.Instance.IsInDialog == false)
                {
                    _ignoreInputs = true;
                    if (_inventoryOpen == false && (_craftOpen == true || _contactsOpen == true))
                    {
                        InventoryTransitionInventory();
                    }
                    else if (_inventoryOpen == false)
                    {
                        CharacterMovementDisable();
                        Transition.StartTransition(OpenInventory);

                    }
                    else
                    {
                        Transition.StartTransition(new Transition.TransitionCallback[] { CloseInventory, CharacterMovementEnable });
                    }
                }
            }
            else if (InputManager.GetButtonDown("CraftInterface"))
            {
                if (DialogueManager.Instance.IsInDialog == false)
                {
                    _ignoreInputs = true;
                    if (_craftOpen == false && (_inventoryOpen == true || _contactsOpen == true))
                    {
                        InventoryTransitionCraft();
                    }
                    else if (_craftOpen == false)
                    {
                        CharacterMovementDisable();

                        Transition.StartTransition(OpenCraft);

                    }
                    else
                    {

                        Transition.StartTransition(new Transition.TransitionCallback[] { CloseCraft, CharacterMovementEnable });

                    }
                }
            }
            else if (InputManager.GetButtonDown("SwitchInterface"))
            {
                if (DialogueManager.Instance.IsInDialog == false && _contactsOpen == false)
                {
                    if (_contactsOpen == false && _craftOpen == true)
                    {
                        _ignoreInputs = true;
                        InventoryTransitionContacts();
                    }
                    else if (_craftOpen == false && _inventoryOpen == true)
                    {
                        _ignoreInputs = true;
                        InventoryTransitionCraft();
                    }
                }
                else if (_contactsOpen == true && _inventoryOpen == false)
                {
                    _ignoreInputs = true;
                    InventoryTransitionInventory();
                }
            }
        }
    }
    #endregion Awake, Start & Update

    #region Craft & Inventory
    public void OpenInventory()
    {
        UpdateInventory();

        _inventoryPanel.SetActive(true);
        _inventoryOpen = true;

        _ignoreInputs = false;
    }
    public void CloseInventory()
    {
        _inventoryPanel.SetActive(false);
        _inventoryOpen = false;
    }

    public void CloseEverything()
    {
        _ignoreInputs = false;
        if (_inventoryOpen)
        {
            CloseInventory();
        }
        if (_craftOpen)
        {
            CloseCraft();
        }
        if (_contactsOpen)
        {
            CloseContacts();
        }
    }

    public void InventoryTransitionCraft()
    {
        _ignoreInputs = true;
        CloseEverything();
        UpdateCraft();
        CameraTools.MoveCamera(_inventoryCamera, _craftPosition.position, _craftPosition.rotation, 0.5f, OpenCraft);
    }

    public void InventoryTransitionContacts()
    {
        _ignoreInputs = true;
        CloseEverything();
        CameraTools.MoveCamera(_inventoryCamera, _contactsPosition.position, _contactsPosition.rotation, 0.5f, OpenContacts);
    }

    public void UpdateInventory()
    {

        CheckInventoryRessources();
        CheckInventoryObjects();

    }

    private void CheckInventoryRessources()
    {

        for (int i = 0; i <= _numberOfRessourcesMaxInventory - 1; i++)
        {
            switch (i)
            {
                case 0:
                    _ressourceNumberTempList[i] = _inventoryManager.Meduses;
                    break;
                case 1:
                    _ressourceNumberTempList[i] = _inventoryManager.Fruits;

                    break;
                case 2:
                    _ressourceNumberTempList[i] = _inventoryManager.Fleurs;

                    break;
                case 3:
                    _ressourceNumberTempList[i] = _inventoryManager.Algues;

                    break;
                case 4:
                    _ressourceNumberTempList[i] = _inventoryManager.Crevettes;

                    break;
                case 5:
                    _ressourceNumberTempList[i] = _inventoryManager.Poulpes;

                    break;
            }

            if (_ressourceNumberTempList[i] > 0)
            {
                TMP_Text numberText = _buttonRessources[i].GetComponentInChildren<TMP_Text>();
                numberText.text = _ressourceNumberTempList[i].ToString();

                _buttonRessources[i].gameObject.SetActive(true);
            }
            else
            {
                _buttonRessources[i].gameObject.SetActive(false);
            }

        }
    }
    private void CheckInventoryObjects()
    {

        for (int i = 0; i <= _numberOfObjectsMaxInventory - 1; i++)
        {
            switch (i)
            {
                case 0:
                    _objectNumberTempList[i] = _inventoryManager.MeduseCoussin;
                    break;
                case 1:
                    _objectNumberTempList[i] = _inventoryManager.MeduseGourmande;
                    break;
                case 2:
                    _objectNumberTempList[i] = _inventoryManager.Sushi;

                    break;
            }

            if (_objectNumberTempList[i] > 0)
            {
                TMP_Text numberText = _buttonObjects[i].GetComponentInChildren<TMP_Text>();
                numberText.text = _objectNumberTempList[i].ToString();

                _buttonObjects[i].gameObject.SetActive(true);
            }
            else
            {
                _buttonObjects[i].gameObject.SetActive(false);
            }

        }
    }


    private void OpenCraft()
    {
        UpdateCraft();

        _craftPanel.SetActive(true);
        _craftOpen = true;

        _ignoreInputs = false;
    }

    private void CloseCraft()
    {
        _craftPanel.SetActive(false);
        _craftOpen = false;
    }

    public void OpenContacts()
    {
        _contactsPanel.SetActive(true);
        _contactsOpen = true;
        _ignoreInputs = false;
    }

    private void CloseContacts()
    {
        _contactsPanel.SetActive(false);
        _contactsOpen = false;
    }

    private void InventoryTransitionInventory()
    {
        _ignoreInputs = true;
        CloseEverything();
        UpdateInventory();
        CameraTools.MoveCamera(_inventoryCamera, _inventoryPosition.position, _inventoryPosition.rotation, 0.5f, OpenInventory);
    }

    public void UpdateCraft()
    {
        CheckRessourceCraft();
    }

    private void CheckRessourceCraft()
    {
        for (int i = 0; i <= _numberOfRessourcesMaxInventory - 1; i++)
        {
            switch (i)
            {
                case 0:
                    _ressourceNumberTempList[i] = _inventoryManager.Meduses;
                    break;
                case 1:
                    _ressourceNumberTempList[i] = _inventoryManager.Fruits;

                    break;
                case 2:
                    _ressourceNumberTempList[i] = _inventoryManager.Fleurs;

                    break;
                case 3:
                    _ressourceNumberTempList[i] = _inventoryManager.Algues;

                    break;
                case 4:
                    _ressourceNumberTempList[i] = _inventoryManager.Crevettes;

                    break;
                case 5:
                    _ressourceNumberTempList[i] = _inventoryManager.Poulpes;

                    break;
            }

            if (_ressourceNumberTempList[i] > 0)
            {
                _buttonCraftList[i].IsOutOfRessource = false;
                _textCraftRessources[i].text = _ressourceNumberTempList[i].ToString();
            }
            else
            {
                _buttonCraftList[i].IsOutOfRessource = true;
                _textCraftRessources[i].text = _ressourceNumberTempList[i].ToString();
                //_buttonCraftRessources[i].image.color = Color.gray;
            }

        }
    }
    #endregion Craft & Inventory

    #region Book

    public void OpenContact()
    {
        _contactPanel.SetActive(true);
        _recettePanel.SetActive(false);
        _decouvertePanel.SetActive(false);
    }

    public void OpenRecette()
    {
        _contactPanel.SetActive(false);
        _recettePanel.SetActive(true);
        _decouvertePanel.SetActive(false);
    }

    public void OpenDecouverte()
    {
        _contactPanel.SetActive(false);
        _recettePanel.SetActive(false);
        _decouvertePanel.SetActive(true);
    }

    public void TurnPage(bool turningRight)
    {
        if (turningRight == false) //TOURNE VERS LA GAUCHE
        {
            if (_contactCurrentPage == 0)
            {
                //SUR LA PREMIERE PAGE DEJA MAXIMUM (NE FAIT RIEN)
            }
            else
            {
                _contactPageObjects[_contactCurrentPage].SetActive(false);

                _contactCurrentPage--;

                _contactPageObjects[_contactCurrentPage].SetActive(true);
            }
        }
        else  //TOURNE VERS LA DROITE
        {
            if (_contactCurrentPage == _contactNumberPageMax)
            {
                //SUR LA DERNIERE PAGE DEJA MAXIMUM (NE FAIT RIEN)

            }
            else
            {
                _contactPageObjects[_contactCurrentPage].SetActive(false);

                _contactCurrentPage++;

                _contactPageObjects[_contactCurrentPage].SetActive(true);
            }
        }
    }

    #endregion Book
    public void CharacterMovementToggle(bool canMove)
    {
        _characterMovement.SetCanMove(canMove);
    }

    public void CharacterMovementEnable()
    {
        _characterMovement.SetCanMove(true);
        _ignoreInputs = false;
    }

    public void CharacterMovementDisable()
    {
        _characterMovement.SetCanMove(false);
    }
    #endregion Methods
}
*/