using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using static NPCPositionMemory;
using ClemCAddons;
using Yarn.Unity;
using Luminosity.IO;

public class DialogueManager : Singleton<DialogueManager>
{
    #region Fields
    private bool _isInDialog = false; //UTILE POUR SAVOIR SI ON EST EN DIALOGUE ET FAIRE EN SORTE QUON NE PUISSE PAS OUVRIR INVENTAIRE ETC...
    private bool _isInLeaveDialog = false; 

    [Header("NPC Datas")]
    [SerializeField] private SpeakerData[] _NPCSpeakerData = null;
    private Dictionary<string, SpeakerData> _speakerDataBase = null;

    [SerializeField] private SpeakerData _interactedCharacter = null;

    [SerializeField] private string _characterCurrentlySpeaking = "Bulle";
    private string _previousCharacterSpeaking = string.Empty; //Will be usefull someday

    [Header("Yarn")]
    [SerializeField] private DialogueRunner _dialogueRunner = null;
    [SerializeField] private BabblesLineView _lineView = null;
    [SerializeField] private InMemoryVariableStorage _variableStorage = null;

    private bool _modeFullScreen = false; //Attribute to enable the dialogue box in the middle of the screen instead of above target (Usefull in campement)

    [Header("Font")]
    [SerializeField] private TMP_FontAsset _dumboFont = null;
    private TMP_FontAsset _defaultDialogueFont = null;

    [Header("Sound")]
    [SerializeField] private VoiceSFXDialogue _voiceSFXDialogue = null;

    [Header("Effect")]
    [SerializeField] private TextEffectDialogue _textEffectDialogue = null;

    [Header("Others")]
    [SerializeField] private Sprite _defaultDialogueBubble = null;
    [SerializeField] private GameObject _interactionLeaveHint = null;
    private bool _panneauTalkingModeActive = false;
    [SerializeField] private GameObject _customBehaviour = null;

    [Header("Dialogue Start & End Config")]
   // [SerializeField] private GameObject _dialogBox = null; //In Progress
    [SerializeField] private BabblesOptionsListView _optionView = null; //Attribute to brute force a set active false and true when dialogue has stopped since it keep reapearing


    //[Header("NPC Animations")]
     private Animator[] _npcAnimator = null;

    #endregion Fields

    #region Properties
    public string CharacterCurrentlySpeaking => _characterCurrentlySpeaking;
    public DialogueRunner DialogueRunner => _dialogueRunner;
    public BabblesLineView LineView => _lineView;

    public InMemoryVariableStorage VariableStorage => _variableStorage;
    public bool IsInDialog
    {
        get
        {
           return _isInDialog;
        }
    }

  

    public bool IsInLeaveDialog
    {
        get
        {
            return _isInLeaveDialog;
        }
        set
        {
            _isInLeaveDialog = value;
        }
    }

    public bool ModeFullScreen
    {
        get
        {
            return _modeFullScreen;
        }
        set
        {
            _modeFullScreen = value;
        }
    }

    public VoiceSFXDialogue VoiceSFXDialogue { get => _voiceSFXDialogue; set => _voiceSFXDialogue = value; }



    #endregion Properties


