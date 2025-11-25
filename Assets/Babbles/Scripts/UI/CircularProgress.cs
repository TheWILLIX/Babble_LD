using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class CircularProgress : MonoBehaviour
{
    [SerializeField, Range(0,1)] private float perc;
    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    [SerializeField] private RectTransform mask;

    private static CircularProgress instance;


    void Start()
    {
        instance = this;
        perc = 0;
    }

    void Update()
    {
        right.gameObject.SetActive(perc != 0);
        if (perc < 0.5f)
        {
            left.gameObject.SetActive(false);
            mask.sizeDelta = mask.sizeDelta.SetX(50);
        }
        if (perc >= 0.5f)
        {
            left.gameObject.SetActive(true);
            mask.sizeDelta = mask.sizeDelta.SetX(100);
        }
        left.rotation = Quaternion.identity * Quaternion.Euler(0, 0, (1 - perc) * -360);
        if (perc >= 0.5f)
            right.rotation = Quaternion.identity;
        else
            right.rotation = Quaternion.identity * Quaternion.Euler(0, 0, (1 - perc + 0.5f) * -360);
    }


    public static void SetProgress(float progress)
    {
        instance.perc = progress;
        
    }

    public static void Setup(Vector2 position, Vector2 size, bool anchoredPosition = false)
    {
        if(anchoredPosition)
            instance.GetComponent<RectTransform>().anchoredPosition = position;
        else
            instance.GetComponent<RectTransform>().position = position;
        instance.GetComponent<RectTransform>().localScale = size / 100;
    }
}
