using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotebookFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private BananeManager _bananeManager = null;

    void Start()
    {
        _bananeManager = UIManager.Instance.UIController.BananeManager;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _bananeManager.SelectNotebookFeedback();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        _bananeManager.DeselectNotebookFeedback();
    }
}
