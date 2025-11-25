using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using ClemCAddons;
using System;

[RequireComponent(typeof(TMPro.TMP_Text))]
[ExecuteInEditMode]
public class MultilangNode : MonoBehaviour
{
    public SerializableDictionary<string, Multilang.Line> Lines;
    public string line;
    public int lineID;
    private TMPro.TMP_Text _text;
    private string currentText = "";

    void Awake()
    {
        _text = GetComponent<TMPro.TMP_Text>();
        if (Application.isPlaying)
        {
            _text.text = Multilang.GetLine(line);
            currentText = _text.text;
        }
    }
    
    void Update()
    {
        if(Application.isPlaying && currentText != _text.text)
        {
            _text.text = currentText;
        }
        if (!Application.isPlaying && Lines != null && line != null && Lines.ContainsKey(line))
        {
            _text.text = Lines[line].Lines.First().Value;
        }
    }

    public void Refresh()
    {
        Awake();
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(MultilangNode))]
public class MultilangNodeEditor : Editor
{
    private MultilangNode multilangNode;

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

    public void OnEnable()
    {
        multilangNode = (MultilangNode)target;
        var r = Load();
        if(r != "")
        {
            multilangNode.Lines = JsonUtility.FromJson<SerializableDictionary<string, Multilang.Line>>(r);
            multilangNode.lineID = multilangNode.Lines.Keys.ToArray().FindIndex(multilangNode.line);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if(multilangNode.lineID != -1)
        {
            if (Multilang.Instance.Filter.Contains(multilangNode.Lines[multilangNode.line].Category))
            {
                // label
                EditorGUILayout.LabelField("Target Filtered Out");
                return;
            }
        }
        var array = multilangNode.Lines.Keys.ToArray();
        var allowed = array.Where(m => !Multilang.Instance.Filter.Contains(multilangNode.Lines[m].Category)).ToArray();
        var id = multilangNode.lineID == -1 ? -1 : Array.FindIndex(allowed, t => t == array[multilangNode.lineID]);
        var a = EditorGUILayout.Popup(id, allowed);
        if (a == -1)
            return;
        var r = Array.FindIndex(array, t => t == allowed[a]);
        if(multilangNode.lineID != r)
        {
            multilangNode.lineID = r;
            if (multilangNode.lineID >= array.Length)
            {
                multilangNode.lineID = 0;
            }
            else
                multilangNode.line = multilangNode.Lines.Keys.ElementAt(multilangNode.lineID);
            EditorUtility.SetDirty(multilangNode);
            serializedObject.ApplyModifiedProperties();
        }
        if (GUILayout.Button("Refresh"))
        {
            OnEnable();
        }
    }
}
#endif