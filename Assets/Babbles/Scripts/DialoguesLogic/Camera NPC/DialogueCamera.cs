using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using ClemCAddons.Player;
using ClemCAddons.Utilities;
using ClemCAddons.CameraAndNodes;

public class DialogueCamera : MonoBehaviour
{
    [SerializeField] private GameObject _targetNPC;
    [SerializeField] private float _transitionSpeed = 0.5f;
    [SerializeField] private float _distance = 5f;
    private CharacterMovement _player;
    private bool _isCam = false;
    private Camera _previousCamera;

    void Start()
    {
        _player = FindObjectOfType<CharacterMovement>();
    }

    void Update()
    {
       
        if (!_isCam && _player.transform.Distance(_targetNPC) < _distance && DialogueManager.Instance.IsInDialog)
        {
            _isCam = true;
            _previousCamera = Camera.main;
            CameraTools.SwitchCamera(Camera.main, GetComponent<Camera>(), _transitionSpeed);
        }
        else if (_isCam && !DialogueManager.Instance.IsInDialog)
        {
            _isCam = false;
            CameraTools.SwitchCamera(GetComponent<Camera>(), _previousCamera, _transitionSpeed);
        }
    }
}