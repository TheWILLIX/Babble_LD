using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Highlights : MonoBehaviour
{
    private Highlight[] _highlights;
    private static Highlights _instance;

    private class Highlight
    {
        public GameObject Object;
        public bool Visible;
        public Highlight(GameObject gameObject, bool visible)
        {
            Object = gameObject;
            Visible = visible;
        }
    }

    void Start()
    {
        _instance = this;
        _highlights = new Highlight[transform.childCount];
        for (int i = 0; i < _highlights.Length; i++)
        {
            _highlights[i] = new Highlight(transform.GetChild(i).gameObject, false);
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public static void Reset()
    {
        for (int i = 0; i < _instance._highlights.Length; i++)
            _instance._highlights[i].Object.SetActive(false);
    }

    public static void ShowHighlights(params string[] name)
    {
        for(int i = 0; i < _instance._highlights.Length; i++)
        {
            var obj = _instance._highlights[i].Object;
            obj.SetActive(name.Contains(obj.name));
        }
    }

    public static void MoveHighlight(string name, Vector3 position)
    {
        Array.Find(_instance._highlights, t => t.Object.name == name).Object.transform.position = position;
    }

    public static string[] GetHighlights()
    {
        return _instance._highlights.Where(t => t.Visible == true).Select(t => t.Object.name).ToArray();
    }

    public static GameObject GetHighlight(string name)
    {
        return Array.Find(_instance._highlights, t => t.Object.name == name).Object;
    }
}
