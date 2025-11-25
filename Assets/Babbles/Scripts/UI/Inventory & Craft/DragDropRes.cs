using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropRes : MonoBehaviour
{

    private RectTransform _rect = null;
    private CanvasGroup _canvasGroup = null;
    private Canvas _mainCanvas = null;
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

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(Canvas canvas, bool outRessource)
    {
     //   _canvasGroup.alpha = 0.6f;
     //   _canvasGroup.blocksRaycasts = false;
        _mainCanvas = canvas;
        _isOutOfRessource = outRessource;
    }



    
   
    

    public void ResetPosition()
    {
        //   _ressourceClone.gameObject.transform.position = _startPosition;
       // _isPlaced = false;
        // Destroy(_ressourceClone);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
