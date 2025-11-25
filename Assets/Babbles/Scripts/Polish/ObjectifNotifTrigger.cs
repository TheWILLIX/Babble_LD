using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectifNotifTrigger : MonoBehaviour
{

    [SerializeField] private int _objectifNumber = 3;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjective(EQuestType.NONE, _objectifNumber);
        }
    }

}