    #region Methods
    #region Awake,Start
    protected override void Awake()
    {
        base.Awake();
        _dialogueRunner.AddCommandHandler<float>("SetSpeed", ModifyTextSpeed); //Change la valeur de défilement du texte des dialogues
        _dialogueRunner.AddCommandHandler<int>("MoreSeum", IncreaseSeum); //Augmente la valeur du seum de Bulle
        _dialogueRunner.AddCommandHandler<int>("LessSeum", LowerSeum); //Descend la valeur du seum de Bulle

        _dialogueRunner.AddCommandHandler<string>("ReceiveItem", ReceiveItemPNJ); //Le PNJ donne un item au joueur

        _dialogueRunner.AddCommandHandler("Open", OpenDialogueBubble); //Ouvre et Ferme la bulle de dialogue, utile pour des effets cinématographique
        _dialogueRunner.AddCommandHandler("Close", CloseDialogueBubble);

        _dialogueRunner.AddCommandHandler<bool>("ModeFullScreen", ActivateFullScreen); //Met les dialogues en FullScreen (Utile lors du campement)

        _dialogueRunner.AddCommandHandler<bool>("Dumbo", DumboMode); //Transforme la police d'écriture en police langage de Dumbo

        _dialogueRunner.AddCommandHandler<bool>("Effect", Effect); //Temporaire (Test pour les effets des textes)

         _dialogueRunner.AddCommandHandler<string>("Story", Story); //Démarre un dialogue Story du PNJ++

        _dialogueRunner.AddCommandHandler<bool>("SignMode", SignMode); //Enlève les animatons des textes et change le sons des dialogues pour simuler une lecture de panneau

        _dialogueRunner.AddCommandHandler("GiveObject", GiveObject); //Donne un Objet au PNJ (Ouvre l'inventaire)

        _dialogueRunner.AddCommandHandler<string>("ResetStade", ResetStade); //Remet le Stade du PNJ++ à 0 (Quand on le rencontre dans une nouvelle zone) 

        _dialogueRunner.AddCommandHandler<int>("RelationMarline", RelationMarline); //Augmente la relation avec Marline

        _dialogueRunner.AddCommandHandler<int>("RelationJeunes", RelationJeunes); //Augmente la relation avec les Jeunes

        _dialogueRunner.AddCommandHandler<bool>("Choice", Choice); //Pour activer les Choix d'intéractions

        _dialogueRunner.AddCommandHandler<string>("ReceiveRecipe", ReceiveRecipe); //Pour recevoir une recette de craft

        _dialogueRunner.AddCommandHandler("LeaveDialogue", ExitInteractionChoice);

        _dialogueRunner.AddCommandHandler<int>("HotuChallenge", HotuChallenge);
        _dialogueRunner.AddCommandHandler<string>("ItemChecker", ItemChecker);

        _dialogueRunner.AddCommandHandler("LockInput", LockInput);
        _dialogueRunner.AddCommandHandler("UnlockInput", UnlockInput);

        _dialogueRunner.AddCommandHandler<int>("Autoskip", AutoSkip); // Auto skip next line


        //--- ANIMATIONS PNJ ---\\
        _dialogueRunner.AddCommandHandler<string>("BulleAnim", BulleAnim); //

        _dialogueRunner.AddCommandHandler<string>("AbletteAnim", AbletteAnim);
        _dialogueRunner.AddCommandHandler<string>("GumpyAnim", GumpyAnim);
        _dialogueRunner.AddCommandHandler<string>("DumboAnim", DumboAnim);
        _dialogueRunner.AddCommandHandler<string>("NapoleonAnim", NapoleonAnim);
        _dialogueRunner.AddCommandHandler<string>("UrfeAnim", UrfeAnim);
        _dialogueRunner.AddCommandHandler<string>("HotuAnim", HotuAnim);
        _dialogueRunner.AddCommandHandler<string>("SloopAnim", SloopAnim);

        _dialogueRunner.AddCommandHandler<string>("MarlineAnim", MarlineAnim);




        //--- SITUATIONS UNIQUES ---\\
        _dialogueRunner.AddCommandHandler("CarteNapoleon", CarteNapoleon);

        _dialogueRunner.AddCommandHandler<string>("ObjectiveNotif", ObjectiveNotif);
        _dialogueRunner.AddCommandHandler<string>("CardUpdate", CardUpdate);


        _dialogueRunner.AddCommandHandler<string>("DebugMessage", DebugMessage);

        _dialogueRunner.AddCommandHandler("ShowStaturf", ShowStaturf);

        _dialogueRunner.AddCommandHandler("SkipTuto", SkipTuto);

    }

    protected override void Start()
    {
      //  base.Start(); //We are not using a dialogueController nor a singleton DialogueManager
        _speakerDataBase = new Dictionary<string, SpeakerData>();

        for (int i = 0; i < _NPCSpeakerData.Length; i++)
        {

            _speakerDataBase.Add(_NPCSpeakerData[i].CharacterName, _NPCSpeakerData[i]);

        }

        _defaultDialogueFont = _lineView.lineText.GetComponent<TMP_Text>().font; //Set the default Font Asset Attribute is the current used font

    }

    #endregion Awake,Start


    #region YarnCommands


    #region Main Commands
    public void Effect(bool enable)
    {
        _textEffectDialogue.EnableEffect = enable;
    }

    public void ModifyTextSpeed(float textSpeedValue)
    {
        _lineView.ChangeTypeWriterSpeed(textSpeedValue);
    }


