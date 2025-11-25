using ClemCAddons;
using ClemCAddons.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCameras : MonoBehaviour
{

    [SerializeField] private Transform _cam1;
    [SerializeField] private Transform _cam2;
    [SerializeField] private Material _material;

    private float currentMain = 0;
    private bool direction = false;

    void Start()
    {
    }
    void Update()
    {
        if(_material != null)
        {
            _material.SetFloat("_Blend", currentMain);
        }
    }

    public void MoveCam(Vector3 position)
    {
        bool pass = false;
        if (!direction)
        {
            if (_cam1 != null)
                _cam1.position = position;
            else
                pass = true;
        }
        else
        {
            if (_cam2 != null)
                _cam2.position = position;
            else
                pass = true;
        }
        if(!pass)
            Camera.main.transform.position = position;
    }

    public void SetCamLook(Vector3 position)
    {
        if (_cam1 == null || _cam2 == null)
            return;
        if (!direction)
            _cam1.LookAt(position);
        else
            _cam2.LookAt(position);
    }


    public void SwitchCam()
    {
        if (!direction)
        {
            Lerper.ConstantLerp(ref currentMain, 1, 2);
            direction = true;
        }
        else
        {
            Lerper.ConstantLerp(ref currentMain, 0, 2);
            direction = false;
        }
    }

}
