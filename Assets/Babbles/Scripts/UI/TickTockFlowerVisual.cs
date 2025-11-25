using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TickTockFlowerVisual : MonoBehaviour
{

    [SerializeField] private Sprite _flowerGoodSprite = null;
    [SerializeField] private Sprite _flowerBadSprite = null;
    [SerializeField] private TickTockMinigame _tickTockMinigame = null;

    private Image _flowerImage = null;

    void Start()
    {
        _flowerImage = GetComponent<Image>();
    }

    void Update()
    {

        _flowerImage.sprite = _tickTockMinigame.Valid ? _flowerGoodSprite : _flowerBadSprite;

    }
}