    #region Seum
    [YarnCommand("MoreSeum")]
    public void IncreaseSeum(int seumAdded)
    {
        Seum.Instance.ChangeSeum(seumAdded);
    }

    [YarnCommand("LessSeum")]
    public void LowerSeum(int seumAdded)
    {
        Seum.Instance.ChangeSeum(-seumAdded);
    }
    #endregion Seum

    //Open and Close Dialogue Bubble

    public void OpenDialogueBubble() 
    {
        _lineView.gameObject.SetActive(true);
    }

    public void CloseDialogueBubble()  
    {
        _lineView.gameObject.SetActive(false);
    }

    //Open and Close Dialogue Bubble



    #region ReceiveItem
    public void ReceiveItemPNJ(string item)
    {
        Item itemToAdd = InventoryManager.Instance.GetItemDataByString(item);

        InventoryManager.Instance.AddItem(itemToAdd);

        try
        {
            var data = new Notification.NotificationData()
            {
                Title = itemToAdd.CleanName,
                Subtitle = "$ReceiveItemSubtitle",
                Description = _characterCurrentlySpeaking + " %%ReceiveItemDescription%% " + itemToAdd.CleanName.Replace("$",""),
                KeyIcon = UIManager.Instance.UIController.HUDBank.InventoryInteractionKey,
                ElementIcon = itemToAdd.Sprite,
                Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
                ActionUponOpening = () => UIManager.Instance.UIController.OpenInventory(true, false, itemToAdd.Name)
            };
            Notification.TriggerNotification(data, 0);
        }
        catch
        {
            Debug.LogError("Error in ReceiveItemPNJ");
            return;
        }
        
    }
    #endregion ReceiveItem

    public void ActivateFullScreen(bool activate)
    {
        if(activate == true)
        {
            ModeFullScreen = true;                
        }
        else
        {
            ModeFullScreen = false;
            _lineView.FullScreenMode(false);
        }
    }

    #region NPC Modes
    public void DumboMode(bool activate)
    {
        if(activate == true)
        {
            _lineView.lineText.GetComponent<TMP_Text>().font = _dumboFont;
            _lineView.lineText.GetComponent<TMP_Text>().enableAutoSizing = false;
            _lineView.lineText.GetComponent<TMP_Text>().fontSize = 120;
        }
        else
        {
            _lineView.lineText.GetComponent<TMP_Text>().font = _defaultDialogueFont;
            _lineView.lineText.GetComponent<TMP_Text>().enableAutoSizing = true;
        }
    }

    public void SignMode(bool activate) //Commande Yarn Spinner pour la lecture des panneaux
    {
        if(activate == true)
        {
            _panneauTalkingModeActive = false;
            _lineView.TypeWriterSpeedActivation(false);
        }
        else
        {
            _panneauTalkingModeActive = true;
            _lineView.TypeWriterSpeedActivation(true);
        }

    }
    #endregion NPC Modes

    #region Interact & Story & Give 

    public void Choice(bool condition)
    {
        Debug.Log("Choice Test");
        UIManager.Instance.UIController.InteractionChoice(condition);

       // _interactionLeaveHint.SetActive(true);
    }

    public void Story(string speaker) //Commande Yarn Spinner pour donner le choix : Donner/Parler
    {
        StartStoryDialogue(speaker);
       // _interactionLeaveHint.SetActive(false);
    }

    public void GiveObject() //Commande Yarn Spinner pour donner le choix : Donner/Parler
    {
        Debug.Log("give object open");
       // OnDialogueComplete();
        UIManager.Instance.UIController.OpenInventory(true, true);
       // _interactionLeaveHint.SetActive(false);
        UIManager.Instance.UIController.IsInGiveSituation = true;

    }
    #endregion Interact & Story & Give


    #region Relations Points PNJ
    public void RelationMarline(int number)
    {
        string characterName = "Marline";

        _speakerDataBase[characterName].RelationPointsPNJ += number;

        if (_speakerDataBase[characterName].RelationPointsPNJ >= _speakerDataBase[characterName].SeuilToIncreaseEachStade) //Si il y a assez de relation pour atteindre un nouveau stade
        {
            if (_speakerDataBase[characterName].StadePNJ <= 1) //Sécrutié en dure car vraiment pas utile de se faire chier a codé un truc paramétrable au niveau des stades.
            {
                _speakerDataBase[characterName].StadePNJ++;
            }

            _speakerDataBase[characterName].RelationPointsPNJ = _speakerDataBase[characterName].RelationPointsPNJ - _speakerDataBase[characterName].SeuilToIncreaseEachStade; //On met les points de relation a 0 en gardant l'excedant de points de relations qui a servit a monter un stade
        }

    }

