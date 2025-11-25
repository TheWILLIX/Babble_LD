using System.Collections;
using System.Collections.Generic;
using ClemCAddons.CameraAndNodes;
using UnityEngine;


public class CameraDistanceChanger : MonoBehaviour
{

    [SerializeField] private float _distance = 200f;
    private Camera _camera = null;


    private void Start()
    {
        _camera = UIManager.Instance.UIController.Character.TpsCamera.GetComponent<Camera>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            _camera.farClipPlane = _distance;
        }
    }

}
