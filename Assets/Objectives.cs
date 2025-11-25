using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    private List<Objective> _objectives = new List<Objective>();
    private TMPro.TMP_Text _text;

    private static Objectives _instance;
    public static Objectives Instance { get => _instance; }

    public class Objective
    {
        public string Name;
        public string Description;
    }

    void Awake()
    {
        _instance = this;
        _text = GetComponentInChildren<TMPro.TMP_Text>();
    }

    void Update()
    {
        if (_objectives.Count == 0)
        {
            _text.text = "";
            return;
        }
        _text.text = _objectives[0].Description;
    }
    
    public void AddObjective(Objective objective)
    {
        _objectives.Add(objective);
    }

    public void CompleteObjective(string name)
    {
        _objectives.RemoveAll(t => t.Name == name);
    }
}
