using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextEffectDialogue : MonoBehaviour
{
    #region Fields
    [SerializeField] private TMP_Text _text = null;
    private bool _enableEffect = false;
    #endregion Fields

    #region Properties
    public bool EnableEffect
    {
        get
        {
            return _enableEffect;
        }
        set
        {
            _enableEffect = value;
        }
    }
    #endregion Properties

    #region Methods
    void Start()
    {
        
    }

    void Update()
    {
        if(_enableEffect == true)
        {
            _text.ForceMeshUpdate();
            TMP_TextInfo textInfo = _text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (charInfo.isVisible == false)
                {
                    continue;
                }

                Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 orig = verts[charInfo.vertexIndex + j];
                   // charInfo.isVisible
                    verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin(Time.time * 2f + orig.x * 0.01f) * 40f, 0);
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                _text.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    
    }
    public void EnableEffectText()
    {
        _enableEffect = true;
    }

    public void DisableEffecText()
    {
        _enableEffect = false;
    }
    #endregion Methods
}
