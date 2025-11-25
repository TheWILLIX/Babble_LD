using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luminosity.IO;
using UnityEngine.EventSystems;
using ClemCAddons;
using System;
using System.Linq;
using ClemCAddons.Utilities;

public class BananeManager : MonoBehaviour
{
    //Les emplacements des 7 ressources sont déja attritré de manière fixe (Quand il n'y a plus de ressources méduse l'emplacement des méduse est remplacé par Item.quantity0)

    #region Fields
    [Header("Inputs")]
    [SerializeField] private InventoryControlMode _controlMode = InventoryControlMode.Cursor;
    [Header("Ressource")]
    [Tooltip("Place the Ressources Slot of the Banane (Where the Ressource are supposed to be)")]
    [SerializeField] private GameObject[] _ressourceEmplacement = null;
    private List<ItemInteraction> _ressourceInteractions = null;

    [Header("Objects")]
    [SerializeField] private Transform[] _pockets = null;
    private GameObject[] _objectEmplacement = new GameObject[] { };
    private List<ItemInteraction> _objectInteractions = null;
    private int _objectEmplacementStocked = 0;
    [SerializeField] private int _pageCount = 3;
    [SerializeField] private GameObject _pageObjectCategory = null;
    [SerializeField] private float _pageTurningSpeed = 1;
    private int _currentPage = 1;

    [Header("Drag & Drop")]
    [Tooltip("A prefab of the drag & drop item faker")]
    [SerializeField] private GameObject _itemFakerPrefab;
    [SerializeField] private GameObject _slotItemFakerPrefab;


    private static Item _itemSelected = null;
    private Item _firstItemInShaker = null;
    private Item _secondItemInShaker = null;

    private bool _isSelectionMode = false;

    private bool _lockExit = false;

    private GameObject cursorFaker;

    [Header("Inventory Sign & Feedback")]
    [SerializeField] private Sprite _shakerClosed = null;
    [SerializeField] private Sprite _shakerTop = null;
    [SerializeField] private Image _craftHint = null;
    private bool _isShakerOpened = false;

    [Header("Utilities Labels")]
    [SerializeField] private Label _useLabels = null;
    [SerializeField] private Label _craftLabels = null;
    [SerializeField] private Label _infoLabels = null;

    private bool _isCurrentlyDown = false;

    [Header("Tutorial")]

    private bool _inFleurTutoPart = false;
    private bool _inFleurUseAndPlaceTutoPart = false;
    
    private bool _inCraftTutoPart = false;
    private bool _inPoulpeCoussinUseAndPlaceTutoPart = false;


    private bool _playerGotInventoryExplanation = false;
    private bool _playerGotPoulpeCoussinRecipeQuizz = false;
    private bool _playerUnderstoodCraft = false;
    private bool _playerHadDialogueEasyBoyShaker = false;
    //    private bool _newRecipeTutoDone = false; //****

    [Header("For The Demo Oral")]
    [SerializeField] private ItemInteraction _meduseItemInteraction = null;     //For The Demo Oral
    [SerializeField] private Animator _brassartSlideFin = null;



    #endregion Fields

    #region Properties
    public static Item ItemSelected
    {
        get
        {
           return _itemSelected;
        }
        set
        {
            _itemSelected = value;
        }
    }

    public InventoryControlMode ControlMode { get => _controlMode; set => _controlMode = value; }

    public bool IsSelectionMode => _isSelectionMode;
    public Item FirstItemInShaker { get => _firstItemInShaker; }

    public Item SecondItemInShaker { get => _secondItemInShaker; }
    public bool LockExit { get => (_lockExit || _firstItemInShaker != null); set => _lockExit = value; }
    public bool InCraftTutoPart { get => _inCraftTutoPart; set => _inCraftTutoPart = value; }
    public bool InFleurTutoPart { get => _inFleurTutoPart; set => _inFleurTutoPart = value; }

    #endregion Properties

    public enum InventoryControlMode
    {
        Cursor,
        Slots,
        Rail
    }
 
