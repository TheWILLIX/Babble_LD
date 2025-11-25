using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PagingCategory : MonoBehaviour
{
    [SerializeField] private List<string> _elements;

    public List<string> Elements { get => _elements; set => _elements = value; }


    public void Refresh()
    {
        transform.GetChild(0).GetComponent<FlowGrid>().enabled = false;
        if (Application.isPlaying)
        {
            for(int i = transform.childCount - 1; i >= 1; i--)
                Destroy(transform.GetChild(i).gameObject);
            for (int i = transform.GetChild(0).childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(0).GetChild(i).gameObject);
        }
        else
        {
            for(int i = transform.childCount - 1; i >= 1; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
            for (int i = transform.GetChild(0).childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(0).GetChild(i).gameObject);
        }
        foreach (var element in _elements)
        {
            try
            {
                Instantiate(Resources.Load("Cards/"+gameObject.name+"/"+element), transform.GetChild(0));
            }
            catch (Exception)
            {
                if(gameObject.name == "Collectibles")
                {
                    Instantiate(Resources.Load(element), transform.GetChild(0));
                }
                else
                    Debug.Log("????");
            }
        }
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(10, () =>
        {
            transform.GetChild(0).GetComponent<FlowGrid>().enabled = true;
            transform.GetChild(0).GetComponent<FlowGrid>().Refresh();
        });
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(PagingCategory))]
public class PagingCategoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var pagingCat = target as PagingCategory;
        if(GUILayout.Button("Refresh"))
            pagingCat.Refresh();
        base.OnInspectorGUI();
    }
}
#endif