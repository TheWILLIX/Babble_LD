using Luminosity.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;


public class UICraftRessourceInteract : MonoBehaviour
{

    #region Fields
    private ERessourceType _ressourceSelected = ERessourceType.NONE;
    private ERessourceType _ressourcePlaced = ERessourceType.NONE;
    private GameObject _objectPlaced = null;

    [SerializeField] private GameObject _feedbackBubble = null;

    #endregion Fields


    #region Methods

    public void SelectRessource(ERessourceType ressourceType)
    {
       _ressourceSelected = ressourceType;
       Debug.Log(_ressourceSelected + " SELECTED");
    }

        public void UnSelectRessource(ERessourceType ressourceType)
        {
            _ressourceSelected = ERessourceType.NONE;
        }

    public void PlaceRessource(GameObject refObj)
    {
        if(_ressourcePlaced != ERessourceType.NONE) //RESSOURCE PLACÉ + RESSOURCE DANS LES MAINS -> CRAFT
        {
            //ENLEVE LA RESSOURCES DES MAINS
            Craft(refObj);
        }
        else if (_ressourceSelected != ERessourceType.NONE) //RESSOURCE DANS LES MAINS MAIS AUCUNE RESSOURCE PLACÉ -> PLACE RESSOURCE
        {
            _ressourcePlaced = _ressourceSelected;
            _objectPlaced = refObj;
            Debug.Log(_ressourcePlaced + " PLACED");
        }
        else
        {
            Debug.Log("PLACEMENT FAILED : NO RESSOURCES IN HANDS");
        }
    }


    public void ResetCraftSlot(GameObject refObj)
    {
        Debug.Log(_ressourcePlaced + " UN-PLACED");

         _ressourcePlaced = ERessourceType.NONE;

       // _objectPlaced.GetComponent<BeginDragRes>().ResetPosition();
        _objectPlaced.GetComponent<CanvasGroup>().blocksRaycasts = true;

       // refObj.GetComponent<BeginDragRes>().ResetPosition();
        refObj.GetComponent<CanvasGroup>().blocksRaycasts = true;

    }

    private void FeedbackCraft()
    {
        StartCoroutine(DelayCraftFeedback());
    }

    private IEnumerator DelayCraftFeedback()
    {
        _feedbackBubble.SetActive(true);
        yield return new WaitForSeconds(1f);
        _feedbackBubble.SetActive(false);
    }

    private void Craft(GameObject refObj) 
    {

        switch(_ressourcePlaced)
        {/*
             
            case ERessourceType.MEDUSE:         //CATEGORIE MEDUSE DANS EMPLACEMENT CRAFT (2 Craft Possible)
                switch (_ressourceSelected)
                {
                    case ERessourceType.FRUIT:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.MEDUSECOUSSIN);
                        Debug.Log("CRAFT DONE : MEDUSE COUSSIN");
                        break;
                    case ERessourceType.ALGUE:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.MEDUSEGOURMANDE);
                        Debug.Log("CRAFT DONE : MEDUSE GOURMANDE");
                        break;
                    default:
                        ResetCraftSlot(refObj);
                        Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                        return;
                }    
                break;


            case ERessourceType.FRUIT:      //CATEGORIE FRUIT DANS EMPLACEMENT CRAFT (1 Craft Possible)
                switch (_ressourceSelected)
                {
                    case ERessourceType.MEDUSE:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.MEDUSECOUSSIN);
                        Debug.Log("CRAFT DONE : MEDUSE COUSSIN");
                        break;
                    default:
                        ResetCraftSlot(refObj);
                        Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                        return;
                }
                break;


            case ERessourceType.FLEUR:      //CATEGORIE FLEUR DANS EMPLACEMENT CRAFT (1 Craft Possible)
                switch (_ressourceSelected)
                {
                    case ERessourceType.ALGUE:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.SUSHI);
                        Debug.Log("CRAFT DONE : CAMPEMENT FEU");
                        break;
                    default:
                        ResetCraftSlot(refObj);
                        Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                        return;
                }
                break;


            case ERessourceType.ALGUE:      //CATEGORIE ALGUE DANS EMPLACEMENT CRAFT (2 Craft Possible)
                switch (_ressourceSelected)
                {
                    case ERessourceType.MEDUSE:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.MEDUSEGOURMANDE);
                        Debug.Log("CRAFT DONE : MEDUSE GOURMANDE");
                        break;
                    case ERessourceType.FLEUR:
                        UIManager.Instance.UIController.InventoryManager.AddCraftObject(EObjectType.SUSHI);
                        Debug.Log("CRAFT DONE : CAMPEMENT FEU");
                        break;
                    default:
                        ResetCraftSlot(refObj);
                        Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                        return;
                }
                break;


            case ERessourceType.CREVETTE:
                ResetCraftSlot(refObj);
                Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                return;

            case ERessourceType.POULPE:
                ResetCraftSlot(refObj);
                Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                return;

            default:
                ResetCraftSlot(refObj);
                Debug.LogWarning("CRAFT FAILED : Ressource Selected + Placed doesn't work with Craft. You probably assembled two ressources that doesn't match together yet");
                return;*/
        }
     

     //   UIManager.Instance.UIController.InventoryManager.RemoveRessources(_ressourcePlaced,1);
    //    UIManager.Instance.UIController.InventoryManager.RemoveRessources(_ressourceSelected, 1);

        ResetCraftSlot(refObj);

    //    UIManager.Instance.UIController.UpdateCraft();

    }

    #endregion Methods

}
