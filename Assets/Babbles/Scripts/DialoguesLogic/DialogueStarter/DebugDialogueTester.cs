using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class DebugDialogueTester : MonoBehaviour
{

    [SerializeField] private string _dialogueName = "Marline_WakeOral";
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.GetButtonDown("InverseCam"))
        {
            UIManager.Instance.UIController.StartDialogue(_dialogueName, false);
        }
    }
}