    public void RelationJeunes(int number)
    {
        string characterName = "Jeunes";

        _speakerDataBase[characterName].RelationPointsPNJ += number;

        //Error Down Below
      
        if (_speakerDataBase[characterName].RelationPointsPNJ >= _speakerDataBase[characterName].SeuilToIncreaseEachStade) //Si il y a assez de relation pour atteindre un nouveau stade
        {
            if (_speakerDataBase[characterName].StadePNJ <= 1) //Sécrutié en dure car vraiment pas utile de se faire chier a codé un truc paramétrable au niveau des stades.
            {
                _speakerDataBase[characterName].StadePNJ++;
            }

            _speakerDataBase[characterName].RelationPointsPNJ = _speakerDataBase[characterName].RelationPointsPNJ - _speakerDataBase[characterName].SeuilToIncreaseEachStade; //On met les points de relation a 0 en gardant l'excedant de points de relations qui a servit a monter un stade
        }
    }
    #endregion Relations Points PNJ

    public void ResetStade(string character)
    {
        _speakerDataBase[character].StadePNJ = 0;
    }


    

    private void ReceiveRecipe(string recipeReceivedName)
    {

        foreach(Recipe recipe in InventoryManager.Instance.RecipeDB)
        {
            if(recipe.name == recipeReceivedName)
            {
                InventoryManager.Instance.UnlockRecipeByRecipe(recipe);
            }
        }     

    }


    public void ObjectiveNotif(string objectiveDataName)
    {

        try
        {
            string lastLetter = objectiveDataName.Substring(objectiveDataName.Length - 1);

            int stadeNumber = int.Parse(objectiveDataName.Substring(objectiveDataName.Length - 2, 1));
            string questTypeText = objectiveDataName.Substring(0, objectiveDataName.Length - 2);




            bool cardOrNot = false;

            EQuestType questType = EQuestType.NONE;


            if (lastLetter == "f")
            {
                cardOrNot = false;
            }
            else
            {
                cardOrNot = true;
            }

            Debug.Log(questTypeText + stadeNumber);


            switch (questTypeText)
            {
                case "SKATE":
                    questType = EQuestType.SKATE;
                    break;
                case "URFE":
                    questType = EQuestType.URFE;
                    break;
                case "SKATEIMAGE":
                    questType = EQuestType.SKATEIMAGE;
                    break;
                case "HOTU":
                    questType = EQuestType.HOTU;
                    break;
                case "SLOOP":
                    questType = EQuestType.SLOOP;
                    break;
            }

            if (cardOrNot == true)
            {
                UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjective(questType, stadeNumber);
            }
            else
            {
                UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjectiveWithoutCard(questType, stadeNumber);
            }


        }
        catch
        {
            Debug.LogError("Error in ObjectiveNotif Yarn Command");
        }

    }

    private void CardUpdate(string objectiveDataName)
    {
        int stadeNumber = int.Parse(objectiveDataName.Substring(objectiveDataName.Length - 1, 1));
        string questTypeText = objectiveDataName.Substring(0, objectiveDataName.Length - 1);

     
        EQuestType questType = EQuestType.NONE;


        switch (questTypeText)
        {
            case "SKATE":
                questType = EQuestType.SKATE;
                break;
            case "URFE":
                questType = EQuestType.URFE;
                break;
            case "SKATEIMAGE":
                questType = EQuestType.SKATEIMAGE;
                break;
            case "HOTU":
                questType = EQuestType.HOTU;
                break;
            case "SLOOP":
                questType = EQuestType.SLOOP;
                break;
        }

        UIManager.Instance.UIController.ObjectifsUpdate.UpdateCard(questType, stadeNumber);


    }

    #endregion Main Commands

    #region Animations Commands

