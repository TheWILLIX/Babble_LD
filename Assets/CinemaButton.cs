using Luminosity.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiPosition;
    [SerializeField] private string _text;
    [SerializeField] private Cinema _cinema;
    [SerializeField] private int _movie;
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
            _cinema.Play(_movie);
        }
    }
}
