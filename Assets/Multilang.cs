using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System;
using UnityEditor;
using System.Linq;
using System.IO;
using Utilities;

public class Multilang : MonoBehaviour
{
    [SerializeField] private string[] _languages = new string[] { "FR", "EN" };
    [SerializeField] private string _currentLanguage = "FR";
    [SerializeField]
    [HideInInspector]
    private SerializableDictionary<string,Line> _lines = new SerializableDictionary<string,Line>();
    [SerializeField]
    [HideInInspector]
    private string[] _categories = new string[] { "Default" };
    [SerializeField]
    [HideInInspector]
    private List<string> filter = new List<string>();
    private static Multilang _instance;
    public static Multilang Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            _instance = FindObjectOfType<Multilang>();
            if (_instance != null)
                return _instance;
            var go = new GameObject("Multilang", typeof(Multilang));
            _instance = go.GetComponent<Multilang>();
            _instance.InitFromFile();
            return _instance;
        }
    }

    public string CurrentLanguage { get => _currentLanguage; }
    public SerializableDictionary<string, Line> Lines { get => _lines; set => _lines = value; }
    public string[] Languages { get => _languages; set => _languages = value; }
    public string[] Categories { get => _categories; set => _categories = value; }
    public List<string> Filter { get => filter; set => filter = value; }

    [Serializable]
    public class Line
    {
        public string Category = "Default";
        public SerializableDictionary<string, string> Lines;
        public Line()
        {
            Lines = new SerializableDictionary<string, string>();
        }
    }

    private void InitFromFile()
    {
        var r = Load();
        if (r == "")
            return;
        Lines = JsonUtility.FromJson<SerializableDictionary<string, Multilang.Line>>(r);
        // create categories
        _categories = new string[] { "Default" }.Concat(Lines.Values.Select(x => x.Category)).Distinct().ToArray();
    }

    private string Load()
    {
        if (File.Exists(Application.dataPath
                     + "/Translation.dat"))
        {
            return File.ReadAllText(Application.dataPath
                     + "/Translation.dat");
        }
        return "";
    }

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            UpdateLanguageFromSettings();
        }
        else
            Destroy(gameObject);
    }

    public static string TryInstantTranslation(string data)
    {
        if (data.StartsWith("$") || data.Contains("%%"))
        {
            if (data.StartsWith("$"))
            {
                data = Multilang.GetLine(data.Substring(1));
            }
            if (data.Contains("%%"))
            {
                // isolate all instances of words surrounded by %%
                var matches = System.Text.RegularExpressions.Regex.Matches(data, @"%%(.*?)%%");
                for (int i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    var key = match.Value.Substring(2, match.Value.Length - 4);
                    var value = Multilang.GetLine(key);
                    data = data.Replace(match.Value, value);
                }
            }
        }
        return data;
    }

    public static string GetLine(string ID)
    {
        if (_instance._lines.ContainsKey(ID))
            return Instance._lines[ID].Lines[Instance._currentLanguage];
        Debug.LogError("Missing translation: " + ID);
        return "";
    }

    public static string GetLanguage()
    {
        return Instance._currentLanguage;
    }

    public static void UpdateLanguage(string language)
    {
        Instance._currentLanguage = language;
        Instance.RefreshNodes();
    }

    public static void UpdateLanguageFromSettings()
    {
        Instance._currentLanguage = Instance._languages[PlayerPrefs.GetInt("SettingLanguage")];
        Instance.RefreshNodes();
    }

    private void RefreshNodes()
    {
        var nodes = FindObjectsOfType<MultilangNode>();
        foreach (var n in nodes)
            n.Refresh();
        var languageUpdaters = FindObjectsOfType<LanguageUpdater>();
        foreach (var l in languageUpdaters)
            l.Refresh();
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(Multilang))]
public class MultilangEditor : Editor
{
    private Multilang multilang;
    private int creatingCategory = -1;
    private bool filterOpen = false;
    private bool filterCreation = false;
    
