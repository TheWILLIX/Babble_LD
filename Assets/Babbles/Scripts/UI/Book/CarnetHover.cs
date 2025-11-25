using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarnetHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private GameObject _carnetNormal = null;
    [SerializeField] private GameObject _carnetMove = null;


    public void OnPointerEnter(PointerEventData eventData)
    {
        _carnetMove.SetActive(true);
        _carnetNormal.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _carnetMove.SetActive(false);
        _carnetNormal.SetActive(true);
    }
}

