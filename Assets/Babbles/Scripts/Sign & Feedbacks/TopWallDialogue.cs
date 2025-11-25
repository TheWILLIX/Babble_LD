using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopWallDialogue : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            UIManager.Instance.UIController.StartDialogue("TopWallBreak", false);
        }
    }

}
