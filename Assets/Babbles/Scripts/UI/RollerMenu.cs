using ClemCAddons;
using Luminosity.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RollerMenu : MonoBehaviour
{
    [SerializeField] private GameObject _rollerItemPrefab = null;
    [SerializeField] private float _distance = 50;
    [SerializeField] private float _size = 100;
    [SerializeField] private Item[] _rollerItem = new Item[] { };
    [SerializeField] private float _openDelay = 1;

    private bool _visible;
    private float _time;
    private bool _changeMode;
    private int _selected = 0;
    private int _framesToIgnore = 0;

    public bool ChangeMode { get => _changeMode; set => _changeMode = value; }
    public int Selected { get => _selected; set => _selected = value; }


    void Update()
    {
        if(UIManager.Instance.UIController.InventoryOpen || UIManager.Instance.UIController.CarnetOpen || ItemPlacer.ConstructionMode)
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            _visible = false;
            _changeMode = false;
            transform.parent.GetComponentInChildren<VerticalSelector>().Visible = false;
            return;
        }
        if (_framesToIgnore > 0)
        {
            _framesToIgnore--;
            return;
        }
        if (!_visible)
        {
            GetComponent<Image>().raycastTarget = false;
            GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            if (InputManager.GetButton("ShortcutUp") || InputManager.GetButton("ShortcutDown") || InputManager.GetButton("ShortcutLeft") || InputManager.GetButton("ShortcutRight"))
            {
                _time += Time.deltaTime;
                CircularProgress.Setup(transform.position, Vector2.one * 20);
                CircularProgress.SetProgress((_time-0.2f).Max(0) / (_openDelay-0.2f));
            }
            if (InputManager.GetButtonUp("ShortcutUp") || InputManager.GetButtonUp("ShortcutDown") || InputManager.GetButtonUp("ShortcutLeft") || InputManager.GetButtonUp("ShortcutRight"))
            {
                if(_time < 0.2f)
                {
                    int v = 0;
                    if (InputManager.GetButtonUp("ShortcutDown"))
                        v = 1;
                    if (InputManager.GetButtonUp("ShortcutLeft"))
                        v = 2;
                    if (InputManager.GetButtonUp("ShortcutRight"))
                        v = 3;
                    var r = InventoryManager.Instance.TryUseItem(InventoryManager.Instance.QuickUseItems[v]);
                    if (r)
                    {
                        Debug.LogWarning("Potential no quick item left feedback");
                    }
                }
                _time = 0;
                CircularProgress.SetProgress(0);
            }
            if (_time > _openDelay)
            {
                _time = 0;
                CircularProgress.SetProgress(0);
                _selected = 0;
                _visible = true;
                if (InputManager.GetButton("ShortcutUp"))
                {
                    _selected = 0;
                }
                if (InputManager.GetButton("ShortcutRight"))
                {
                    _selected = 1;
                }
                if (InputManager.GetButton("ShortcutDown"))
                {
                    _selected = 2;
                }
                if (InputManager.GetButton("ShortcutLeft"))
                {
                    _selected = 3;
                }
            }
            return;
        }
        GetComponent<Image>().raycastTarget = true;
        GetComponent<RectTransform>().sizeDelta = Vector2.one * _size;
        _rollerItem = InventoryManager.Instance.QuickUseItems;
        var children = GetComponentsInChildren<RectTransform>().ToList();
        children.Remove(GetComponent<RectTransform>());
        if (children.Count != _rollerItem.Length)
        {
            for (int i = 0; i < children.Count; i++)
            {
                Destroy(children[i].gameObject);
            }
            for (int i = 0; i < _rollerItem.Length; i++)
            {
                Instantiate(_rollerItemPrefab, transform);
            }
        }
        if (children.Count > 0)
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].localPosition = Vector3.SlerpUnclamped(Vector3.up, Vector3.right, i / (children.Count / 4f)) * _distance;
                children[i].GetComponent<Image>().sprite = _rollerItem[i].Sprite;
            }
            ChildrenSelection(children);
        }
    }
    private void ChildrenSelection(List<RectTransform> children)
    {
        if (_selected == -1)
        {
            _selected = children.Count - 1;
        }
        if (_selected >= children.Count)
        {
            _selected = 0;
        }
        if (!_changeMode)
        {
            children[_selected].GetComponent<Button>().Select();
            if (InputManager.GetButtonDown("ShortcutUp"))
            {
                _selected = 0;
            }
            if (InputManager.GetButtonDown("ShortcutDown"))
            {
                _selected = 2;
            }
            if (InputManager.GetButtonDown("ShortcutLeft"))
            {
                _selected = 3;
            }
            if (InputManager.GetButtonDown("ShortcutRight"))
            {
                _selected = 1;
            }
            if (InputManager.GetButtonDown("UI_Submit"))
            {
                var selector = transform.parent.GetComponentInChildren<VerticalSelector>();
                selector.transform.position = children[_selected].transform.position + Vector3.up * 25f;
                selector.Visible = true;
                for (int i = 0; i < children.Count; i++)
                {
                    if (i != _selected)
                        children[i].gameObject.SetActive(false);
                }
                _changeMode = true;
            }
            if (InputManager.GetButtonDown("UI_Cancel"))
            {
                _visible = false;
            }
        }
    }
    public void IgnoreAFrame()
    {
        _framesToIgnore++;
    }
}
