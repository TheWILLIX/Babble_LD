using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using ClemCAddons.Player;
using System;

public class PickupEffect : MonoBehaviour
{
    private bool pickingUp = false;
    private float perc = 0;
    private Vector3 originalPosition;
    private Transform targetTransform;
    private Action toDoInstead;


    public void Pickup()
    {
        perc = 0;
        pickingUp = true;
        if (TryGetComponent<ResourcePickUp>(out _) && GetComponent<ResourcePickUp>().ObjectToMove != null)
            targetTransform = GetComponent<ResourcePickUp>().ObjectToMove.transform;
        else
            targetTransform = transform;
        originalPosition = targetTransform.position;

    }

    public void Pickup(Action action)
    {
        perc = 0;
        pickingUp = true;
        if (TryGetComponent<ResourcePickUp>(out _) && GetComponent<ResourcePickUp>().ObjectToMove != null)
            targetTransform = GetComponent<ResourcePickUp>().ObjectToMove.transform;
        else
            targetTransform = transform;
        originalPosition = targetTransform.position;
        toDoInstead = action;
    }

    void Update()
    {
        if (!pickingUp)
            return;
        targetTransform.position = originalPosition + new Vector3(0,0-Vector3.Slerp(new Vector3(-2,0), new Vector3(2,0), perc).z);
        if(perc < 0.5)
        {
            targetTransform.localScale -= Vector3.one * perc * 3 * Time.deltaTime;
        }
        else
        {
            targetTransform.localScale -= Vector3.one * perc * 4 * Time.deltaTime;
        }
        if (targetTransform.localScale.Abs() != targetTransform.localScale)
        {
            targetTransform.localScale = Vector3.zero;
        }
        if (perc >= 1)
        {
            if (toDoInstead != null)
                toDoInstead.Invoke();
            else
                GetComponent<ResourcePickUp>()?.ObjectToRemove?.SetActive(false);
        }
        perc += Time.deltaTime;
    }
}
