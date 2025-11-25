using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Luminosity.IO;
using UnityEngine.UI;
using System.IO;

[ExecuteInEditMode]
public class Paging : MonoBehaviour
{
    [SerializeField] private bool _enableEditor;
    [SerializeField, Range(0, 0.5f)] private float _spacing = 0;
    [SerializeField, Range(0, 0.5f)] private float _sideSpacing = 0;
    [SerializeField] private GameObject[] _additionalHiding;
    [SerializeField] private Image _categoryImage;
    [SerializeField] private Sprite[] _categorySprites;
    [SerializeField] private Image _bottomRightCornerImage;
    [SerializeField] private Image _bottomLeftCornerImage;
    [SerializeField] private Image _topRightCornerImage;
    [SerializeField] private Image _topLeftCornerImage;
    [SerializeField] private Image _title;
    private int doublePage = 0;
    [NonSerialized] public string FirstPage = "";
    [NonSerialized] public string SecondPage = "";
    [NonSerialized] public string Category = "";
    public List<CategoryContent> Content = new List<CategoryContent>();

    private BananeManager _bananeManager;
    private RectTransform _rectTransform;


    private static Paging _instance;
    private bool visible = false;



    public int DoublePage
    {
        get => doublePage;
        set
        {
            doublePage = value;
        }
    }

    [Serializable]
    public class CategoryContent
    {
        public string Name;
        public List<string> Content;
        public int LastPriority;
        public CategoryContent(string name, List<string> content = default)
        {
            Name = name;
            Content = content;
            LastPriority = 0;
        }
        public void AddCard(string name, bool priority)
        {
            Content.Insert(LastPriority, name);
            LastPriority++;
        }
        public void RemoveCard(string name)
        {
            var id = Content.IndexOf(name);
            Content.RemoveAt(id);
            if (id < LastPriority)
                LastPriority--;
        }
    }

    private RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    private int page1
    {
        get
        {
            return doublePage * 2;
        }
    }

    private int page2
    {
        get
        {
            return doublePage * 2 + 1;
        }
    }

    public static Paging Instance { get => _instance; set => _instance = value; }

    void Start()
    {
        Hide();
        _instance = this;
    }

    public bool AddCard(string card, string category, bool priorityCard = false)
    {
        foreach (var cat in Content)
        {
            if (cat.Name == category)
            {
                cat.AddCard(card, priorityCard);
                return true;
            }
        }
        return false;
    }

    public bool ModifyCard(string oldCardName, string newCardName, string category)
    {
        for (int t = 0; t < Content.Count; t++)
        {
            var cat = Content[t];
            if (cat.Name == category)
            {
                int target = -1;
                for (int i = 0; i <= cat.Content.Count; i++)
                {
                    if (cat.Content[i] == oldCardName)
                    {
                        target = i;
                        break;
                    }
                }

                if (target == -1)
                {
                    return false;
                }

                cat.Content[target] = newCardName;

                return true;
            }
        }
        return false;
    }

    public bool RemoveCard(string card, string category)
    {
        foreach (var cat in Content)
        {
            if (cat.Name == category)
            {
                cat.RemoveCard(card);
                return true;
            }
        }
        return false;
    }

    internal void Show()
    {
        AudioManager.Start2DSound("S_CarnetOpen");
        visible = true;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        foreach (var add in _additionalHiding)
        {
            add.SetActive(true);
#if(UNITY_EDITOR)
            EditorUtility.SetDirty(add);
#endif
        }
    }
    public void Show(BananeManager bananeManager)
    {
        AudioManager.Start2DSound("S_CarnetOpen");

        _bananeManager = bananeManager;
        visible = true;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        foreach (var add in _additionalHiding)
        {
            add.SetActive(true);
#if (UNITY_EDITOR)
            EditorUtility.SetDirty(add);
#endif
        }
    }
    public void Show(Item item, BananeManager bananeManager)
    {
        Show(item.Name, bananeManager);
    }


