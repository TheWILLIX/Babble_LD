using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using ClemCAddons;

public class ItemInteraction : MonoBehaviour, IPointerDownHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IMoveHandler, ICancelHandler
{
    #region Fields
    [SerializeField] private Item _ressourceType = null;
    [SerializeField] private Image _ressourcePocketImage = null;
    [SerializeField] private TMP_Text _ressourceNumber = null;

    private BananeManager _bananeManager = null;
    private bool _selected;
    //private bool _assigned = false; //This attribute is only usefull for the crafted objects (When a new craft is discovered game will a non assigned slot for the discovery to be placed in).
    #endregion Fields

    #region Properties
    public Item RessourceType
    {
        get
        {
            return _ressourceType;
        }
        set     // The set is only necessary for the oral if you are in july 2022 or august 2022 please remove it
        {
            _ressourceType = value;
        }
    }
    public bool IsAssigned
    {
        get
        {
            return _ressourceType != null;
        }
    }

    #endregion Properties

    #region Methods

    void Start()
    {
        _bananeManager = GetComponentInParent<BananeManager>();
    }

    void Update()
    {
        if (!_selected)
            return;
        if (transform.position.ToVector2().IsBetween(Vector2.zero,new Vector2(Screen.width, Screen.height)))
        {
            return;
        }
        UIManager.Instance.UIController.BananeManager.ChangeObjectPage(transform.position.x.Sign().Round(), GetComponent<Button>());

        var r = GetComponent<Button>().colors;
        
       //_ressourcePocketImage.color = t;  //Usefull to know when you have your cursor on a pocket ressource  

        
    }

    public void UpdateRessourceSprite(Sprite sprite)
    {
        ClemCAddons.Utilities.GameTools.RunInMainThread(() => _ressourcePocketImage.sprite = sprite);
    }

    public void UpdateRessourceNumber(bool noRessource = false)
    {
        ClemCAddons.Utilities.GameTools.RunInMainThread(() =>
        {
            if (noRessource == true)
            {
                _ressourceNumber.text = "?";
                //Make it transparent when there is no object 
                Image image = GetComponent<Image>();
                var tempColor = image.color;
                tempColor.a = 0f;
                image.color = tempColor;
            }
            else
            {
                ClemCAddons.Utilities.GameTools.RunInMainThread(() =>
                _ressourceNumber.text = (InventoryManager.Instance.GetItemData(_ressourceType)).ToString());

                //Make it non-transparent when there is no object 
                if (_ressourceType.IsRessource == false)
                {
                    Image image = GetComponent<Image>();
                    var tempColor = image.color;
                    tempColor.a = 1f;
                    image.color = tempColor;
                }
            }
        });
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_bananeManager.ControlMode == BananeManager.InventoryControlMode.Cursor)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(1, () => { EventSystem.current.SetSelectedGameObject(null); });
            // delay by a while to get in next frame
            return;
        }
        _bananeManager.SlotSelected(GetComponent<Button>(), GetComponent<RectTransform>().sizeDelta.Min() * Vector2.one, GetComponent<RectTransform>().rotation, _ressourceType);
        _selected = true;
        if(_ressourcePocketImage != null)
            _ressourcePocketImage.color = GetComponent<Button>().colors.selectedColor;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _selected = false;
        _bananeManager.SlotDeselected();
        if (_ressourcePocketImage != null)
            _ressourcePocketImage.color = GetComponent<Button>().colors.normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_bananeManager.ControlMode == BananeManager.InventoryControlMode.Cursor)
            _bananeManager.SelectRessource(_ressourceType, GetComponent<RectTransform>().sizeDelta.Min() * Vector2.one, GetComponent<RectTransform>().rotation);

    }

    public void TemporaryClick()
    {
        _bananeManager.SelectRessource(_ressourceType, GetComponent<RectTransform>().sizeDelta.Min() * Vector2.one, GetComponent<RectTransform>().rotation);
    }

    public void AssignItem(Item item)
    {
        _ressourceType = item;
    }

    public void UnassignItem()
    {
        _ressourceType = null;
    }


    public void OnSubmit(BaseEventData eventData)
    {
        _bananeManager.ItemSubmitted(_ressourceType, GetComponent<RectTransform>().sizeDelta.Min() * Vector2.one, GetComponent<RectTransform>().rotation, GetComponent<Button>());
    }

    public void OnMove(AxisEventData eventData)
    {
        _bananeManager.SubmittedItemMove(eventData.moveVector);
    }

    public void OnCancel(BaseEventData eventData)
    {
        _bananeManager.CancelSlot(_ressourceType, GetComponent<Button>());
    }

    #endregion Methods

}
