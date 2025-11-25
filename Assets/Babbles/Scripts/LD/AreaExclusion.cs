using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaExclusion : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            Areas.Interdict(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            Areas.Interdict(false);
    }
}
