using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageTurning : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _pageCornerTurningSprite = null;
    [SerializeField] private GameObject _pageCornerSprite = null;


    public void OnPointerEnter(PointerEventData eventData)
    { 
        _pageCornerTurningSprite.SetActive(true);
        _pageCornerSprite.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pageCornerTurningSprite.SetActive(false);
        _pageCornerSprite.SetActive(true);
    }
}