    public void Show(string name, BananeManager bananeManager)
    {
        AudioManager.Start2DSound("S_CarnetOpen");

        foreach (var add in _additionalHiding)
        {
            add.SetActive(true);
        }
        _bananeManager = bananeManager;
        visible = true;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        int cat = -1;
        int id = 0;
        for (int i = 0; i < Content.Count; i++)
        {
            id = 0;
            foreach (var card in Content[i].Content)
            {
                if (card == name + "Card" || card == name)
                {
                    cat = i;
                    break;
                }
                id++;
            }
            if (cat != -1)
                break;
        }
        if (cat == -1)
        {
            doublePage = 0;
            return;
        }
        int innerPage = 0;
        for (int i = 0; i < transform.GetChild(cat).childCount; i++)
        {
            var page = transform.GetChild(cat).GetChild(i);
            if (page.childCount > id)
            {
                innerPage = i;
                break;
            }
            id -= page.childCount;
        }
        int innerDouble = Mathf.FloorToInt(innerPage / 2f);
        for (int i = 0; i < cat; i++)
        {
            innerDouble += Mathf.CeilToInt(transform.GetChild(i).childCount / 2f);
        }
        doublePage = innerDouble;
    }

    public void ShowCategory(string name, BananeManager bananeManager)
    {
        AudioManager.Start2DSound("S_CarnetOpen");

        foreach (var add in _additionalHiding)
        {
            add.SetActive(true);
        }
        _bananeManager = bananeManager;
        visible = true;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        int cat = -1;
        for (int i = 0; i < Content.Count; i++)
        {
            if (Content[i].Name == name)
            {
                cat = i;
                break;
            }
        }
        if (cat == -1)
        {
            doublePage = 0;
            return;
        }
        for (int i = 0; i < cat; i++)
        {
            doublePage = Mathf.CeilToInt(transform.GetChild(i).childCount / 2f);
        }
    }

    public void Hide()
    {
        AudioManager.Start2DSound("S_CarnetOpen");

        foreach (var add in _additionalHiding)
        {
            add.SetActive(false);
#if (UNITY_EDITOR)
            EditorUtility.SetDirty(add);
#endif
        }
        visible = false;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(1920, 1080);
    }

    public void Load()
    {
        var r = Saver.Instance.Load<List<CategoryContent>>(80085);
        if (r == null)
            return;
        Content = r;
    }

    public void Save()
    {
        Saver.Instance.Save(80085, Content);
    }

