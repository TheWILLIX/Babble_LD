using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour
{

    [SerializeField] private float _speed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up * _speed * Time.deltaTime);
    }
}
