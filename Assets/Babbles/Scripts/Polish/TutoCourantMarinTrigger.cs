using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoCourantMarinTrigger : MonoBehaviour
{
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == ("Player") && _triggered == false)
        {
            _triggered = true;
            UIManager.Instance.UIController.StartDialogue("Tuto_CourantMarinSortie", true);
        }

    }
}
