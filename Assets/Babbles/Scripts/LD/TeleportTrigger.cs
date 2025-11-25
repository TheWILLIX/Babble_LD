using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using UnityEngine.UI;
using TMPro;


public class TeleportTrigger : MonoBehaviour
{


   [SerializeField] private GameObject _neighboorTeleporter = null;

    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
          //  GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>().Target = transform.parent.GetComponent<Collider>();
         //   UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
        }
    }


    private void OnTriggerStay(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (InputManager.GetButtonDown("Jump"))
            {
                collider.transform.position = _neighboorTeleporter.transform.position;
            }


        }
    }


    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player")
        {
       /*     var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            if (t.Target != null && t.transform == transform.parent)
                t.Target = null;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);*/
        }
    }
}
