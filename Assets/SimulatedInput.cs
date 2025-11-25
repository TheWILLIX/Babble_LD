using ClemCAddons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedInput : MonoBehaviour
{
    private ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport _tpsCam;
    private ClemCAddons.Player.CharacterMovement _characterMovement;
    private bool _interrupt;
    private bool _previouslyMoving = true;
    [SerializeField] private int _jumpingFrameDelay = 2;
    [SerializeField] private float _speedOfCamTilt = 0.3f;
    [SerializeField] private float _camTiltAngle = 0.27f;
    [SerializeField] private float _tiltingTolerance = 10f;


    void Start()
    {
        _tpsCam = FindObjectOfType<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>();
        _characterMovement = FindObjectOfType<ClemCAddons.Player.CharacterMovement>();
        _tpsCam.BreakSimulatedInput += Interrupt;
    }

    void Update()
    {
        if (_characterMovement.Jumping) // true on frame
        {
            JumpingAnimation();
            JumpingDelay();
        }
        //if (_characterMovement.IsMoving && !_previouslyMoving)
        //{
        //   TurningAnimation();
        //}
        _previouslyMoving = _characterMovement.IsMoving;
    }

    private void Interrupt(object sender, EventArgs e)
    {
        _interrupt = true;
    }

    private async void JumpingAnimation()
    {
        var direction = _tpsCam.Position.SetY(0).normalized * _tpsCam.Position.magnitude;
        direction = Vector3.Slerp(direction, direction.magnitude * Vector3.up, _camTiltAngle);
        for (float i = 0; i < 1 && Time.deltaTime > 0; i += Time.deltaTime * _speedOfCamTilt)
        {
            if (_interrupt)
            {
                _interrupt = false;
                break;
            }
            var y = direction.Distance(_tpsCam.Position) * (direction.y - _tpsCam.Position.y).Sign() * _speedOfCamTilt;
            if (direction.Distance(_tpsCam.Position) < _tiltingTolerance * Time.deltaTime)
                break;
            _tpsCam.SimulatedInput = Vector2.zero.SetY(y);
            await System.Threading.Tasks.Task.Delay((Time.deltaTime * 1000).Round());
        }
        _tpsCam.SimulatedInput = Vector2.zero;
    }

    private async void JumpingDelay()
    {
        _tpsCam.Delay = _jumpingFrameDelay;
        await System.Threading.Tasks.Task.Delay(500);
        while(_tpsCam.Delay > 0 && Application.isPlaying)
        {
            await System.Threading.Tasks.Task.Delay((Time.deltaTime * 1000).Round());
            _tpsCam.Delay--;
        }
        _tpsCam.Delay = 0;
    }


    /* private async void TurningAnimation()
     {
         var direction = _characterMovement.Rigidbody.velocity.SetY(0).normalized;
         for (float i = 0; i < 1 && Time.deltaTime > 0; i += Time.deltaTime)
         {
             if (_interrupt)
                 break;
             var origin = _tpsCam.transform.forward.SetY(0).normalized;
             var left = Vector3.Dot(origin.Left(), direction);
             var right = Vector3.Dot(origin.Right(), direction);
             float x = origin.Distance(direction);
             if (left > right)
                 x *= -1;
             _tpsCam.SimulatedInput = Vector2.zero.SetX(x);
             await System.Threading.Tasks.Task.Delay((Time.deltaTime * 1000).Round());
         }
         _tpsCam.SimulatedInput = Vector2.zero;
     }*/
}
