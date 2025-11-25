using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBaleineForet : MonoBehaviour
{

    [SerializeField] private GameObject _baleine = null;
    

    private void OnTriggerEnter(Collider other)
    {
        _baleine.SetActive(true);
    }
}
