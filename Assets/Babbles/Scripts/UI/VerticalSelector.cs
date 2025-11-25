using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons;
using System.Linq;
using UnityEngine.UI;

public class VerticalSelector : MonoBehaviour
{
    [SerializeField] private GameObject _rollerItemPrefab = null;
    [SerializeField] private float _distance = 50;
    [SerializeField] private float _size = 100;
    [SerializeField] private Item[] _rollerItem = new Item[] { };

    private bool _visible;
    private int _selected = 0;

    public bool Visible { get => _visible; set => _visible = value; }


    void Update()
    {
        if (!_visible)
        {
            GetComponent<Image>().raycastTarget = false;

            GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            for (int i = 0; i < transform.childCount; i++)
                Destroy(transform.GetChild(i).gameObject);
            return;
        }

        GetComponent<Image>().raycastTarget = true;

        GetComponent<RectTransform>().sizeDelta = Vector2.one * _size;
        if (InputManager.GetButton("UI_Cancel"))
        {
            _visible = false;
            var roller = transform.parent.GetComponentInChildren<RollerMenu>();
            roller.ChangeMode = false;
            for (int i = 0; i < roller.transform.childCount; i++)
            {
                roller.transform.GetChild(i).gameObject.SetActive(true);
            }
            roller.IgnoreAFrame();
            _selected = 0;
        }
        _rollerItem = InventoryManager.Instance.ItemDB.Where(t => InventoryManager.Instance.GetItemData(t) > 0).ToArray();
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
                children[i].localPosition = Vector3.up * i * _distance;
                children[i].GetComponent<Image>().sprite = _rollerItem[i].Sprite;
            }
            ChildrenSelection(children);
        }
    }
    private void ChildrenSelection(List<RectTransform> children)
    {
        if (InputManager.GetButtonDown("ShortcutUp"))
        {
            _selected++;
        }
        if (InputManager.GetButtonDown("ShortcutDown"))
        {
            _selected--;
        }
        if (_selected >= children.Count)
            _selected = children.Count - 1;
        if (_selected < 0)
            _selected = 0;
        children[_selected].GetComponent<Button>().Select();
        if (InputManager.GetButtonDown("UI_Submit"))
        {
            _visible = false;
            var roller = transform.parent.GetComponentInChildren<RollerMenu>();
            roller.ChangeMode = false;
            InventoryManager.Instance.QuickUseItems[roller.Selected] = _rollerItem[_selected];
            for (int i = 0; i < roller.transform.childCount; i++)
            {
                roller.transform.GetChild(i).GetComponent<Button>().enabled = true;
            }
            roller.IgnoreAFrame();
            _selected = 0;
        }
    }
}
