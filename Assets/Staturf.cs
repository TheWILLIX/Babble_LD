using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staturf : MonoBehaviour
{
    [SerializeField] private bool defaultState;
    private static bool currentState;
    private bool localState;

    void Awake()
    {
        Active(defaultState);
    }

    void Update()
    {
        if(currentState != localState)
        {
            localState = currentState;
            Active(currentState != defaultState);
        }
    }

    public static void ShowStaturf()
    {
        currentState = true;
    }

    private void Active(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }
}
