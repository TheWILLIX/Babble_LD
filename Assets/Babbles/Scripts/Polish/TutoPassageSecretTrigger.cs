using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoPassageSecretTrigger : MonoBehaviour
{
    private bool _triggered = false;

    [SerializeField] private GameObject _portePassageSecret = null;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == ("Player") && _triggered == false)
        {
            UIManager.Instance.UIController.StartDialogue("Tuto_PassageSecret", false);
            _portePassageSecret.SetActive(false);
            _triggered = true;
        }
        
    }
}