    private string Load()
    {
        if (File.Exists(Application.dataPath
                     + "/Translation.dat"))
        {
            return File.ReadAllText(Application.dataPath
                     + "/Translation.dat");
        }
        return "";
    }

    private void Save(string content)
    {
        File.WriteAllText(Application.dataPath
                     + "/Translation.dat", content);
    }

    public void OnEnable()
    {
        if (targets == null || targets.Length == 0)
            return;
        multilang = (Multilang)target;
        EnableActions(multilang);
    }

    public void EnableActions(Multilang multilang)
    {
        var r = Load();
        if (r == "")
            return;
        multilang.Lines = JsonUtility.FromJson<SerializableDictionary<string, Multilang.Line>>(r);
        // set categories
        var categories = new List<string>
        {
            "Default"
        };
        foreach (var l in multilang.Lines)
        {
            if (!categories.Contains(l.Value.Category))
                categories.Add(l.Value.Category);
        }
        multilang.Categories = categories.ToArray();
    }

    private void OnDirty(bool setDirty = true, bool serialize = true, Multilang target = null)
    {
        if (target == null)
            target = multilang;
        if(setDirty)
            EditorUtility.SetDirty(target);
        if (serialize)
        {
            var r = JsonUtility.ToJson(target.Lines);
            Save(r);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_languages"));

        EditorGUILayout.Space();

        // is a window open
        if (MultilangWindow.IsWindowOpen())
        {
            EditorGUILayout.LabelField("Multilang is open as a window");
            return;
        }

        // display as a window
        if (GUILayout.Button("Display as a window"))
        {
            MultilangWindow.ShowWindow(multilang, this);
        }

        EditorGUILayout.Space();

        DrawInspector(multilang);
        
        serializedObject.ApplyModifiedProperties();
    }

    public void DrawInspector(Multilang multilang)
    {
        EditorGUILayout.Space();
        DisplayFilters(multilang);


        EditorGUILayout.Space();

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Line"))
        {
            multilang.Lines.Add("undefined key", new Multilang.Line());
            OnDirty(target: multilang);
        }

        // iterate over categories
        for (int i = 0; i < multilang.Categories.Length; i++)
        {
            if (multilang.Filter.Contains(multilang.Categories[i]))
                continue;
            // get line element ID of category
            var lines = new List<int>();
            for (int j = 0; j < multilang.Lines.Count; j++)
            {
                var line = multilang.Lines.ElementAt(j);
                if (line.Value.Category == multilang.Categories[i])
                {
                    lines.Add(j);
                }
            }
            if (lines.Count == 0)
                continue;
            // label category
            EditorGUILayout.LabelField(multilang.Categories[i], EditorStyles.boldLabel);
            // add indent
            EditorGUI.indentLevel++;
            DisplayLineConnection(multilang, lines);
            EditorGUI.indentLevel--;
        }
    }

    private void DisplayFilters(Multilang multilang)
    {
        // display text on the right of the button
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleRight;
        var mainrect = GUILayoutUtility.GetRect(new GUIContent("Filter"), GUI.skin.button);
        if (GUI.Button(mainrect, "Filter"))
        {
            filterOpen = !filterOpen;
        }
        var st = new GUIStyle(style);
        st.fontSize = Mathf.RoundToInt(st.fontSize * 1.5f);
        st.padding = new RectOffset(0, 4, 0, 4);
        EditorGUI.LabelField(mainrect, filterOpen ? "-" : "+", st);
        if (filterOpen)
        {
            EditorGUILayout.BeginHorizontal();
            int currentLine = 0;
            bool skipNext = false;
            // categories are displayed if they are not in the filter
            foreach (var category in multilang.Categories)
            {
                if (multilang.Filter.Contains(category))
                    continue;
                if (skipNext)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                // place label at the same position as the button
                var rect = GUILayoutUtility.GetRect(new GUIContent(category), GUI.skin.button);
                if (GUI.Button(rect, category))
                {
                    multilang.Filter.Add(category);
                    OnDirty(target: multilang, serialize: false);
                }
                // place label
                EditorGUI.LabelField(rect, "\u2716", style);
                currentLine++;
                if (currentLine >= 4)
                {
                    currentLine = 0;
                    skipNext = true; // using skipnext avoids cutting for the last button
                }
            }
            var localStyle = new GUIStyle(GUI.skin.button);
            localStyle.fontSize = Mathf.RoundToInt(localStyle.fontSize * 1.25f);
            localStyle.padding = new RectOffset(3, 0, 0, 3);
            if (GUILayout.Button("+", localStyle, GUILayout.Height(20), GUILayout.Width(20)))
            {
                filterCreation = true;
            }
            EditorGUILayout.EndHorizontal();
            if (filterCreation)
            {
                EditorGUILayout.BeginHorizontal();
                if (multilang.Filter.Count == 0)
                {
                    // there is nothing here
                    EditorGUILayout.LabelField("No category left");
                }
                // list all filtered categories
                currentLine = 0;
                skipNext = false;
                foreach (var category in multilang.Filter)
                {
                    if (skipNext)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    var rect = GUILayoutUtility.GetRect(new GUIContent(category), GUI.skin.button);
                    if (GUI.Button(rect, category))
                    {
                        multilang.Filter.Remove(category);
                        OnDirty(target: multilang);
                        break;
                    }
                    EditorGUI.LabelField(rect, "+", st);
                    currentLine++;
                    if (currentLine >= 4)
                    {
                        currentLine = 0;
                        skipNext = true;
                    }
                }
                // cancel button
                if (GUILayout.Button("Cancel"))
                {
                    filterCreation = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DisplayLineConnection(Multilang multilang, List<int> targetLines)
    {
        // foreach in targetlines
        foreach(var i in targetLines)
        {
            var line = multilang.Lines.ElementAt(i);
            EditorGUILayout.BeginHorizontal();
            var txt = EditorGUILayout.DelayedTextField(line.Key);
            if (txt != line.Key)
            {
                if (multilang.Lines.ContainsKey(txt))
                {
                    // notify that it already exists
                    EditorUtility.DisplayDialog("Can't rename "+line.Key+" to "+txt, "This line already exists", "Ok");
                    return;
                }
                multilang.Lines[txt] = line.Value;
                multilang.Lines.Remove(line.Key);
                OnDirty(target: multilang);
                return;
            }
            if (GUILayout.Button("Remove"))
            {
                multilang.Lines.Remove(line.Key);
                OnDirty(target: multilang);
                return;
            }
            // selector
            var r = creatingCategory == i ? multilang.Categories.Length : EditorGUILayout.Popup(Array.IndexOf(multilang.Categories, line.Value.Category), multilang.Categories.Append("New").ToArray());
            if (r == multilang.Categories.Length)
            {
                creatingCategory = i;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                var newCat = EditorGUILayout.DelayedTextField("New Category");
                if (newCat != "" && newCat != "New Category")
                {
                    if (multilang.Categories.Contains(newCat))
                    {
                        if(EditorUtility.DisplayDialog("This category already exists", "Do you want to set your line to that category instead?", "Yes", "Cancel"))
                        {
                            line.Value.Category = newCat;
                            creatingCategory = -1;
                            OnDirty(target: multilang);
                            return;
                        }
                        else
                            return;
                    }
                    multilang.Categories = multilang.Categories.Append(newCat).ToArray();
                    line.Value.Category = newCat;
                    creatingCategory = -1;
                    OnDirty(target: multilang);
                }
                if (GUILayout.Button("\u2716"))
                {
                    creatingCategory = -1;
                }
            }
            else
            {
                line.Value.Category = multilang.Categories[r];
                multilang.Lines[line.Key] = line.Value;
                OnDirty(target: multilang);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            for (int j = 0; j < line.Value.Lines.Count || j < multilang.Languages.Length; j++)
            {
                KeyValuePair<string, string> lineV;
                if (j < line.Value.Lines.Count && j < multilang.Languages.Length)
                {
                    lineV = line.Value.Lines.ElementAt(j);
                }
                else if (j < multilang.Languages.Length)
                {
                    line.Value.Lines.Add(multilang.Languages[j], "");
                    lineV = new KeyValuePair<string, string>(multilang.Languages[j], "");
                    OnDirty(target: multilang);
                }
                else
                {
                    var e = line.Value.Lines.ElementAt(j);
                    line.Value.Lines.Remove(e.Key);
                    OnDirty(target: multilang);
                    break;
                }
                EditorGUILayout.BeginHorizontal();
                txt = EditorGUILayout.TextField(lineV.Key);
                if (txt != lineV.Key)
                {
                    line.Value.Lines[txt] = lineV.Value;
                    line.Value.Lines.Remove(lineV.Key);
                    EditorGUILayout.EndHorizontal();
                    GUI.FocusControl(null);
                    OnDirty(target: multilang);
                    break;
                }
                txt = EditorGUILayout.TextField(lineV.Value);
                if (txt != lineV.Value)
                {
                    line.Value.Lines[lineV.Key] = txt;
                    OnDirty(target: multilang);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
    }
}

// multilang window
public class MultilangWindow : EditorWindow
{
    private bool triedRefresh;
    private Multilang multilang;
    private MultilangEditor multilangEditor;
    private Vector2 scrollPosition = Vector2.zero;
    

    void OnGUI()
    {
        if(multilangEditor == null || multilang == null)
        {
            // try refresh
            if (!triedRefresh)
            {
                triedRefresh = true;
                if(multilang == null)
                {
                    multilang = FindObjectOfType<Multilang>();
                }
                if (multilangEditor == null)
                {
                    multilangEditor = FindObjectOfType<MultilangEditor>();
                    if (multilangEditor != null)
                        multilangEditor.EnableActions(multilang);
                }
            }
            // label
            EditorGUILayout.LabelField("The editor is not initialized and the auto refresh failed");
            if(GUILayout.Button("Try to refresh"))
            {
                triedRefresh = false;
            }
            if(GUILayout.Button("Initialize a new instance (can break if a second one opens)"))
            {
                if(multilang == null)
                {
                    multilang = new Multilang();
                    multilang.Invoke("Awake", 0);
                }
                if (multilangEditor == null)
                {
                    multilangEditor = CreateInstance<MultilangEditor>();
                    multilangEditor.EnableActions(multilang);
                }
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            return;
        }
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        multilangEditor.DrawInspector(multilang);
        EditorGUILayout.EndScrollView();
    }

    public static bool IsWindowOpen()
    {
        return EditorWindow.HasOpenInstances<MultilangWindow>();
    }

    // add open window in menu
    [MenuItem("Translation/Multilang")]
    public static void ShowWindow()
    {
        ShowWindow(null, null);
    }

    public static void ShowWindow(Multilang multilang, MultilangEditor editor)
    {
        if (EditorWindow.HasOpenInstances<MultilangWindow>())
            return;
        var r = EditorWindow.GetWindow(typeof(MultilangWindow));
        r.titleContent = new GUIContent("Multilang");
        r.SetField("multilang", multilang);
        r.SetField("multilangEditor", editor);
    }
}


#endif
[Serializable]
 public class SerializableDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>, ISerializationCallbackReceiver
 {
     [SerializeField]
     private List<TKey> keys = new List<TKey>();
     
     [SerializeField]
     private List<TValue> values = new List<TValue>();
     
     // save the dictionary to lists
     public void OnBeforeSerialize()
     {
         keys.Clear();
         values.Clear();
         foreach(KeyValuePair<TKey, TValue> pair in this)
         {
             keys.Add(pair.Key);
             values.Add(pair.Value);
         }
     }
     
     // load dictionary from lists
     public void OnAfterDeserialize()
     {
         this.Clear();
 
         if(keys.Count != values.Count)
             throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
 
         for(int i = 0; i < keys.Count; i++)
             this.Add(keys[i], values[i]);
     }
 }

