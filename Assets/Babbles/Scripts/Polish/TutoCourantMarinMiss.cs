using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoCourantMarinMiss : MonoBehaviour
{
    int triggerNumber = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == ("Player"))
        {
            Debug.Log(triggerNumber);
            switch(triggerNumber)
            {
                case 0:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin0", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                case 1:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin1", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                case 2:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin2", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                case 3:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin3", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                case 4:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin4", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                case 5:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin5", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;

                default:
                    UIManager.Instance.UIController.StartDialogue("Tuto_MissedCourantMarin6", true);
                    TutorialManager.Instance.MissedTutoCourantMarinTeleport();
                    break;
            }
            triggerNumber++;
        }
    }
}
