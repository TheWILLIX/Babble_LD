using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouleOuverture : MonoBehaviour
{
    [SerializeField] private Animator _mouleAnime;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _mouleAnime.SetTrigger("Open");
        }
    }


}