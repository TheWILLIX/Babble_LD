using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

[ExecuteInEditMode]
public class PlaceOnTopOf : MonoBehaviour
{
    [SerializeField] private RectTransform _target;


    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        rectTransform.anchoredPosition = _target.anchoredPosition + new Vector2(_target.sizeDelta.x /2f,_target.sizeDelta.y);
    }
}
