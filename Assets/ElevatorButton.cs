using Luminosity.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiPosition;
    [SerializeField] private string _text;
    [SerializeField] private Elevator _elevator;
    [SerializeField] private bool _directionMode;
    [SerializeField, DrawIf("_directionMode", true, ComparisonType.Equals)] private bool _direction;
    [SerializeField, DrawIf("_directionMode", false, ComparisonType.Equals)] private int _target;
    public GameObject UIPosition { get => _uiPosition; }
    public string Text { get => _text; }
    private bool isIn;
    

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractButtonUpdater.Instance.StartInteraction(this);
            isIn = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractButtonUpdater.Instance.EndInteraction(this);
            isIn = false;
        }
    }

    void Update()
    {
        if (isIn && InputManager.GetButtonDown("Interaction"))
        {
            if (_directionMode)
                _elevator.Move(_direction);
            else
                _elevator.Move(_target);
        }
    }
}
