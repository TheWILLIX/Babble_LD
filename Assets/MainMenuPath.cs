using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MainMenuPath : MonoBehaviour
{
    [SerializeField] private bool _showPath = true;
    private MainMenuCameras _mainMenuCameras;

    void Awake()
    {
        _mainMenuCameras = FindObjectOfType<MainMenuCameras>();
    }

    void Update()
    {
        if (!Application.isPlaying && _showPath)
            Tracer(GetComponent<BezierSpline>(), transform.GetChild(0).GetComponent<BezierSpline>());
    }

    public void Play(Action finished)
    {
        PlayThrough(finished);
    }

    private async void PlayThrough(Action finished)
    {
        var spline = GetComponent<BezierSpline>();
        var subSpline = transform.GetChild(0).GetComponent<BezierSpline>();
        var passes = 1000f * spline.CurveCount;
        for (int i = 0; i < passes && Application.isPlaying; i++)
        {
            _mainMenuCameras.MoveCam(spline.GetPoint(i / passes));
            _mainMenuCameras.SetCamLook(subSpline.GetPoint(i / passes));
            await System.Threading.Tasks.Task.Delay(10);
#if(UNITY_EDITOR)
            if (Input.GetKeyDown(KeyCode.Space))
                break;
#endif
        }
        if (Application.isPlaying)
        {
            _mainMenuCameras.SwitchCam();
            finished.Invoke();
        }
    }

    private void Tracer(BezierSpline spline, BezierSpline target)
    {
        for(int i = 0; i < 100; i++)
        {
            Debug.DrawLine(spline.GetPoint(i / 100f), target.GetPoint(i/100f), Color.red);
        }
    }
}