    public void Share()
    {
        var r = Saver.Instance.GetPath(80085);
        if (!File.Exists(r[0]))
        {
            return;
        }

        r[0] = r[0].Replace(@"/", @"\");   // explorer doesn't like front slashes
        string argument = "/select," + r[0];

        GUIUtility.systemCopyBuffer = r[1];

#if (UNITY_EDITOR)
        EditorUtility.DisplayDialog("Share Save", "Key copied in clipboard: " + r[1], "Understood");
        System.Diagnostics.Process.Start("explorer.exe", argument);
#endif

    }


    public void Import()
    {
#if (UNITY_EDITOR)

        if (EditorUtility.DisplayDialog("Read Save", "Copy the key in your clipboard", "Done", "Cancel"))
        {
            Saver.Instance.SetKey(80085, GUIUtility.systemCopyBuffer);



            var r = Saver.Instance.GetPathOnly(80085);

            r = r.Replace(@"/", @"\");   // explorer doesn't like front slashes
            string argument = "/select," + r;

            System.Diagnostics.Process.Start("explorer.exe", argument);
            if (EditorUtility.DisplayDialog("Read Save", "Now you can replace your file by the shared file", "Done, load", "Done, don't load"))
                Load();
        }
#endif
    }

    public void Clear()
    {
        Saver.Instance.Clear(80085);
    }

    public void ClearAll()
    {
        Content = default;
    }

    void Update()
    {
        if (visible && Application.isPlaying)
        {
            if (InputManager.GetButtonDown("UI_Cancel"))
            {
                _bananeManager.HideNotebook();
            }
            if (InputManager.GetButtonDown("NotebookPage+"))
            {
                AudioManager.Start2DSound("S_CarnetPage");

                int totalDouble = 0;
                for (int i = 0; i < transform.childCount; i++)
                {
                    totalDouble += Mathf.CeilToInt((transform.GetChild(i).childCount) / 2f).Max(0);
                }
                doublePage = (doublePage + 1).Min(totalDouble - 1);
            }
            if (InputManager.GetButtonDown("NotebookPage-"))
            {
                AudioManager.Start2DSound("S_CarnetPage");

                doublePage = (doublePage - 1).Max(0);
            }
            if ((InputManager.GetAxis("NotebookCategory+") > 0.5f).OnceIfTrueGate("NotebookUp+".GetHashCode()))
            {
                AudioManager.Start2DSound("S_CarnetPage");

                int doublePages = 0;
                int cat = Content.FindIndex(t => t.Name == Category);
                for (int i = 0; i <= cat; i++)
                {
                    doublePages += Mathf.CeilToInt(transform.GetChild(i).childCount / 2f);
                }
                doublePage = doublePages;
            }
            if ((InputManager.GetAxis("NotebookCategory-") > 0.5f).OnceIfTrueGate("NotebookUp-".GetHashCode()))
            {
                AudioManager.Start2DSound("S_CarnetPage");

                int doublePages = 0;
                int cat = Content.FindIndex(t => t.Name == Category);
                for (int i = 0; i < cat - 1; i++)
                {
                    doublePages += Mathf.CeilToInt(transform.GetChild(i).childCount / 2f);
                }
                doublePage = doublePages;
            }
        }
        if (!Application.isPlaying && !_enableEditor)
            return;
        int pages = 0;
        if (Content.Count > transform.childCount && transform.childCount > 0)
            Instantiate(transform.GetChild(0), transform);
        if (transform.childCount > Content.Count && Content.Count > 0)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(transform.childCount - 1).gameObject);
            else
                DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            var category = transform.GetChild(i);
            if (Content.Count <= i)
            {
                Content.Add(new CategoryContent(category.name));
            }
            else
            {
                if (category.name != Content[i].Name)
                    category.name = Content[i].Name;
                var elements = category.GetComponent<PagingCategory>().Elements;
                if (elements.Count != Content[i].Content.Count || !elements.SequenceEqual(Content[i].Content))
                {
                    category.GetComponent<PagingCategory>().Elements = Content[i].Content.ToList();
                    category.GetComponent<PagingCategory>().Refresh();
                }
            }
            for (int t = 0; t < category.childCount; t++)
            {
                var child = category.GetChild(t);
                child.gameObject.name = "Page " + t;
                if (child.GetComponent<FlowGrid>().XDirection != true) // (t % 2 == 1))
                {
                    child.GetComponent<FlowGrid>().XDirection = true;//t % 2 == 1;
                    child.GetComponent<FlowGrid>().Refresh();
                }
                child.GetComponent<RectTransform>().pivot = Vector2.zero;
                if (pages + t != page1 && pages + t != page2)
                {
                    child.GetComponent<RectTransform>().sizeDelta = child.GetComponent<RectTransform>().rect.size;
                    child.GetComponent<RectTransform>().anchorMin = child.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                    child.position = -child.GetComponent<RectTransform>().rect.size * 2;
#if (UNITY_EDITOR)
                    EditorUtility.SetDirty(child);
#endif
                }
                else if (pages + t == page1)
                {
                    _bottomLeftCornerImage.enabled = t != 0;
                    _topLeftCornerImage.enabled = i != 0;
                    _topRightCornerImage.enabled = i != transform.childCount - 1;
                    _categoryImage.sprite = _categorySprites[i];
                    var r = Resources.Load<Sprite>("Titles/" + category.name);
                    if (r != null)
                        if(Multilang.Instance.CurrentLanguage == "EN")
                        {
                            if(r.name == "Recipes" || r.name == "Collectibles")
                            {
                                r = Resources.Load<Sprite>("Titles/EN/" + category.name);
                                _title.sprite = r;

                            }
                            else
                            {
                                _title.sprite = r;
                            }
                        }
                        else
                        {
                            _title.sprite = r;
                        }
                    else
                        _title.sprite = Resources.Load<Sprite>("Titles/EmptyTitle");
                    _bottomRightCornerImage.enabled = t != category.childCount - 1; // in case there's only a page, second page never runs
                    Category = category.name;
                    SecondPage = "";
                    FirstPage = t.ToString();
                    child.GetComponent<RectTransform>().anchorMin = Vector2.zero + Vector2.right * _sideSpacing;
                    child.GetComponent<RectTransform>().anchorMax = Vector2.right / 2 + Vector2.up + Vector2.left * _spacing;
                    child.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    child.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
#if (UNITY_EDITOR)
                    EditorUtility.SetDirty(child);
#endif
                }
                else if (t != 0)
                {
                    _bottomRightCornerImage.enabled = t != category.childCount - 1;
                    SecondPage = t.ToString();
                    child.GetComponent<RectTransform>().anchorMin = Vector2.right / 2 + Vector2.right * _spacing;
                    child.GetComponent<RectTransform>().anchorMax = Vector2.right + Vector2.up + Vector2.left * _sideSpacing;
                    child.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    child.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
#if (UNITY_EDITOR)
                    EditorUtility.SetDirty(child);
#endif
                }
                else
                {
                    _bottomRightCornerImage.enabled = t != category.childCount - 1;
                    SecondPage = "";
                    child.GetComponent<RectTransform>().sizeDelta = child.GetComponent<RectTransform>().rect.size;
                    child.GetComponent<RectTransform>().anchorMin = child.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                    child.position = -child.GetComponent<RectTransform>().rect.size;
#if (UNITY_EDITOR)
                    EditorUtility.SetDirty(child);
#endif
                }
                if (t % 2 == 0 && t == category.childCount - 1)
                    pages += 1;
            }
            pages += category.childCount;
        }
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(Paging))]
public class PagingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var paging = target as Paging;
        int totalChild = 0;
        int totalDouble = 0;
        for (int i = 0; i < paging.transform.childCount; i++)
        {
            totalChild += paging.transform.GetChild(i).childCount;
            totalDouble += Mathf.CeilToInt((paging.transform.GetChild(i).childCount) / 2f).Max(0);
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Show"))
        {
            paging.Show();
            EditorUtility.SetDirty(paging);
        }
        if (GUILayout.Button("Hide"))
        {
            paging.Hide();
            EditorUtility.SetDirty(paging);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Share"))
        {
            paging.Share();
            EditorUtility.SetDirty(paging);
        }
        if (GUILayout.Button("Import"))
        {
            paging.Import();
            EditorUtility.SetDirty(paging);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh All"))
        {
            var r = paging.transform.GetComponentsInChildren<PagingCategory>();
            foreach (var c in r)
            {
                c.Refresh();
                EditorUtility.SetDirty(c);
            }
        }
        if (GUILayout.Button("Clear All"))
            paging.ClearAll();
        EditorGUILayout.LabelField("Category: " + paging.Category);
        EditorGUILayout.LabelField("First Page: " + paging.FirstPage);
        EditorGUILayout.LabelField("Second Page: " + paging.SecondPage);
        paging.DoublePage = EditorGUILayout.IntSlider("Double Page", paging.DoublePage, 0, totalDouble - 1);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
            paging.DoublePage = (paging.DoublePage - 1).Max(0);
        if (GUILayout.Button(">"))
            paging.DoublePage = (paging.DoublePage + 1).Min(totalDouble - 1);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
            paging.Save();
        if (GUILayout.Button("Load"))
        {
            paging.Load();
            EditorUtility.SetDirty(paging);
        }
        if (GUILayout.Button("Clear"))
            paging.Clear();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}
#endif