using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private MainMenuButtons _mainMenuButtons;
    [SerializeField] private bool _defaultSelection = false;
    [SerializeField] private float _targetPosition = 100f;
    [SerializeField] private float _speed = 1;
    private float _baseX;
    private float _target;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        _baseX = rectTransform.anchoredPosition.x;
        _target = _baseX;
        if (_defaultSelection)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
                rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetX(_target);
            });
        }
    }

    void Update()
    {
        var difference = _target - rectTransform.anchoredPosition.x;
        if (difference == 0)
            return;
        rectTransform.anchoredPosition += Vector2.zero.SetX(Mathf.Min(_speed * Time.deltaTime, difference.Abs()) * difference.Sign());
    }

    public void OnSelect(BaseEventData eventData)
    {
        _target = _targetPosition;
        _mainMenuButtons.HideOptions();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _target = _baseX;
    }
}
