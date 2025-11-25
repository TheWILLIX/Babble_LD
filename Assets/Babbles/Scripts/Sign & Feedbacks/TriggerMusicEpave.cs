using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerMusicEpave: MonoBehaviour
{
    private bool _security = false;

    private void OnTriggerEnter(Collider other)
    {
        if(_security == false && other.tag == "Player")
        {
            AudioManager.Instance?.SwitchAmbiantTransition("M_Epave", 4f, 2f);
            _security = true;
        }

    }
  

   
}
