using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBaleineEpave : MonoBehaviour
{
    [SerializeField] private GameObject MamanBaleine;
    [SerializeField] private GameObject BebeBaleine;
    private void OnTriggerEnter(Collider other)
    {
        MamanBaleine.SetActive(true);
        BebeBaleine.SetActive(true);
    }
}