    private void BulleAnim(string animType)
    {
        switch(animType)
        {
            case "Happy":
                FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.Happy);
                break;
            case "Angry":
                FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.Angry);
                break;
            case "Star":
                FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.StaryEyes);
                break;
            case "Sad":
                FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.Sad);
                break;
            case "Idle":
                FreshBulleAnimation.EndFaceAnimation();
                break;
            case "Coucou":
                FreshBulleAnimation.Coucou();
                break;
        }
    }

    private void AbletteAnim(string animType)
    {
        Animator abletteAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Oklm":
                abletteAnimator.SetTrigger("OKLMOn");
                break;

            case "Angry":
                abletteAnimator.SetTrigger("ClashOn");
                break;

            case "Laugh":
                abletteAnimator.SetTrigger("LaughOn");
                break;

            case "Proud":
                abletteAnimator.SetTrigger("ProudOn");
                break;

            case "Clash":
                abletteAnimator.SetTrigger("ClashOn");
                break;

            case "Coucou":
                abletteAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void GumpyAnim(string animType)
    {
        Animator gumpyAnimator = _npcAnimator[1];

        switch (animType)
        {
            case "Angry":
                gumpyAnimator.SetTrigger("AngryOn");
                break;

            case "Laugh":
                gumpyAnimator.SetTrigger("LaughOn");
                break;

            case "Clash":
                gumpyAnimator.SetTrigger("ClashOn");
                break;

            case "Pissed":
                gumpyAnimator.SetTrigger("PissedOn");
                break;

            case "Coucou":
                gumpyAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void DumboAnim(string animType)
    {
        Animator dumboAnimator = _npcAnimator[2];

        switch (animType)
        {
            case "Dumb":
                dumboAnimator.SetTrigger("DumbOn");
                break;

            case "Happy":
                dumboAnimator.SetTrigger("HappyOn");
                break;

            case "Clash":
                dumboAnimator.SetTrigger("ClashOn");
                break;

            case "Coucou":
                dumboAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void NapoleonAnim(string animType)
    {
        Animator napoleonAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Angry":
                napoleonAnimator.SetTrigger("AngryOn");
                break;

            case "Proud":
                napoleonAnimator.SetTrigger("ProudOn");
                break;

            case "Coucou":
                napoleonAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void UrfeAnim(string animType)
    {
        Animator urfeAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Fly":
                urfeAnimator.SetTrigger("FlyOn"); 
                break;

            case "Coucou":
                urfeAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void HotuAnim(string animType)
    {
        Animator hotuAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Proud":
                break;
            case "Sad":
                break;
            case "Victim":
                break;
            case "Coucou":
                break;
        }
    }

    private void MarlineAnim(string animType)
    {
        Animator marlineAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Angry":
                marlineAnimator.SetTrigger("AngryOn");
                break;
            case "Sad":
                marlineAnimator.SetTrigger("SadOn");
                break;
            case "Laugh":
                marlineAnimator.SetTrigger("LaughOn");
                break;
            case "Coucou":
                marlineAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    private void SloopAnim(string animType)
    {
        Animator sloopAnimator = _npcAnimator[0];

        switch (animType)
        {
            case "Happy":
                sloopAnimator.SetTrigger("HappyOn1");
                break;

            case "Happy2":
                sloopAnimator.SetTrigger("HappyOn2");
                break;

            case "Sad":
                sloopAnimator.SetTrigger("SadOn");
                break;

            case "Angry":
                sloopAnimator.SetTrigger("AngryOn");
                break;

            case "Coucou":
                sloopAnimator.SetTrigger("CoucouOn");
                break;
        }
    }

    #endregion Animations


    #region Situations Uniques
    private void AutoSkip(int delay)
    {
        StartCoroutine(DelayedSkip(delay));
    }

    private IEnumerator DelayedSkip(int delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsInDialog)
        {
            DialogueRunner.Stop();
            UIManager.Instance.UIController.IsInInteractionChoice = false;
            OnDialogueLeave();
        }
    }

    private void CarteNapoleon()
    {
        UIManager.Instance.UIController.NapoleonQuestSetup();
    }

    private void HotuChallenge(int challenge)
    {
        EpaveCustomBehaviour.Instance.StartHotuChallenge(challenge);
    }

    private void DebugMessage(string message)
    {
        Debug.Log(message);
    }

    public void ItemChecker(string itemName)
    {
        Item itemToCheck = null;

        foreach (Item item in InventoryManager.Instance.ItemDB)
        {
            if (item.Name == itemName)
            {
                itemToCheck = item;
            }
        }

        if(itemToCheck != null)
        {
            if (InventoryManager.Instance.GetItemData(itemToCheck) >= 1)
            {
                UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$" +itemToCheck.Name+"InInventory", true);
            }
            else
            {
                UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$" + itemToCheck.Name + "InInventory", false);
            }
        }
        else
        {
            return;
        }
    }

    public void LockInput()
    {
        Luminosity.IO.StandaloneInputModule.LockInput = true;
    }

    public void UnlockInput()
    {
        Luminosity.IO.StandaloneInputModule.LockInput = false;
        UIManager.Instance.UIController.Character.SetCanMove(false);
    }


 

    private void ShowStaturf()
    {
         Staturf.ShowStaturf();
        //Audio
    }

    private void SkipTuto()
    {
        TutorialManager.Instance.SkipTuto();
        //Audio
    }

    #endregion Situations Uniques




    #endregion YarnCommands




    #region PNJ++ Interaction

    private string CheckForJeunes(string currentCharacName)
    {
        if(currentCharacName == "Jeunes" || currentCharacName == "Gumpy" || currentCharacName == "Dumbo" || currentCharacName == "Ablette")
        {
            return "Jeunes";
        }
        else
        {
            return currentCharacName;
        }
    }

    public void CheckPNJPreference(Item itemGiven)
    {
        Debug.Log("Checking PNJ Preferences");
        string currentCharacter = CheckForJeunes(_characterCurrentlySpeaking); //Check if it's one of the 'Jeunes'
        SpeakerData speaker = _speakerDataBase[currentCharacter];

        //There is 4 types of preferences
        //The Unique : When a NPC has a unique reaction to a object given (Hotu et Méduse Coussin) = Receive_ITEMNAME
        //The Like : When a NPC like a object = Receive_Like
        //The Dislike : When a NPC dislike a object = Receive_Dislike
        //The Neutral : When nothing is specified/default = Receive

        //DialogueRunner.Stop();

        foreach (Item itemPNJUnique in speaker.ReceiveUniqueItem)
        {
            if (itemPNJUnique == itemGiven)
            {
                Debug.Log("TestUnique");
                DialogueRunner.StartDialogue(speaker.name + "_Receive_" + itemGiven.Name);
                //LE PNJ REAGIT DE FAçON UNIQUE
                RelationPNJ(5);
                OnDialogueLeave();
                return;
            }
        }

        foreach (Item itemPNJLike in speaker.ItemLike)
        {
            if (itemPNJLike == itemGiven)
            {
                DialogueRunner.StartDialogue(speaker.name + "_Receive_Like");
                //LE PNJ EST CONTENT
                RelationPNJ(5);
                OnDialogueLeave();
                return;
            }
        }

        foreach (Item itemPNJDislike in speaker.ItemDislike)
        {
            if (itemPNJDislike == itemGiven)
            {
                DialogueRunner.StartDialogue(speaker.name + "_Receive_Dislike");
                //LE PNJ EST PAS CONTENT.
                RelationPNJ(3);
                OnDialogueLeave();
                return;
            }
        }

        Debug.Log("Failed to find any link of " + itemGiven + " To " + _characterCurrentlySpeaking + " preferences");
        DialogueRunner.StartDialogue(speaker.name + "_Receive");
        RelationPNJ(4);
        OnDialogueLeave();
    }

    public void RelationPNJ(int number) //Not clean at all but for such a small feature i feel like it's a bit pointless to overkill it and make a complex system (Plus we only have 2 to 3 NPC++ max)
    {
        if (_characterCurrentlySpeaking == "Marline")
        {
            RelationMarline(number);
        }
        else if (CheckForJeunes(_characterCurrentlySpeaking) == "Jeunes")
        {
            RelationJeunes(number);
        }
        else
        {
            Debug.Log("The NPC we are giving a object to is not the Jeunes or Marline");
        }
    }



    private void StartStoryDialogue(string character)
    {
        string currentSpeakingCharacter = CheckForJeunes(character);
        SpeakerData speaker = _speakerDataBase[currentSpeakingCharacter];

        ELevelType currentLevel = SceneLoader.CurrentSceneElevelType;

        int stade = speaker.StadePNJ;

        DialogueRunner.Stop(); //Stop the current dialogue, Yarn Spinner is not allowing a new dialogue to start over a already running one

        string nodeTarget = character + "_Story_" + currentLevel + "_" + stade;

        Debug.Log(currentLevel.ToString()  + "    " + stade);


        if (DialogueRunner.NodeExists(nodeTarget))
        {
            DialogueRunner.StartDialogue(nodeTarget); //We try running the dialogue with all the info
            RelationPNJ(5);
        }
        else if (DialogueRunner.NodeExists(currentSpeakingCharacter + "_Story_" + currentLevel + "_" + (stade+1).ToString()) )
        {
            DialogueRunner.StartDialogue(currentSpeakingCharacter + "_NeedNewInteraction");
            OnDialogueLeave();
        }
        else
        {
            Debug.Log("NeedNewLocation");
            DialogueRunner.StartDialogue(currentSpeakingCharacter + "_NeedNewLocation");
            OnDialogueLeave();
           
        }

    }



    #endregion PNJ++ Interaction


    #region Speaker
    public void UpdateSpeaker() //Is Called by Line View 
    {
        AudioManager.Start2DSound("S_OuvertureBulleDialogue");

        string name = UIManager.Instance.UIController.CharacterNameText.text; //On récupère le string du TMP_Text du characterName
        if(_previousCharacterSpeaking == null) { _previousCharacterSpeaking = _characterCurrentlySpeaking; }
        _characterCurrentlySpeaking = String.Concat(name.Where(c => !Char.IsWhiteSpace(c))); //On enlève les espace blancs (juste une sécurité)

        if (_speakerDataBase.ContainsKey(_characterCurrentlySpeaking) != false) 
        {
            //Debug.Log(_characterCurrentlySpeaking);
            //Conditions Astère / Bulle etc....

            //Add Conditions if _characterCurrently Speaking = Bulle and Previous = Astère Or Command ?
        
            if(ModeFullScreen == true)
            {
                _lineView.FullScreenMode(true);
            }
            else if(_characterCurrentlySpeaking == "Astère")
            {
                _lineView.AstereMode();
            }
            else if(_characterCurrentlySpeaking == "Panneau")
            {
                _lineView.PanneauMode();
            }
            else if(_characterCurrentlySpeaking == "Jeunes")
            {
                _lineView.JeunesMode();
            }
            else
            {
                _lineView.ClassicMode();

                ColorUtility.TryParseHtmlString(_speakerDataBase[_characterCurrentlySpeaking].TextColorID, out Color hexColor); //On récupère la couleur du speaker

              //  UIManager.Instance.UIController.CharacterNameView.color = hexColor; //On met la couleur sur le CharacterNameView

                UIManager.Instance.UIController.BulleContour.color = hexColor;
            }

        }
        else
        {
            Debug.LogError("No speaker data was found for " + _characterCurrentlySpeaking);
        }

        NewLine();
    }


    public SpeakerData GetCurrentSpeaker()
    {
       return _speakerDataBase[_characterCurrentlySpeaking];
    }
    #endregion Speaker




    #region New Line & End Line

    public void NewLine()
    {

        SpeakerData speaker = _speakerDataBase[_characterCurrentlySpeaking];

        //Endroit ou on peut démarrer des trucs en début de dialogue (Exemple apparition stylé de la bulle)
        if (_panneauTalkingModeActive == true)
        {
            Animator panneauAnimator = _npcAnimator[0];
            panneauAnimator.SetTrigger("TalkOn");
        }

        if (speaker.VoiceSound.Length >= 1)
        {
            int rand = UnityEngine.Random.Range(0, speaker.VoiceSound.Length);

            if(AudioManager.Instance != null)
            {
                AudioManager.Instance?.StartPNJSound(speaker.VoiceSound[rand]);
            }
        }


        if(_modeFullScreen == true)
        {
            if(_speakerDataBase[_characterCurrentlySpeaking].FaceSprite != null)
            {
                _lineView.FullscreenCharacterFace.sprite = speaker.FaceSprite;
            }
        }


        //SON DES APPARITIONS DE LETTRES
        _voiceSFXDialogue.StartLettersSFX(speaker); //Démare le son des PNJ

    }

    public void EndLine()
    {
        _previousCharacterSpeaking = _characterCurrentlySpeaking;


        if(_panneauTalkingModeActive == true)
        {
            Animator panneauAnimator = _npcAnimator[0];
            panneauAnimator.SetTrigger("TalkOff");
        }
    }

    #endregion New Line & End Line

    #region Event Start Node & Dialogue Complete

    public void OnNodeStart() //Triggered in the Dialogue Runner Event 
    {
        //Main Config
        _lineView.CurrentDialogBox.SetActive(true);
        _lineView.CurrentSkipButton.gameObject.SetActive(true);


        _lineView.BlockSkip = false; //Security to avoid being able to select a response

        _isInDialog = true;
        _lineView.autoAdvance = false;
        _optionView.gameObject.SetActive(true);
        _optionView.ExitOptions(); //A brute force thing again


        _interactionLeaveHint.gameObject.SetActive(false);

    }

    public void OnDialogueComplete() //Triggered in the Dialogue Runner Event
    {
        //Main Config
        _lineView.CurrentDialogBox.SetActive(false);
        _isInDialog = false;
        _optionView.gameObject.SetActive(false); // brute force a set active false and true when dialogue has stopped since it keep reapearing
       
        AudioManager.Start2DSound("S_FermetureBulleDialogue");

        //Quest Check
        QuestUpdateCheck(_characterCurrentlySpeaking); //If Bulle or Astère end the dialogue well shiiittt
    }

    

    public void OnDialogueLeave() //Trigger when we decide to leave the NPC
    {
        _optionView.gameObject.SetActive(false); // brute force a set active false and true when dialogue has stopped since it keep reapearing
        _lineView.autoAdvance = true;
        _lineView.CurrentSkipButton.gameObject.SetActive(false);
        _lineView.BlockSkip = true;

        AudioManager.Start2DSound("S_FermetureBulleDialogue");
    }


    public void StartAurevoirDialogue()
    {
        Debug.Log("TAMER??");
        try
        {        
            if (CharacterCurrentlySpeaking == "Ablette" || CharacterCurrentlySpeaking == "Gumpy" || CharacterCurrentlySpeaking == "Dumbo")
            {
                DialogueRunner.StartDialogue("Jeunes" + "_Aurevoir");
                //We do this dirty because Start Dialogue won't let 
                _isInDialog = false;

            }
            else
            {
                DialogueRunner.StartDialogue(CharacterCurrentlySpeaking + "_Aurevoir");
                _isInDialog = false;
            }
        }
        catch
        {
            Debug.LogError("Error starting the Aurevoir Dialogue");
            return;
        }

    }

    public void ExitInteractionChoice()
    {
        Debug.Log("ExitInteration");

        DialogueRunner.Stop();

        StartAurevoirDialogue();

        UIManager.Instance.UIController.IsInInteractionChoice = false;

        OnDialogueLeave();
        UIManager.Instance.UIController.Character.SetCanMove(true);
    }
    #endregion Event Start Node & Dialogue Complete

    #region Other
    public string DebugRelation(string character = "")
    {
        if (character == "")
        {
            character = CharacterCurrentlySpeaking;
        }
        return _speakerDataBase[character].RelationPointsPNJ.ToString();
    }

    public void AnimatorInit(Animator[] animators)
    {
        _npcAnimator = animators;
    }

    private void QuestUpdateCheck(string npcName) //To Optimize it there is something that could be done (Avoid checking everytime, once a quest is done stop checking for it)
    {
        bool result = false;

        switch (npcName)
        {

            case "Jeunes":
            case "Gumpy":
            case "Dumbo":
            case "Ablette":

                UIManager.Instance.UIController.DialogueManager.VariableStorage.TryGetValue("$skateTaken", out result);
                if (result == true)
                {
                    UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjective(EQuestType.SKATE, 4);
                }
                else
                {
                    UIManager.Instance.UIController.DialogueManager.VariableStorage.TryGetValue("$visitedJeunesIntro", out result);
                    if(result == true)
                    {
            
                        //We can probably delete that 

                    }
                }

                break;

            case "Urfe":
               

            case "Hotu":
                break;

            case "Napoleon":
                UIManager.Instance.UIController.DialogueManager.VariableStorage.TryGetValue("$mapGiven", out result);
                if(result == true)
                {
                  
                    //To delete

                }
                else
                {
                    UIManager.Instance.UIController.DialogueManager.VariableStorage.TryGetValue("$jeuneParle", out result);
                    if (result == true)
                    {
                        UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjectiveWithoutCard(EQuestType.SKATE, 5);
                    }
                }
                break;
        }
    }




    #endregion Other

    #endregion Methods

}
