using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using ClemCAddons;

[ExecuteInEditMode]
public class FlowGrid : MonoBehaviour
{
    private Vector2 _canvas = new Vector2(1080, 1920);
    [SerializeField] private bool _xDirection = true;
    [SerializeField] private bool _yDirection = true;
    private GameObject next;
    private List<List<GridContent>> _gridContents = new List<List<GridContent>>();
    private int _children;
    private RectTransform _rectTransform;
    private bool _lockRefresh = false;
    
    private RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public bool XDirection { get => _xDirection; set => _xDirection = value; }
    public bool YDirection { get => _yDirection; set => _yDirection = value; }

    private class GridContent
    {
        public Vector2 Size;
        public Vector2 Position; // at 0,0
        public Vector2 GetPosition(Vector2 corner) // with anchor on top left, direction bottom right
        {
            return Position + Size * corner;
        }
        public GridContent(Vector2 size, Vector2 position)
        {
            Size = size;
            Position = position;
        }
    }

    void Update()
    {
        if (RectTransform.rect.size != _canvas || _children != transform.childCount)
        {
            _canvas = RectTransform.rect.size;
            _children = transform.childCount;
            PlaceAll();
        }
    }

    public void Refresh()
    {
        PlaceAll();
    }

    private void PlaceAll()
    {
        _gridContents.Clear();
        var children = transform.GetComponentsInChildren<FlowGridElement>();
        for(int i = 0; i < children.Count(); i++)
        {
            var child = children[i];
            if(child != null)
                Place(child.GetComponent<RectTransform>(), child.Size, i);
        }
    }

    private void Place(RectTransform element, Vector2 size, int index)
    {
        int placingRow = -1;
        float placing = -1;
        float placingx = -1;
        var height = size.y;
        for (int i = 0; i < _gridContents.Count; i++)
        {
            var row = _gridContents[i];
            if(_canvas.x - row.Sum(t => t.Size.x) >= size.x)
            {
                var target = row.Find(r => r.Position.x == row.Max(t => t.Position.x));
                if (placing == -1)
                {
                    placingRow = i;
                    placingx = target.GetPosition(Vector2.one).x;
                    placing = target.Position.y;
                }
                height -= target.Size.y;
                if (height <= 0)
                    break;
            }
            else
            {
                height = size.y;
                placing = -1;
            }
        }
        if(placing != -1)
        {
            if (CheckOverflow(index, new GridContent(size, new Vector2(placingx, placing))))
                return;
            _gridContents[placingRow].Add(new GridContent(size, new Vector2(placingx, placing)));
            element.anchoredPosition = ConvertPosition(new Vector2(placingx, placing), size);
            return;
        }
        if (_gridContents.Count == 0)
        {
            if (CheckOverflow(index, new GridContent(size, Vector2.zero)))
                return;
            var f = new List<GridContent>();
            f.Add(new GridContent(size, Vector2.zero));
            _gridContents.Add(f);
            element.anchoredPosition = ConvertPosition(Vector2.zero, size);
            return;
        }
        height = 0;
        var lastRow = _gridContents.Last();
        var width = size.x;
        for(int i = 0; i < lastRow.Count; i++)
        {
            var column = lastRow[i];
            if(column.Position.y + column.Size.y > height)
            {
                height = column.Position.y + column.Size.y;
            }
            width -= column.Size.x;
            if (width <= 0)
                break;
        }
        if (CheckOverflow(index, new GridContent(size, new Vector2(0, height))))
            return;
        var r = new List<GridContent>();
        r.Add(new GridContent(size, new Vector2(0, height)));
        _gridContents.Add(r);
        element.anchoredPosition = ConvertPosition(new Vector2(0, height), size);
    }

    private bool CheckOverflow(int index, GridContent content)
    {
        if (index >= transform.childCount)
            return false;
        var position = content.Position;
        var size = content.Size;
        var pos = ConvertPosition(position, size);
        if (pos.IsBetween(Vector2.zero, _canvas - size))
            return false;
        
        if(next != null)
        {
            var target = transform.GetChild(index);
            target.SetParent(next.transform);
            target.SetAsLastSibling();
            return true;
        }
        var newPage = Instantiate(gameObject, transform.parent);
        next = newPage;
        newPage.GetComponent<FlowGrid>().enabled = false;
        
        for (int i = index - 1; i >= 0 ; i--)
        {
            if (Application.isPlaying)
                Destroy(newPage.transform.GetChild(i).gameObject);
            else
                DestroyImmediate(newPage.transform.GetChild(i).gameObject);
        }

        for (int i = transform.childCount - 1; i >= index; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        _children = transform.childCount;
        newPage.GetComponent<FlowGrid>().enabled = true;
        return true;
    }

    private Vector2 ConvertPosition(Vector2 position, Vector2 size)
    {
        if (!_xDirection)
            position.x = _canvas.x - position.x - size.x;
        if (_yDirection)
            position.y = _canvas.y - position.y - size.y;
        return position;
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(FlowGrid))]
public class FlowGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var flowGrid = target as FlowGrid;
        if (GUILayout.Button("Refresh"))
        {
            flowGrid.Refresh();
        }
        var x = flowGrid.XDirection;
        var y = flowGrid.YDirection;
        base.OnInspectorGUI();
        if (flowGrid.XDirection != x || flowGrid.YDirection != y)
            flowGrid.Refresh();
    }
}
#endif