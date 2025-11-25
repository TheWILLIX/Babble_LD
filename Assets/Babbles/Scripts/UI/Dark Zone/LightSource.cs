using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkZone;

public class LightSource : MonoBehaviour
{
    [SerializeField] private bool _useCustomRadiuses;
    [SerializeField] private int[] _radiuses = new int[] { 500, 300 };

    void Start()
    {
        if (_useCustomRadiuses)
        {
            DarkFunctions.StartLight(transform, DarkFunctions.LightType.external, true, _radiuses);
        } else
        {
            DarkFunctions.StartLight(transform, DarkFunctions.LightType.external, true);
        }
    }
}
