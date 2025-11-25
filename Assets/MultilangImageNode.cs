using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MultilangImageNode : MonoBehaviour
{
    private Image _target;
    private string _translatedLanguage;
    [HideInInspector]
    [SerializeField] private ImageTranslation[] _images = new ImageTranslation[1] { new ImageTranslation(new SerializableDictionary<string, Sprite>()) };
    [SerializeField] private int _spriteID;
    public ImageTranslation[] Image { get => _images; set => _images = value; }
    private Sprite _currentTarget;

    [Serializable]
    public struct ImageTranslation
    {
        public SerializableDictionary<string, Sprite> Translations;

        public ImageTranslation(SerializableDictionary<string, Sprite> translations) : this()
        {
            if (translations != null)
                Translations = translations;
            else
                Translations = new SerializableDictionary<string, Sprite>();
        }

        public Sprite GetSprite(string language)
        {
            if (Translations.ContainsKey(language))
                return Translations[language];
            else
            {
                Debug.LogError("Error in multilang, returning null sprite");
                return null;
            }
        }
    }

    void Awake()
    {
        _target = GetComponent<Image>();
        _translatedLanguage = Multilang.GetLanguage();
        _currentTarget = _images[_spriteID].GetSprite(_translatedLanguage);
        _target.sprite = _currentTarget;
    }

    public void SetSpriteID(int id)
    {
        _spriteID = id;
        _currentTarget = _images[_spriteID].GetSprite(_translatedLanguage);
        _target.sprite = _currentTarget;
    }

    void Update()
    {
        if (_translatedLanguage != Multilang.GetLanguage())
        {
            _translatedLanguage = Multilang.GetLanguage();
            _currentTarget = _images[_spriteID].GetSprite(_translatedLanguage);
            _target.sprite = _currentTarget;
        }
        if(_target.sprite != _currentTarget)
        {
            // is sprite somewhere in our translations?
            var i = Array.FindIndex(_images, t => t.Translations.ContainsValue(_target.sprite));
            if(i == -1)
            {
                _target.sprite = _currentTarget;
            }
            else
            {
                SetSpriteID(i);
            }
        }
    }
}

#if (UNITY_EDITOR)
[CustomEditor(typeof(MultilangImageNode))]
public class MultilangImageNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var availableLanguages = Multilang.Instance.Languages;
        var node = target as MultilangImageNode;
        var images = node.Image;
        bool dirty = false;
        // is an available language missing
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].Translations.Count != availableLanguages.Length)
            {
                // figure out which
                for (int a = 0; a < availableLanguages.Length; a++)
                {
                    var lang = availableLanguages[a];
                    if (!images[i].Translations.ContainsKey(lang))
                    {
                        // add it
                        images[i].Translations.Add(lang, null);
                        dirty = true;
                    }
                }
            }
        }
        for (int i = 0; i < images.Length; i++)
        {
            var image = images[i];
            EditorGUILayout.LabelField("Image " + i);
            EditorGUI.indentLevel++;
            foreach (var translation in image.Translations)
            {
                EditorGUILayout.LabelField(translation.Key);
                var r = EditorGUILayout.ObjectField(translation.Value, typeof(Sprite), false) as Sprite;
                if (r != translation.Value)
                {
                    image.Translations[translation.Key] = r;
                    dirty = true;
                    break;
                }
            }
            if (i == images.Length - 1)
            {
                if (GUILayout.Button("Add"))
                {
                    images = images.Add(new MultilangImageNode.ImageTranslation(new SerializableDictionary<string, Sprite>()));
                    dirty = true;
                }
            }
            EditorGUI.indentLevel--;
        }

        if (dirty)
        {
            node.Image = images;
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif