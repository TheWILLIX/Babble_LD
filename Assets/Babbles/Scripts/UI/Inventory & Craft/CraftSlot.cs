using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftSlot : MonoBehaviour, IDropHandler
{

    [SerializeField] private UICraftRessourceInteract _craftInteract = null;
    private bool _readyToCraft = false;
    private GameObject _ressourceAlreadyPlaced = null;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(eventData.selectedObject);

        if(eventData.pointerDrag.GetComponent<BeginDragRes>().IsOutOfRessource == false)
        {
        
           Debug.Log("OnDrop");
           if(eventData.pointerDrag != null)
           {
                eventData.pointerDrag.GetComponent<BeginDragRes>().IsPlaced = true;

                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;

                eventData.pointerDrag.GetComponent<CanvasGroup>().blocksRaycasts = false;

                _craftInteract.PlaceRessource(eventData.pointerDrag.gameObject);

                if(_readyToCraft == false) //Système non Compatible avec du Reset de Ressource (Provisoire)
                {
                    _readyToCraft = true;
                    _ressourceAlreadyPlaced = eventData.pointerDrag;
                }
                else
                {
                    Destroy(_ressourceAlreadyPlaced); //La ressource qui etait déja placé juste avant
                    Destroy(eventData.pointerDrag); //La ressource qu'on viens de placer
                    _readyToCraft = false;
                }

            }
        }


    }

}
