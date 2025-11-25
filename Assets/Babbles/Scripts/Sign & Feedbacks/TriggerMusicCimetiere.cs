using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMusicCimetiere : MonoBehaviour
{
    private bool _security = false;

    private void OnTriggerEnter(Collider other)
    {
        if(_security == false && other.tag == "Player")
        {
            AudioManager.Instance?.StopAmbiantWithFadeOut(2f);
            AudioManager.Start2DSound("M_CimetiereSon");
            AudioManager.Instance?.PlayAmbiant("M_CimetiereMusic");

            _security = true;
        }

    }
  

   
}
