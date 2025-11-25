using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(TMPro.TMP_Text))]
public class MultilangAutoNode : MonoBehaviour
{
    private TMPro.TMP_Text _text;
    private string _lang;
    private string _prev = "";

    void Awake()
    {
        _text = GetComponent<TMPro.TMP_Text>();
        _lang = Multilang.GetLanguage();
        if (_text.text.StartsWith("$") || _text.text.Contains("%%"))
            AutoFill(_text.text);
    }

    public void Setup(string _prev)
    {
        this._prev = _prev;
    }

    void Update()
    {
        if (_text.text.StartsWith("$") || _text.text.Contains("%%"))
        {
            AutoFill(_text.text);
        }
        if (_prev != "" && _lang != Multilang.GetLanguage())
        {
            AutoFill(_prev);
        }
    }

    private void AutoFill(string tag)
    {
        if (tag.StartsWith("$"))
        {
            _prev = tag;
            _lang = Multilang.GetLanguage();
            _text.text = Multilang.GetLine(tag.Substring(1));
        }
        if (tag.Contains("%%"))
        {
            _prev = tag;
            _lang = Multilang.GetLanguage();
            // isolate all instances of words surrounded by %%
            var matches = System.Text.RegularExpressions.Regex.Matches(tag, @"%%(.*?)%%");
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var key = match.Value.Substring(2, match.Value.Length - 4);
                var value = Multilang.GetLine(key);
                tag = tag.Replace(match.Value, value);
            }
            _text.text = tag;
        }
    }
}
