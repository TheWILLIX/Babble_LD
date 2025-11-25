using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepStraight : MonoBehaviour
{
    private Vector3 relativePosition;
    private Quaternion rotation;
    // Start is called before the first frame update
    void Start()
    {
        relativePosition = transform.position - transform.parent.position;
        rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.parent.position + relativePosition;
        transform.rotation = rotation;
    }
}
