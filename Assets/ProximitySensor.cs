using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class ProximitySensor : MonoBehaviour
{
    [SerializeField] private float _distance = 10f;
    private Transform _player;
    private Animator _animator;

    void Start()
    {
        _player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>().transform;
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(_player.Distance(transform) <= _distance)
        {
            _animator.SetTrigger("ProximitySensor");
        }
    }
}
