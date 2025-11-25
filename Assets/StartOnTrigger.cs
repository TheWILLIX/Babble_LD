using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartOnTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GetComponent<AudioSource>().Play();
    }
}