    #region Methods
    #region General Logic
    #region Start
    private void Start()
    {
        try
        {
            _ressourceInteractions = new List<ItemInteraction>();
            _objectInteractions = new List<ItemInteraction>();
            
            foreach(Transform pocket in _pockets)
            {
                for(int i = 0; i < pocket.childCount; i++)
                {
                    if (pocket.GetChild(i).name.StartsWith("P"))
                        _objectEmplacement = _objectEmplacement.Add(pocket.GetChild(i).gameObject);
                }
            }

            foreach (GameObject emplacement in _ressourceEmplacement)
            {
              _ressourceInteractions.Add(emplacement.GetComponent<ItemInteraction>());
            }

            foreach (GameObject emplacement in _objectEmplacement)
            {
                _objectInteractions.Add(emplacement.GetComponent<ItemInteraction>());
            }


            GetComponent<Canvas>().enabled = true;  //This might feel useless but we need banane manager to be active to initialize itself
            gameObject.SetActive(false); //Once it's done we set it as inactive, this will be set active back when opening the banane
        }
        catch
        {
            Debug.LogError("Issue in BananeManager, there must be missing reference in RessourceEmplacement Field");
        }

    }
    #endregion Start
    #region Opening and Closing Banane
    public void OpeningBanane(bool selectionMode = false, string defaultSelection = "")
    {

        
            if (TutorialManager.Instance.TutorialDone == false && _inFleurTutoPart == true)
            {
                if(_playerGotInventoryExplanation == false)
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_Inventory", true);
                    TutorialManager.Instance.LoadTutoVideo(TutorialManager.EVideoTutoType.FLEURINVENTORY);
                    _playerGotInventoryExplanation = true;
                }
                else
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_Inventory_Short", true);
                }
                UIManager.Instance.UIController.IsInTutorialDialogue = true;

            }
            else if (TutorialManager.Instance.TutorialDone == false && _inCraftTutoPart == true)
            {
                if(_playerGotPoulpeCoussinRecipeQuizz == false)
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Intro", true);
                    TutorialManager.Instance.LoadTutoVideo(TutorialManager.EVideoTutoType.RESSOURCEINSHAKER);
                    _playerGotPoulpeCoussinRecipeQuizz = true;
                }
                else
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Intro_Short", true);
                }
                UIManager.Instance.UIController.IsInTutorialDialogue = true;
            }
        

        //Reset the Shaker Sprite
        transform.FindDeep("CraftSprite").GetComponent<Image>().sprite = _shakerClosed;

       // Debug.Log(selectionMode + " TAMER");
        _isSelectionMode = selectionMode;
        UpdateBanane();
        switch (_controlMode)
        {
            case InventoryControlMode.Cursor:
                BabblesCursor.Hidden = false;
                if(defaultSelection != "")
                {
                    BabblesCursor.Move(_ressourceInteractions.Find(t => t.RessourceType.Name == defaultSelection).transform.position);
                }
                break;
            case InventoryControlMode.Slots:
                if (defaultSelection == "")
                    _ressourceInteractions[0].GetComponent<Button>().Select();
                else
                    _ressourceInteractions.Find(t => t.RessourceType.Name == defaultSelection).GetComponent<Button>().Select();
                GenerateNavigation();
                break;
            case InventoryControlMode.Rail:
                if (defaultSelection == "")
                    _ressourceInteractions[0].GetComponent<Button>().Select();
                else
                    _ressourceInteractions.Find(t => t.RessourceType.Name == defaultSelection).GetComponent<Button>().Select(); GenerateNavigation();
                break;
            default:
                BabblesCursor.Hidden = false;
                break;
        }

        

    }

    public void SetSelection(string selection)
    {
        switch (_controlMode)
        {
            case InventoryControlMode.Cursor:
                BabblesCursor.Hidden = false;
                if (selection != "")
                {
                    BabblesCursor.Move(_ressourceInteractions.Find(t => t.RessourceType.Name == selection).transform.position);
                }
                break;
            case InventoryControlMode.Slots:
                if (selection == "")
                    _ressourceInteractions[0].GetComponent<Button>().Select();
                else
                    _ressourceInteractions.Find(t => t.RessourceType.Name == selection).GetComponent<Button>().Select();
                GenerateNavigation();
                break;
            case InventoryControlMode.Rail:
                if (selection == "")
                    _ressourceInteractions[0].GetComponent<Button>().Select();
                else
                    _ressourceInteractions.Find(t => t.RessourceType.Name == selection).GetComponent<Button>().Select(); GenerateNavigation();
                break;
            default:
                BabblesCursor.Hidden = false;
                break;
        }
    }

    public void ClosingBanane()
    {
        //Audio :
        AudioManager.Start2DSound("S_FermetureInventaire");

        if (IsSelectionMode)
        {
            UIManager.Instance.UIController.IsInGiveSituation = false;
            Debug.LogError("Need to add reaction to closing selection without giving");
            //UIManager.Instance.UIController.ExitInteractionChoice();
        }
        BabblesCursor.Hidden = true;
        if (_itemSelected != null)
        {
            InventoryManager.Instance.AddItem(_itemSelected);
            if (slotFaker != null)
                Destroy(slotFaker.gameObject);
            if(cursorFaker != null)
                Destroy(cursorFaker.gameObject);
            _itemSelected = null;
        }
        transform.FindDeep("Craft").GetComponent<Shaker>().Reset();
        HideResourcesCraft();
        if (_firstItemInShaker != null)
        {
            InventoryManager.Instance.AddItem(_firstItemInShaker);
            _firstItemInShaker = null;
        }
        if (_secondItemInShaker != null)
        {
            InventoryManager.Instance.AddItem(_secondItemInShaker);
            _secondItemInShaker = null;
        }

        ShowLabels(false, true); //Unshow Labels
        _useLabels.ResetLabel();

    }

   
    #endregion Opening and Closing Banane
    #region Updates
    void Update()
    {
        UpdateObjectPage();
    }
    public void UpdateBanane()
    {
        _ = InventoryManager.Instance; // make sure instance value is set, as findobjectoftype can't be called in non-main thread
        GameTools.RunInThread(true,
        () =>
        {
            // foreach is way more optimized than people think, in cases like this where you'd get it many times,
            // it's worth having it declared as a variable, so it's equivalent to doing a for and fetch it many time
            _objectEmplacementStocked = 0; // reset the count
            foreach (Item item in InventoryManager.Instance.ItemDB)
            {
                UpdateItemLocal(item);
            }

            // objects => from objectInteraction, set sprite by quantity, also reorder
            ClearEmptyObjects();
            AddObjects();
            SortObjects();
        });
    }

    public void RedrawNumbers()
    {
        foreach (Item item in InventoryManager.Instance.ItemDB)
        {
            if (item.IsRessource)
            {
                var r = _ressourceInteractions.Find(t => t.RessourceType == item);
                if(r != null)
                    r.UpdateRessourceNumber();
            }
            else
            {
                var r = _objectInteractions.Find(t => t.RessourceType == item);
                if(r != null)
                    r.UpdateRessourceNumber();
            }
        }
    }

    [Obsolete("Use BananeUpdate instead")]
    public void UpdateItem(Item item)
    {
        UpdateItemLocal(item);
    }
    private protected void ClearEmptyObjects()
    {
        foreach(ItemInteraction itemInteraction in _objectInteractions)
        {
            if (!itemInteraction.IsAssigned)
                continue;
            if(InventoryManager.Instance.GetItemData(itemInteraction.RessourceType) == 0)
            {
                itemInteraction.UnassignItem();
            }
        }
    }
    private protected void AddObjects()
    {
        foreach(Item item in InventoryManager.Instance.ItemDB)
        {
            if (item.IsRessource)
                continue;
            if (InventoryManager.Instance.GetItemData(item) == 0)
                continue;
            var r = _objectInteractions.Find(t => t.RessourceType == item);
            if(r == null) // add an item to the first foudn empty slot if none can be found
            {
                r = _objectInteractions.Find(t => t.RessourceType == null);
                r.AssignItem(item);
            }
        }
    }
    private protected void SortObjects()
    {
        Item[] r = _objectInteractions.FindAll(t => t.RessourceType != null).Select(t => t.RessourceType).ToArray();
        for(int i = 0; i < _objectInteractions.Count; i++)
        {
            if(i < r.Length)
            {
                _objectInteractions[i].AssignItem(r[i]);
                var value = InventoryManager.Instance.GetItemData(r[i]);
                var id = i; // by the time the main thread executes it, the loop finished
                GameTools.RunInMainThread(() =>
                {
                    switch (value)
                    {
                        case 0:
                            _objectInteractions[id].GetComponent<Image>().sprite = r[id].SpriteQuantity0;
                            break;
                        case 1:
                            _objectInteractions[id].GetComponent<Image>().sprite = r[id].Sprite;
                            break;
                        case 2:
                            _objectInteractions[id].GetComponent<Image>().sprite = r[id].SpriteQuantity2;
                            break;
                        case 3:
                            _objectInteractions[id].GetComponent<Image>().sprite = r[id].SpriteQuantity3;
                            break;
                        default:
                            _objectInteractions[id].GetComponent<Image>().sprite = r[id].SpriteQuantity3;
                            break;
                    }
                });
                _objectInteractions[i].UpdateRessourceNumber();
            }
            else
            {
                _objectInteractions[i].UnassignItem();
                var id = i; // by the time the main thread executes it, the loop finished
                GameTools.RunInMainThread(() => {
                    _objectInteractions[id].GetComponent<Image>().sprite = null;
                });
                _objectInteractions[i].UpdateRessourceNumber(true);
            }
        }
    }
    private protected void UpdateItemLocal(Item item)
    {
        // discovered ressources => from ressourceInteraction, set sprite by quantity
        if (item.IsRessource == true)     // FOR THE RESSOURCES
        {
            if (InventoryManager.Instance.isItemDiscovered(item) == true)
            {
                if (_ressourceInteractions == null)
                {
                    Debug.LogError("_ressourceInteractions is null, Start did not run yet, the BananeManager prefab might be misconfigured");
                    return;
                }
                var r = _ressourceInteractions.Find(t => t.RessourceType == item);
                if (r == null)
                {
                    Debug.LogError("No ressource interaction value matches the "+item.Name+" item");
                    return;
                }
                switch (InventoryManager.Instance.GetItemData(item))
                {
                    case 0:
                        r.UpdateRessourceSprite(r.RessourceType.SpriteQuantity0); 
                        break;

                    case 1:
                        r.UpdateRessourceSprite(r.RessourceType.SpriteQuantity1);
                        break;

                    case 2:
                        r.UpdateRessourceSprite(r.RessourceType.SpriteQuantity2);
                        break;

                    case 3:
                        r.UpdateRessourceSprite(r.RessourceType.SpriteQuantity3);
                        break;

                    default:
                        r.UpdateRessourceSprite(r.RessourceType.SpriteQuantity3);
                        break;
                }
                //Update Ressource Number

                r.UpdateRessourceNumber();
            }
        }
    }

    public void CheckAllItems()
    {
        foreach (Item item in InventoryManager.Instance.ItemDB)
        {
            Debug.Log(item.Name + " : " + InventoryManager.Instance.GetItemData(item));
        }

        UpdateBanane();
    }
    #endregion Updates
    #region Item Usage
    private bool UseItem(Item item)
    {
    

            if(TutorialManager.Instance.TutorialDone == false && _inFleurUseAndPlaceTutoPart == true)
            {
                if (item.Name == "Fleur")
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_FleurBrulante_Active", true);
                    TutorialManager.Instance.HideTutoVideo(); //AAAHAHAHAHAHAHAJHBFSJKBDJKSD?FBSE
                    _inFleurUseAndPlaceTutoPart = false;
                }
            }
            else if (TutorialManager.Instance.TutorialDone == false && _inPoulpeCoussinUseAndPlaceTutoPart == true)
            {
                if (item.Name == "PoulpeCoussin")
                {
                    UIManager.Instance.UIController.StartDialogue("Tuto_Poser_Meduse", true);
                    _inPoulpeCoussinUseAndPlaceTutoPart = false;
                }
            }
        

        var r = InventoryManager.Instance.TryUseItem(item);
        if(r)
            UpdateBanane();
        return r;
    }
    #endregion Item Usage
    #region Craft
    public void PlaceRessource()
    {
     
        if(_firstItemInShaker == null)
        {
            _firstItemInShaker = _itemSelected;
        }
        else
        {
            _secondItemInShaker = _itemSelected;
        }   

        _itemSelected = null;

    }

    public bool PlaceRessource(Item ressource)
    {

        if (_firstItemInShaker == null)
        {
            _firstItemInShaker = ressource;
            transform.FindDeep("CraftMask1").FindDeep(t => t.name.ToLower() == ressource.Name.ToLower()).gameObject.SetActive(true);
            BabblesVibration.CustomVibration(0.2f, 2f); //Vibration
                                                        //if(_debugDesactivateTutorial == false && _tuto)
           

                
                if (TutorialManager.Instance.TutorialDone == false && _inFleurTutoPart == true)
                {
                    if (_firstItemInShaker.Name == "Fleur")
                    {
                        Debug.Log("Futur Dialogue Tutoriel");
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;

                    }
                }
                else if (TutorialManager.Instance.TutorialDone == false && (_playerUnderstoodCraft == false && _inCraftTutoPart == false))
                {
                    if(_playerHadDialogueEasyBoyShaker == false)
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_ShakerUsedTooSoon", true);
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;
                    }
                }
                else if (TutorialManager.Instance.TutorialDone == false && _inCraftTutoPart == true)
                {
                    if(_firstItemInShaker.Name == "Poulpe")
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_FirstItem_Poulpe2", true);
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;

                    }
                    else if(_firstItemInShaker.Name == "Fruit")
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_FirstItem_Fruit", true);
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;

                    }
                    else
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Item_Bad", true);
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;
                    }
                }

            
        }
        else if (_secondItemInShaker == null)
        {
            _secondItemInShaker = ressource;
            transform.FindDeep("CraftMask2").FindDeep(t => t.name.ToLower() == ressource.Name.ToLower()).gameObject.SetActive(true);
            BabblesVibration.CustomVibration(0.2f, 2f); //Vibration

          
                if (TutorialManager.Instance.TutorialDone == false && _inCraftTutoPart == true)
                {
                    if(_secondItemInShaker.Name == "Fruit" || _secondItemInShaker.Name == "Poulpe")
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Item_Good", true);
                        TutorialManager.Instance.HideTutoVideo();
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;
                    }
                    else
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Item_Bad", true);
                        UIManager.Instance.UIController.IsInTutorialDialogue = true;
                    }
                }
            

                    
        }
        else
        {
            Debug.LogWarning("Shaker already full");
            AudioManager.Start2DSound("S_ShakerRempli");

            _itemSelected = null;
            return false;
        }
        _itemSelected = null;
        return true;
    }

    public void HideResourcesCraft()
    {
        AudioManager.Start2DSound("S_AnnulerSelectionObjet"); //AUDIO

        if (_firstItemInShaker != null)
            transform.FindDeep("CraftMask1").FindDeep(t => t.name.ToLower() == _firstItemInShaker.Name.ToLower()).gameObject.SetActive(false);
        if (_secondItemInShaker != null)
            transform.FindDeep("CraftMask2").FindDeep(t => t.name.ToLower() == _secondItemInShaker.Name.ToLower()).gameObject.SetActive(false);

    }

    public bool CanCraft()
    {
        return _firstItemInShaker != null && _secondItemInShaker != null;
    }

    public bool CraftEmpty()
    {
        return _firstItemInShaker == null && _secondItemInShaker == null;
    }
    public bool CraftValid()
    {
        Debug.Log("Checking if craft is valid, ressource 1 = " + _firstItemInShaker + "  ; ressource2 = " + _secondItemInShaker);

        var recipe = InventoryManager.Instance.RecipeDB.ToList().Find(t =>
        (t.FirstItem == _firstItemInShaker && t.SecondItem == _secondItemInShaker)
        || (t.FirstItem == _secondItemInShaker && t.SecondItem == _firstItemInShaker));

        if (recipe != null)
        {
            if (InventoryManager.Instance.RecipeLocked[recipe.name] == true)
            {
                Debug.Log("Craft locked");
                return false;
            }
            else
            {
                Debug.Log("Craft valid");

                
                    if (TutorialManager.Instance.TutorialDone == false && _inCraftTutoPart == true)
                    {
                        UIManager.Instance.UIController.StartDialogue("Tuto_Craft_Fin", true);
                        TutorialManager.Instance.LoadTutoVideo(TutorialManager.EVideoTutoType.USEPOULPECOUSSIN);
                        _inCraftTutoPart = false;
                        _playerUnderstoodCraft = true;
                        _inPoulpeCoussinUseAndPlaceTutoPart = true;


                        UIManager.Instance.UIController.IsInTutorialDialogue = true;
                    }
                
                return true;
            }
        }
        Debug.Log("Craft invalid");
        return false;
    }
    public ItemInteraction GetCraftedSlot()
    {
        var result = GetCraftResult();
        var count = InventoryManager.Instance.GetItemData(result);
        if(count == 0)
        {
            foreach (var slot in _objectInteractions)
            {
                if (slot.IsAssigned == false)
                    return slot;
            }
        }
        foreach(var slot in _objectInteractions)
        {
            if (slot.RessourceType == result)
                return slot;
        }
        return null;
    }
    public Item GetCraftResult()
    {
        var recipe = InventoryManager.Instance.RecipeDB.ToList().Find(t =>
        (t.FirstItem == _firstItemInShaker && t.SecondItem == _secondItemInShaker)
        || (t.FirstItem == _secondItemInShaker && t.SecondItem == _firstItemInShaker));

        return recipe.ResultItem;
    }
    public void CraftCheck()
    {

        Debug.Log("Checking if craft is valid, ressource 1 = " + _firstItemInShaker + "  ; ressource2 = " + _secondItemInShaker);

        var recipe = InventoryManager.Instance.RecipeDB.ToList().Find(t =>
        (t.FirstItem == _firstItemInShaker && t.SecondItem == _secondItemInShaker)
        || (t.FirstItem == _secondItemInShaker && t.SecondItem == _firstItemInShaker));



        if (recipe != null)
        {

        /*    if (recipe.RecipeName == "REC_BrassartSlide1" || recipe.RecipeName == "REC_BrassartSlide2") //Pour la présentation de fin d'année
            {
                _brassartSlideFin.gameObject.SetActive(true);
                _brassartSlideFin.SetTrigger("BrassartSlideOn");
                return;
            }*/


            if (InventoryManager.Instance.RecipeLocked[recipe.name] == true)
            {
                Debug.LogWarning("The Recipe has not been unlocked, nothing has been setup yet when it happens");
                UIManager.Instance.UIController.StartDialogue("Tuto_CraftLock", true);
                CancelCraft();
                return;
            }
            else
            {
                CraftLogic(recipe);
                return;
            }
        }
        else
        {
            CancelCraft();
            Debug.LogWarning("Recipe could not be found in the Recipe Database, make sure to correctly setup all Recipe in the Inventory Manager");
        }
    }

    private void CraftLogic(Recipe recipe)
    {

        _firstItemInShaker = null;
        _secondItemInShaker = null;
        InventoryManager.Instance.AddItem(recipe.ResultItem, 1);
        Debug.Log("Craft complete, the item " + recipe.ResultItem + "has been added to the banane");
        InventoryManager.Instance.QuickUseItems[3] = recipe.ResultItem;
      
        UpdateBanane();

        if (ControlMode == InventoryControlMode.Slots)
        {
            _ressourceInteractions[0].GetComponent<Button>().Select();
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(10, () =>
            {
                GenerateNavigation();
            });
            Debug.Log("navigation generated");
        }

        if (InventoryManager.Instance.isItemDiscovered(recipe.ResultItem) == false) //If it's the first time we craft this object then we will have to assign it in a banane slot
        {
            InventoryManager.Instance.SetItemDiscovered(recipe.ResultItem, true); //and set the Item as discovered
                 
            Debug.Log("New Crafted Object discovered, " + recipe.ResultItem.Name + " is now part of the discovered objects");

            return;
        }

        return;
    }

    public void CancelCraft()
    {
        HideResourcesCraft();
        if (_firstItemInShaker != null)
            BringBackFromCraft(_firstItemInShaker);
        if(_secondItemInShaker != null)
            BringBackFromCraft(_secondItemInShaker);
        _firstItemInShaker = null;
        _secondItemInShaker = null;
        BabblesVibration.CustomVibration(0.2f, 0.75f); //Vibration
        CloseShakerFeedback(true);

        AudioManager.Start2DSound("S_AnnulerSelectionObjet"); //AUDIO
    }
    private void BringBackFromCraft(Item item)
    {
        var faker = Instantiate(_slotItemFakerPrefab, transform).transform;
        faker.GetComponentInChildren<Image>().sprite = item.Sprite;
        faker.localScale = Vector3.one * 3;
        faker.position = transform.FindDeep("Craft").position;
        var r = _ressourceInteractions.Find(t => t.RessourceType == item);
        if(r == null)
        {
            Debug.LogError("Missing " + item + " slot in resource inventory");
        }
        Lerper.ConstantLerp(faker.localScale, Vector3.one, 0.5f, (t) => faker.localScale = t);
        Lerper.ConstantLerp(faker.position, r.transform.position, 0.5f, (t) => faker.position = t,
        () => {
            _ = GameTools.DelayedCall(100, () =>
            {
                if (_controlMode == InventoryControlMode.Slots)
                {
                    _ressourceInteractions[0].GetComponent<Button>().Select();
                }
                InventoryManager.Instance.AddItem(item);
                Destroy(faker.gameObject);
                UpdateBanane();
                GenerateNavigation();

            });
        });
    }
    #endregion Craft
    #region Pages
    private Quaternion currentRotation = Quaternion.Euler(0, 0, -33.8f);
    private Quaternion currentObjective = Quaternion.Euler(0, 0, -33.8f);

    public void ChangeObjectPage(int change, Button target)
    {

        if (!currentRotation.IsApproximatelyEqual(currentObjective, 0.001f))
        {
            return;
        }
        _currentPage += change;
        currentObjective = Quaternion.identity * Quaternion.Euler(0, 0, 33.8f * (_currentPage - Mathf.Ceil(_pageCount / 2f)));
        if (target == null)
        {
            Lerper.ConstantLerp(ref currentRotation, currentObjective, _pageTurningSpeed);
            return;
        }
        var nav = target.navigation;
        target.navigation = new Navigation() { mode = Navigation.Mode.None };
        Lerper.ConstantLerp(ref currentRotation, currentObjective, _pageTurningSpeed, () => { SetNavigation(target, nav); });

    }

    public void ChangeObjectPage(int change)
    {
        ChangeObjectPage(change, null);
    }

    private void SetNavigation(Button target, Navigation navigation)
    {
        target.navigation = navigation;
    }

    private void UpdateObjectPage()
    {
        _pageObjectCategory.transform.rotation = currentRotation;
        if (currentRotation.IsApproximatelyEqual(currentObjective, 0.0001f).OnceIfTrueGate("PageChange".GetHashCode()))
        {
            GenerateNavigation();
        }
    }
    #endregion Pages
    #region Notebook
    public void ShowNotebook()
    {
        _lockExit = true;
        transform.parent.GetComponentInChildren<Paging>().Show(this);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void ShowNotebookByCard(string card)
    {
        _lockExit = true;
        transform.parent.GetComponentInChildren<Paging>().Show(card, this);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void ShowNotebook(Item item)
    {
        _lockExit = true;
        transform.parent.GetComponentInChildren<Paging>().Show(item, this);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowNotebookByCategory(string category)
    {
        _lockExit = true;
        transform.parent.GetComponentInChildren<Paging>().ShowCategory(category, this);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void HideNotebook()
    {
        _ = GameTools.DelayedCall(50, () =>
        {
            _lockExit = false;
        });
        transform.parent.GetComponentInChildren<Paging>().Hide();
        transform.FindDeep("CarnetSprite").GetComponent<Button>().Select();
    }
    #endregion Notebook
    #endregion General Logic
    #region Modes Logic
    #region Cursor
    #region Drag & Drop
    #region Selection



    public void SelectRessource(Item selection, Vector2 size, Quaternion rotation)
    {
        if (InventoryManager.Instance.GetItemData(selection) >= 1)
        {
      
            if (selection)
            CreateItemFaker(selection, size, rotation);
            InventoryManager.Instance.RemoveItem(selection);
            _itemSelected = selection;
            if (selection.IsRessource)
                UpdateBanane();
            else
                RedrawNumbers();
        }
        else
        {
            Debug.Log("No " + selection.Name + " ressource available");
        }
        //Ressource on the cursor
    }

    public void UnSelectRessource(Item selection)
    {
        InventoryManager.Instance.AddItem(selection);
        _itemSelected = null;
        UpdateBanane();
        //Ressource on the cursor
    }
    #endregion Selection
    #region Item Faker
    private void CreateItemFaker(Item ressource, Vector2 size, Quaternion rotation)
    {
      
        var item = Instantiate(_itemFakerPrefab, transform).GetComponent<RectTransform>();

        item.GetComponentInChildren<Image>().sprite = ressource.Sprite;
        item.sizeDelta = size;
        item.rotation = rotation;
        cursorFaker = item.gameObject;
        _lockExit = true;
        StartCoroutine(MoveItemFaker(item, ressource));
    }

    private IEnumerator MoveItemFaker(RectTransform item, Item ressource)
    {
        while (InputManager.GetMouseButton(0)) // move item every frame to mouse position while mouse button is down
        {
            item.position = BabblesCursor.GetPosition();
            yield return new WaitForEndOfFrame();
        }
        ReleaseItemFaker(item, ressource);
    }


    private void ReleaseItemFaker(RectTransform item, Item ressource)
    {
        _ = GameTools.DelayedCall(50, () => { _lockExit = false; });
        cursorFaker = null;
        var raycaster = GetComponent<GraphicRaycaster>();
        List<RaycastResult> r = new List<RaycastResult>();
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition.Clamp(Vector3.zero, new Vector3(Screen.width, Screen.height));
        raycaster.Raycast(pointer, r);
        var noHitRun = true;
        foreach (var underMouse in r)
        {
            if(underMouse.gameObject.TryGetComponent<DropArea>(out var area))
            {
                if (!_isSelectionMode)
                {
                    switch (area.AreaType)
                    {
                        case DropArea.DropAreaType.Use:
                            if (!UseItem(ressource))
                                UnSelectRessource(ressource);
                            // refund if ressource usage is invalid, and refund
                            break;
                        case DropArea.DropAreaType.Shaker:
                            if (!ressource.IsRessource || !PlaceRessource(ressource))
                                UnSelectRessource(ressource);
                            // if shaker is full, reset
                            break;
                        case DropArea.DropAreaType.Notebook:
                            UnSelectRessource(ressource);
                            //
                            // to implement & insert => Open notebook at page
                            // (keep the unselect ressource)
                            //
                            break;
                        default:
                            UnSelectRessource(ressource);
                            break;
                    }
                    noHitRun = false;
                }
                else
                {
                    if (area.AreaType == DropArea.DropAreaType.Use || area.AreaType == DropArea.DropAreaType.Cancel)
                    {
                        DialogueManager.Instance.CheckPNJPreference(ressource);
                        _isSelectionMode = false;
                        UIManager.Instance.UIController.CloseInventory(true);
                        noHitRun = false;
                    }
                }
            }
        }
        if (noHitRun)
        {
            if(!IsSelectionMode)
                UnSelectRessource(ressource);
            else
            {
                DialogueManager.Instance.CheckPNJPreference(ressource);
                UIManager.Instance.UIController.CloseInventory(true);
            }
        }
        Destroy(item.gameObject);
    }
    #endregion Item Faker
    #endregion Drag & Drop
    #endregion Cursor
    #region Slots
    #region Selection
    #region Slot Management
    public void SlotSelected(Button slot, Vector2 size, Quaternion rotation, Item item)
    {
        if (InventoryManager.Instance.GetItemData(item) == 0)
            return;
        if (slotFaker != null)
        {
            slotFaker.GetComponentInChildren<FakerFloat>().Hide(); // prevention
        }
        slotFaker = CreateNeutralItemFaker(item, size, rotation);
        slotFaker.position = slot.transform.position;
    }
    public void SlotDeselected()
    {
        if (slotFaker != null)
        {
            slotFaker.GetComponentInChildren<FakerFloat>().Hide();
            slotFaker = null;
        }
    }
    public void SubmittedItemMove(Vector2 direction)
    {
        if ((direction.x == 0 && direction.y == 0) || !CraftEmpty())
            return;
        if (direction.y > 0) // moving up
        {
            if (isSelf) // if it's objects, they won't move to the craft, and if it's resources, the craft is on the right, not up
            {
                Highlights.ShowHighlights("Use");
                _useLabels.SetActive(true, _isSelectionMode);
                isUsing = true;
                isSelf = false;
                fromWhich = Areas.Self;

                CursorOnUseFeedback();
            }
            if (isUsing)
                return;
            if (isCrafting)
            {
                Highlights.ShowHighlights("Use");
                _useLabels.SetActive(true, _isSelectionMode);
                isUsing = true;
                isCrafting = false;
                fromWhich = Areas.Craft;
                CloseShakerFeedback(true);

                CursorOnUseFeedback();
            }
            if (isNotebook)
            {
                Highlights.ShowHighlights("Use");
                _useLabels.SetActive(true, _isSelectionMode);
                isUsing = true;
                isNotebook = false;
                fromWhich = Areas.Notebook;
                DeselectNotebookFeedback();

                CursorOnUseFeedback();
            }
        }
        else if (direction.y < 0) // moving down
        {
            if (isUsing)
            {
                switch (fromWhich)
                {
                    case Areas.Self:
                        Highlights.ShowHighlights("Item");
                        _useLabels.SetActive(false, _isSelectionMode);
                        isSelf = true;
                        isUsing = false;
                        break;
                    case Areas.Craft:
                        Highlights.ShowHighlights("Craft");
                        _craftLabels.SetActive(true);
                        isCrafting = true;
                        isUsing = false;
                        OpenShakerFeedback(true);
                        CursurOutstideUseFeedback();
                        break;
                    case Areas.Notebook:
                        Highlights.ShowHighlights("Notebook");
                        _infoLabels.SetActive(true);
                        isNotebook = true;
                        isUsing = false;
                        SelectNotebookFeedback();
                        break;
                    default:
                        break;
                }
            } 
            else if (isCrafting || isNotebook)
            {
                if (!isRessource)
                {
                    if (isCrafting)
                    {
                        Highlights.ShowHighlights("Item");
                        _infoLabels.SetActive(false);
                        isSelf = true;
                        isCrafting = false;
                        CloseShakerFeedback(true);
                    }
                    else
                    {
                        Highlights.ShowHighlights("Item");
                        _infoLabels.SetActive(false);
                        isSelf = true;
                        isNotebook = false;
                        DeselectNotebookFeedback();
                    }
                }
            }
        }
        else if (direction.x > 0) // moving right
        {
            if (isSelf && isRessource && !IsSelectionMode)
            {
                Highlights.ShowHighlights("Craft");
                _craftLabels.SetActive(true);
                isCrafting = true;
                isSelf = false;
                OpenShakerFeedback(true);
                CursurOutstideUseFeedback();
            }
            else if (isCrafting)
            {
                Highlights.ShowHighlights("Notebook");
                _infoLabels.SetActive(true);
                isCrafting = false;
                isNotebook = true;
                CloseShakerFeedback(true);
                SelectNotebookFeedback();
            } else if(isSelf && !isRessource)
            {
                Highlights.ShowHighlights("Notebook");
                _infoLabels.SetActive(true);
                isSelf = false;
                isNotebook = true;
                SelectNotebookFeedback();

            }
        }
        else if (direction.x < 0) // moving left
        {
            if (isCrafting)
            {
                Highlights.ShowHighlights("Item");
                _infoLabels.SetActive(false);
                isSelf = true;
                isCrafting = false;
                CloseShakerFeedback(true);
            }
            else if (isNotebook && isRessource)
            {
                Highlights.ShowHighlights("Craft");
                _craftLabels.SetActive(true);
                isCrafting = true;
                isNotebook = false;
                OpenShakerFeedback(true);
                DeselectNotebookFeedback();
                CursurOutstideUseFeedback();

            }
            else if (isNotebook)
            {
                Highlights.ShowHighlights("Item");
                _craftLabels.SetActive(false);
                isSelf = true;
                isNotebook = false;
                DeselectNotebookFeedback();

            }
        }
    }
    private bool lockSlot = false;
    public void ItemSubmitted(Item item, Vector2 size, Quaternion rotation, Button slot)
    {
        if (_controlMode == InventoryControlMode.Cursor)
            return;
        if (lockSlot)
            return;
        if (_controlMode == InventoryControlMode.Slots)
        {
            if (isSubmitting)
            {
                if (isUsing)
                {
                    lockSlot = true;
                    Lerper.ConstantLerp(slotFaker.position, Highlights.GetHighlight("Use").transform.position, 0.2f, v => { slotFaker.position = v; }, () =>
                    {
                        BabblesVibration.CustomVibration(0.1f, 0.05f);  //Vibration

                        Destroy(slotFaker.gameObject);
                        Highlights.Reset();
                        ShowLabels(false, true);
                        Debug.Log("using");
                        if (!_isSelectionMode)
                        {
                            if (!UseItem(item))
                                UnSelectRessource(item);
                        }
                        else
                        {
                            Debug.Log("giving");
                            DialogueManager.Instance.CheckPNJPreference(item);
                            UIManager.Instance.UIController.CloseInventory(true);
                        }
                        slot.navigation = savedNav;
                        _ = GameTools.DelayedCall(50, () => { _lockExit = false; });
                        isUsing = false;
                        lockSlot = false;
                    });
                }
                else if (isCrafting)
                {
                    lockSlot = true;
                    Lerper.ConstantLerp(slotFaker.position, Highlights.GetHighlight("Craft").transform.position, 0.2f, v => { slotFaker.position = v; }, () =>
                    {
                        if(slotFaker != null)
                            Destroy(slotFaker.gameObject);
                        Highlights.Reset();
                        ShowLabels(false, true);
                        if (!PlaceRessource(item))
                            UnSelectRessource(item);
                        var r = savedNav;
                        // special exception for notebook
                        if (item.Name == "Meduse")
                            r.selectOnRight = null;
                        else 
                        {
                            var me = _ressourceInteractions.Find(t => t.RessourceType.Name == "Meduse")?.GetComponent<Button>();
                            if(me == null)
                                me = _ressourceInteractions.Find(t => t.RessourceType.Name == "Brassart")?.GetComponent<Button>();
                            var nav = me.navigation;
                            nav.selectOnRight = null;
                            me.navigation = nav;
                        }
                        slot.navigation = r;

                        
                        var up = _ressourceInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnUp == slot);
                        up = up.Concat(_objectInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnUp == slot));

                        var down = _ressourceInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnDown == slot);
                        down = down.Concat(_objectInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnDown == slot));

                        var left = _ressourceInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnLeft == slot);
                        left = left.Concat(_objectInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnLeft == slot));

                        var right = _ressourceInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnRight == slot);
                        
                        right = right.Concat(_objectInteractions.Where(t => t.GetComponent<Button>().navigation.selectOnRight == slot));

                        foreach (var t in up)
                        {
                            var nav = t.GetComponent<Button>().navigation;
                            if (slot.navigation.selectOnUp == slot)
                                nav.selectOnUp = null;
                            else
                                nav.selectOnUp = slot.navigation.selectOnUp;
                            t.GetComponent<Button>().navigation = nav;
                        }
                        foreach (var t in down)
                        {
                            var nav = t.GetComponent<Button>().navigation;
                            if (slot.navigation.selectOnUp == slot)
                                nav.selectOnDown = null;
                            else
                                nav.selectOnDown = slot.navigation.selectOnDown;
                            t.GetComponent<Button>().navigation = nav;
                        }
                        foreach (var t in left)
                        {
                            var nav = t.GetComponent<Button>().navigation;
                            if (slot.navigation.selectOnUp == slot)
                                nav.selectOnLeft = null;
                            else
                                nav.selectOnLeft = slot.navigation.selectOnLeft;
                            t.GetComponent<Button>().navigation = nav;
                        }
                        foreach (var t in right)
                        {
                            var nav = t.GetComponent<Button>().navigation;
                            if (slot.navigation.selectOnUp == slot)
                                nav.selectOnRight = null;
                            else
                                nav.selectOnRight = slot.navigation.selectOnRight;
                            t.GetComponent<Button>().navigation = nav;
                        }

                        var objs = _ressourceInteractions
                        .Where(t => t.GetComponent<Button>().navigation.selectOnDown != null)
                        .Where(t => _objectInteractions.Contains
                        (
                            t.GetComponent<Button>().navigation.selectOnDown.GetComponent<ItemInteraction>())
                        );
                        foreach(var obj in objs)
                        {
                            var nav = obj.GetComponent<Button>().navigation;
                            nav.selectOnDown = null;
                            obj.GetComponent<Button>().navigation = nav;
                        }

                        for(int i = 0; i < _ressourceInteractions.Count; i++)
                        {
                            var interaction = _ressourceInteractions[i];
                            if (interaction.RessourceType == _firstItemInShaker || _secondItemInShaker != null)
                                continue;
                            interaction.GetComponent<Button>().Select();
                            break;
                        }

                        _ = GameTools.DelayedCall(50, () => { _lockExit = false; });
                        isCrafting = false;
                        if (CanCraft())
                        {
                            EngageCraftingSlot();
                        }
                        lockSlot = false;
                    });
                    
                } else if (isSelf)
                {
                    UnSelectRessource(item);


                    slot.navigation = savedNav;
                    isSelf = false;
                    Highlights.Reset();
                    ShowLabels(false, true);
                    _ = GameTools.DelayedCall(50, () => { _lockExit = false; });
                } else if (isNotebook)
                {
                    Lerper.ConstantLerp(slotFaker.position, Highlights.GetHighlight("Notebook").transform.position, 0.2f, v => { slotFaker.position = v; }, () =>
                    {
                        BabblesVibration.CustomVibration(0.1f, 0.05f);  //Vibration
                        UnSelectRessource(item);


                        Destroy(slotFaker.gameObject);
                        Highlights.Reset();
                        ShowLabels(false, true);
                        slot.navigation = savedNav;
                        ShowNotebook(item);
                        isNotebook = false;
                        lockSlot = false;
                    });
                }
                return;
            }
            if (InventoryManager.Instance.GetItemData(item) >= 1)
            {
                _lockExit = true;
                if (slotFaker == null)
                {
                    slotFaker = CreateNeutralItemFaker(item, size, rotation);
                    slotFaker.position = slot.transform.position; // starts at item location
                }
                isRessource = item.IsRessource;
                InventoryManager.Instance.RemoveItem(item);

                savedNav = slot.navigation;
                var t = savedNav;
                t.mode = Navigation.Mode.None;
                slot.navigation = t;

                Highlights.MoveHighlight("Item", slot.transform.position);
                if (isRessource)
                {

                    ShowLabels(true, true); //Showing the Labels when selecting ressource (The 3 Labels)
                    AudioManager.Start2DSound("S_SelectionObjet");  //AUDIO

                   
                        if(TutorialManager.Instance.TutorialDone == false && _inFleurTutoPart == true)
                        {
                            if (item.Name == "Fleur")
                            {
                                UIManager.Instance.UIController.StartDialogue("Tuto_Fleur_Selected", true);
                                UIManager.Instance.UIController.IsInTutorialDialogue = true;
                                _inFleurTutoPart = false;
                                _inFleurUseAndPlaceTutoPart = true;


                            }
                            else
                            {
                                UIManager.Instance.UIController.StartDialogue("Tuto_Fleur_NotSelected", true);
                                UIManager.Instance.UIController.IsInTutorialDialogue = true;

                                CancelSlot(item, slot);

                            }
                        }
                        else if (TutorialManager.Instance.TutorialDone == false && _inPoulpeCoussinUseAndPlaceTutoPart == true)
                        {
                            if (item.Name == "PoulpeCoussin")
                            {
                                UIManager.Instance.UIController.StartDialogue("Tuto_UseObject", true);
                                UIManager.Instance.UIController.IsInTutorialDialogue = true;
                            }

                        }
                    

                    if (IsSelectionMode)
                    {
                        Highlights.ShowHighlights("Use");
                        _useLabels.SetActive(true, _isSelectionMode);
                        isUsing = true;
                        fromWhich = Areas.Self;
                        CloseShakerFeedback(true);


                        CursorOnUseFeedback();
                    }
                    else
                    {
                        Highlights.ShowHighlights("Craft");
                        _craftLabels.SetActive(true);
                        isCrafting = true;
                        OpenShakerFeedback(true);
                        CursurOutstideUseFeedback();

                    }
                }
                else
                {

                    ShowLabels(true, false); //Showing the Labels when selecting object (The 2 Labels : Use and Info)
                    AudioManager.Start2DSound("S_SelectionObjet");  //AUDIO


                  
                        if (TutorialManager.Instance.TutorialDone == false && _inPoulpeCoussinUseAndPlaceTutoPart == true)
                        {
                            if (item.Name == "PoulpeCoussin")
                            {
                                UIManager.Instance.UIController.StartDialogue("Tuto_UseObject", true);
                                UIManager.Instance.UIController.IsInTutorialDialogue = true;
                            }

                        }
                    


                    Highlights.ShowHighlights("Use");
                    _useLabels.SetActive(true, _isSelectionMode);
                    isUsing = true;
                    fromWhich = Areas.Self;

                    CursorOnUseFeedback();
                }

                if (item.IsRessource) //To Update the Sprites and ressources number 
                    UpdateBanane();
                else
                    RedrawNumbers();
            }
        }
    }

    public void OpenShakerFeedback(bool playAudio = false)
    {
        //Might be usefull to have a small delay cause the shaker open before the ressource is even on it
        transform.FindDeep("CraftSprite").GetComponent<Image>().sprite = _shakerTop;
        transform.FindDeep("CraftBottom").GetComponent<Image>().enabled = true;
        Debug.Log("OpenShaker");
        _isShakerOpened = true;

        if (playAudio == true)
        {
           // AudioManager.Start2DSound("S_ShakerOuverture");
        }

    }
    public void CloseShakerFeedback(bool playAudio = false)
    {
        transform.FindDeep("CraftSprite").GetComponent<Image>().sprite = _shakerClosed;
        transform.FindDeep("CraftBottom").GetComponent<Image>().enabled = false;
        Debug.Log("CloseShaker");
        _isShakerOpened = false;

        if(playAudio == true && _isShakerOpened == true)
        {
            AudioManager.Start2DSound("S_ShakerFermeture");
        }

    }
    public void SelectNotebookFeedback()
    {
        var rect = transform.FindDeep("CarnetSprite").GetComponent<RectTransform>();
        Lerper.ConstantLerp(rect.anchoredPosition, Vector2.right * 20 + Vector2.up * 70, 0.1f, (v) => { rect.anchoredPosition = v; });
        Lerper.ConstantLerp(rect.localRotation, Quaternion.Euler(0,0, -30), 0.1f, (r) => { rect.localRotation = r; });
    }
    public void DeselectNotebookFeedback()
    {
        var rect = transform.FindDeep("CarnetSprite").GetComponent<RectTransform>();
        Lerper.ConstantLerp(rect.anchoredPosition, Vector2.zero, 0.1f, (v) => { rect.anchoredPosition = v; });
        Lerper.ConstantLerp(rect.localRotation, Quaternion.identity, 0.1f, (r) => { rect.localRotation = r; });
    }

    private void CursorOnUseFeedback()
    {

        Debug.Log("Pochette qui descend");

        Lerper.ConstantLerp(transform.localPosition, transform.localPosition.SetY(300), 0.3f, (v) => { transform.localPosition = v; });
        _isCurrentlyDown = true;
    }

    private void CursurOutstideUseFeedback()
    {
        if (_isCurrentlyDown != false)
        {
            Debug.Log("Pochette qui monte");
            Lerper.ConstantLerp(transform.localPosition, transform.localPosition.SetY(-300), 0.3f, (v) => { transform.localPosition = v; });
            _isCurrentlyDown = false;
        }
    }

    private void ShowLabels(bool show, bool craftable)
    {

        if (craftable == true)
        {
            if (show == true)
            {
                _infoLabels.SetTargeted(true);
                _craftLabels.SetTargeted(true);
                _useLabels.SetTargeted(true);
            }
            else
            {
                _infoLabels.SetTargeted(false);
                _craftLabels.SetTargeted(false);
                _useLabels.SetTargeted(false);
            }
        }
        else
        {
            if (show == true)
            {
                _infoLabels.SetTargeted(true);
                _craftLabels.SetTargeted(false);
                _useLabels.SetTargeted(true);
            }
            else
            {
                _infoLabels.SetTargeted(false);
                _craftLabels.SetTargeted(false);
                _useLabels.SetTargeted(false);
            }
        }


        
    }
    #endregion Slot Moving & Validation
    #region Item Faker
    private RectTransform CreateNeutralItemFaker(Item ressource, Vector2 size, Quaternion rotation)
    {
        var item = Instantiate(_slotItemFakerPrefab, transform).GetComponent<RectTransform>();

        item.GetComponentInChildren<Image>().sprite = ressource.Sprite;
        item.sizeDelta = size;
        item.rotation = rotation;
        return item;
    }
    private bool isSubmitting { get => isCrafting || isUsing || isSelf || isNotebook; }
    public GameObject ItemFakerPrefab { get => _itemFakerPrefab; set => _itemFakerPrefab = value; }
    public List<ItemInteraction> RessourceInteractions { get => _ressourceInteractions; set => _ressourceInteractions = value; }
    public ItemInteraction MeduseItemInteraction { get => _meduseItemInteraction; set => _meduseItemInteraction = value; }

    private bool isCrafting;
    private bool isUsing;
    private bool isSelf;
    private bool isNotebook;
    private Navigation savedNav;
    private Transform slotFaker;
    private bool isRessource;
    private Areas fromWhich;

    private enum Areas
    {
        None,
        Craft,
        Using,
        Notebook,
        Self
    }

    public void CancelSlot(Item item, Button slot)
    {
        if (!isSubmitting && !(isCrafting||isUsing||isSelf))
        {
            if (_firstItemInShaker != null)
                _ = GameTools.DelayedCall(50, () => { CancelCraft();  });
            return;
        }
        isCrafting = false;
        isUsing = false;
        isSelf = false;
        Highlights.Reset();
        slot.navigation = savedNav;
        _ = GameTools.DelayedCall(50, () => { _lockExit = false; });
        UnSelectRessource(item);
        GenerateNavigation();
        CloseShakerFeedback(true);
        DeselectNotebookFeedback();
        CursurOutstideUseFeedback();


        AudioManager.Start2DSound("S_AnnulerSelectionObjet");  //AUDIO

        ShowLabels(false, true); //Unshow the labels
    }
    
    #endregion Item FAker
    #endregion Selection
    #region Navigation Generation
    // auto generate down movement for slots where it's null or objects (also do for every page turn eventually)
    Navigation[] hardSave;
    private void GenerateNavigation()
    {
        if(hardSave == null)
        {
            hardSave = _ressourceInteractions.Select(t => t.GetComponent<Button>().navigation).ToArray();
        }
        for(int i = 0; i < hardSave.Length; i++)
        {
            _ressourceInteractions[i].GetComponent<Button>().navigation = hardSave[i];
        }
        
        for (int i = 0; i < _objectInteractions.Count; i++)
        {
            var nav = _objectInteractions[i].GetComponent<Button>().navigation;
            nav.selectOnUp = null;
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            if (i != 0)
                nav.selectOnLeft = _objectInteractions[i - 1].GetComponent<Button>();
            if(i != _objectInteractions.Count - 1 && _objectInteractions[i+1].IsAssigned)
                nav.selectOnRight = _objectInteractions[i+1].GetComponent<Button>();
            _objectInteractions[i].GetComponent<Button>().navigation = nav;
        }

        //Navigation Carnet
        Navigation carnetNavigation = transform.FindDeep("CarnetSprite").GetComponent<Button>().navigation;
        
        carnetNavigation.selectOnDown = _objectInteractions.Where(t => t.RessourceType != null).OrderBy(t => Vector2.Distance(t.transform.position, transform.FindDeep("CarnetSprite").transform.position)).FirstOrDefault().GetComponent<Button>();
        transform.FindDeep("CarnetSprite").GetComponent<Button>().navigation = carnetNavigation; 
       

        if (CraftEmpty())
        {
            foreach (var ressource in _ressourceInteractions)
            {
                // notebook exception
                if (ressource.RessourceType.Name == "Meduse" || ressource.RessourceType.Name == "Canette")
                {
                    var n = ressource.GetComponent<Button>();
                    var navi = n.navigation;
                    navi.selectOnRight = transform.FindDeep("CarnetSprite").GetComponent<Button>();
                    n.navigation = navi;
                }
                if (ressource.GetComponent<Button>().navigation.selectOnDown != null
                    && _ressourceInteractions.FindIndex(t => t.transform == ressource.GetComponent<Button>().navigation.selectOnDown.transform) != -1)
                    continue;
                // only generates if the current down parameter exists and is towards a ressource. Objects can move.
                ItemInteraction selection = null;
                float x = float.MaxValue;
                foreach (var obj in _objectInteractions)
                {
                    if (!obj.IsAssigned)
                        continue;
                    var xDiff = (ressource.transform.position.x - obj.transform.position.x).Abs();
                    if (xDiff > x)
                        continue;
                    x = xDiff;
                    selection = obj;
                }
                var nav = ressource.GetComponent<Button>().navigation;
                if (selection != null)
                {
                    nav.selectOnDown = selection.GetComponent<Button>();
                    ressource.GetComponent<Button>().navigation = nav;
                }
                else
                {
                    nav.selectOnDown = null;
                    ressource.GetComponent<Button>().navigation = nav;
                }
            }
            foreach (var obj in _objectInteractions)
            {
                if (!obj.IsAssigned)
                    continue;
                var nav = obj.GetComponent<Button>().navigation;
                ItemInteraction selection = null;
                float x = float.MaxValue;
                foreach (var ressource in _ressourceInteractions)
                {
                    if (_objectInteractions.FindIndex(t => t.transform == ressource.GetComponent<Button>().navigation.selectOnDown.transform) == -1)
                        continue;
                    var xDiff = (ressource.transform.position.x - obj.transform.position.x).Abs();
                    if (xDiff > x)
                        continue;
                    x = xDiff;
                    selection = ressource;
                }
                if (selection != null)
                    nav.selectOnUp = selection.GetComponent<Button>();
                else
                    nav.selectOnUp = null;
                obj.GetComponent<Button>().navigation = nav;
            }
        }
        else // if crafting is engaged, you don't want to be able to go to objects
        {
            foreach (var ressource in _ressourceInteractions)
            {
                if (ressource.GetComponent<Button>().navigation.selectOnDown != null
                    && _ressourceInteractions.FindIndex(t => t.transform == ressource.GetComponent<Button>().navigation.selectOnDown.transform) != -1)
                    continue;
                // no need to do anything if it goes to a ressource
                var nav = ressource.GetComponent<Button>().navigation;
                nav.selectOnDown = null;
                ressource.GetComponent<Button>().navigation = nav;
            }
        }
    }
    #endregion Navigation Generation
    #region Craft
    public void EngageCraftingSlot()
    {
        transform.FindDeep("Craft").GetComponent<Button>().enabled = true;
        transform.FindDeep("Craft").GetComponent<Button>().Select();
    }
    public void CancelCraftSlot()
    {
        CancelCraft();
        transform.FindDeep("Craft").GetComponent<Button>().enabled = false;
        _ressourceInteractions[0].GetComponent<Button>().Select();
    }
    #endregion Craft
    #endregion Slots
    #endregion Modes Logic

    #endregion Methods

}
