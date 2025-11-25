using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] private float _duration = 10;

    void Update()
    {
        _duration -= Time.deltaTime;
        if (_duration <= 0)
            Destroy(gameObject);
    }
}
