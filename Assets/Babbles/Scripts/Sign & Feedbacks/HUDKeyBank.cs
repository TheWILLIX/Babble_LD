using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDKeyBank : MonoBehaviour
{

    [Header("HUD Keys")]

    [SerializeField] public Sprite XButton = null;
    [SerializeField] public Sprite AButton = null;
    [SerializeField] public Sprite BButton = null;
    [SerializeField] public Sprite YButton = null;
    [SerializeField] public Sprite SelectButton = null;
    [SerializeField] public Sprite LtButton = null;
    [SerializeField] public Sprite RtButton = null;

    [SerializeField] public Sprite UndefinedButton = null;

    private Sprite _UI_ValidationKey;
    private Sprite _ressourceInteractionKey;
    private Sprite _dialogueInteractionKey;
    private Sprite _inventoryInteractionKey;
    private Sprite _notebookKey;



    [Header("HUD Notification")]
    [SerializeField] public Sprite BackgroundNotification = null;
    [SerializeField] public Sprite BackgroundObjectifUpdate = null;
    [SerializeField] public Sprite BackgroundCollectibles = null;




    #region Properties

    public Sprite UIValidationKey => _UI_ValidationKey;
    public Sprite RessourceInteractionKey => _ressourceInteractionKey;
    public Sprite DialogueInteractionKey => _dialogueInteractionKey;
    public Sprite InventoryInteractionKey => _inventoryInteractionKey;
    public Sprite NotebookKey => _notebookKey;

    #endregion Properties


    private void Start()
    {
        _UI_ValidationKey = AButton;
        _ressourceInteractionKey = XButton;
        _dialogueInteractionKey = AButton;
        _inventoryInteractionKey = YButton;
        _notebookKey = YButton;
    }

}
