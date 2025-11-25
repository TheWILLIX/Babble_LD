using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
public class cameraMovement : MonoBehaviour
{



 

    float timeElapsed;
    [SerializeField] private float lerpDuration = 3;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private Transform _targetTransform = null;
    //private Vector3 targetPos = null;
     private Transform _startTransform = null;
    float startValue = 0;
    float endValue = 10;
    float valueToLerp;

    private void Start()
    {
        _startTransform = transform;
    }

    void Update()
    {
        if (timeElapsed < lerpDuration)
        {
            // valueToLerp = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            transform.position = Vector3.MoveTowards(transform.position, _targetTransform.position, speed * timeElapsed);
            //transform.position  = Vector3.Lerp(_startTransform.position, _targetTransform.position, timeElapsed / lerpDuration);
            //transform.position = Vector3.SmoothDamp(transform.position, targetPos)
            timeElapsed += Time.deltaTime;
        }
        else
        {
           // transform.position = _targetTransform.position;
        }
    }
}
