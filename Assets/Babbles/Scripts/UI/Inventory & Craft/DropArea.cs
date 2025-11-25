using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropArea : MonoBehaviour
{
    public DropAreaType AreaType;


    public enum DropAreaType
    {
        Use,
        Shaker,
        Notebook,
        Cancel
    }

    void Start()
    {
        GetComponent<Image>().color = Color.clear;
    }
}
