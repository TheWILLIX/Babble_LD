using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



 public class BeginDragRes : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
 {
    
    [SerializeField] ERessourceType _ressourceType = ERessourceType.NONE;


    private Vector3 _startPosition = Vector3.zero;

    private RectTransform _craftRectTransfrom = null;
    private CanvasGroup _canvasGroup = null;
    private bool _isPlaced = false;
    private bool _isOutOfRessource = false;

    [Header("Clone Info")]
    [SerializeField] private GameObject _ressourceClone = null;

    public bool IsOutOfRessource
    {
        get
        {
            return _isOutOfRessource;
        }
        set
        {
            _isOutOfRessource = value;
        }
    }

    public bool IsPlaced
    {
        get
        {
            return _isPlaced;
        }
        set
        {
            _isPlaced = value;
        }
    }



    private void Start()
    {
        _startPosition = this.gameObject.transform.position;
        _craftRectTransfrom = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    #region Interface Pointer
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        /*
        Debug.Log("Test BeginDrag");

        if(_isOutOfRessource == false)
        {
            _ressourceClone = Instantiate(_ressourceClone, _startPosition, Quaternion.identity, UIManager.Instance.UIController.CraftInteract.transform);
            
            switch(_ressourceType)
            {
                case ERessourceType.MEDUSE:
                    UIManager.Instance.UIController.ButtonCraftList[0] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;


                case ERessourceType.FRUIT:
                    UIManager.Instance.UIController.ButtonCraftList[1] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;


                case ERessourceType.FLEUR:
                    UIManager.Instance.UIController.ButtonCraftList[2] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;


                case ERessourceType.ALGUE:
                    UIManager.Instance.UIController.ButtonCraftList[3] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;

                case ERessourceType.CREVETTE:
                    UIManager.Instance.UIController.ButtonCraftList[4] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;

                case ERessourceType.POULPE:
                    UIManager.Instance.UIController.ButtonCraftList[5] = _ressourceClone.GetComponent<BeginDragRes>();
                    break;
            }
           // 
          //  _ressourceClone.GetComponent<DragDropRes>().Init(_canvas, _isOutOfRessource);

            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;

            UIManager.Instance.UIController.CraftInteract.SelectRessource(_ressourceType);

        }

        */

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsOutOfRessource == false)
        {
     //       _craftRectTransfrom.anchoredPosition += eventData.delta / UIManager.Instance.UIController.MainCanvas.scaleFactor;
        }

      //  _craftRectTransfrom.anchoredPosition += eventData.delta / UIManager.Instance.UIController.MainCanvas.scaleFactor;

    }


    public void OnPointerDown(PointerEventData eventData)
    {
       // Debug.Log("On Pointer Down");
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (IsOutOfRessource == false)
           {
               if (_isPlaced == false)
               {
                   ResetPosition();
               }
               _canvasGroup.alpha = 1f;
       //        UIManager.Instance.UIController.CraftInteract.UnSelectRessource(_ressourceType);
           }
    }

    private void  ResetPosition()
    {
         // this.transform.position = _startPosition;
         //Destroy(this.gameObject);
         _isPlaced = false;

    }


    #endregion Interface Pointer
    /*
       [SerializeField] ERessourceType _ressourceType = ERessourceType.NONE;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private UICraftRessourceInteract _craftInteract = null;

    private Vector3 _startPosition = Vector3.zero;

    private RectTransform _craftRectTransfrom = null;
    private CanvasGroup _canvasGroup = null;
    private bool _isPlaced = false;
    private bool _isOutOfRessource = false;

    public bool IsOutOfRessource
    {
        get
        {
            return _isOutOfRessource;
        }
        set
        {
            _isOutOfRessource = value;
        }
    }
    public bool IsPlaced
    {
        get
        {
            return _isPlaced;
        }
        set
        {
            _isPlaced = value;
        }
    }


    private void Start()
    {
        _startPosition = this.gameObject.transform.position;
        _craftRectTransfrom = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    #region Interface Pointer

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(IsOutOfRessource == false)
        {
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;

            _craftInteract.SelectRessource(_ressourceType);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (IsOutOfRessource == false)
        {
            _craftRectTransfrom.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsOutOfRessource == false)
        {
            if (_isPlaced == false)
            {
                ResetPosition();
            }
            _canvasGroup.alpha = 1f;
            _craftInteract.UnSelectRessource(_ressourceType);
        }
    }

    public void ResetPosition()
    {
        this.gameObject.transform.position = _startPosition;
        _isPlaced = false;
        _canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
       // Debug.Log("On Pointer Down");
    }


    #endregion Interface Pointer
      
   */
}
