using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnStart : MonoBehaviour
{

    [SerializeField] private string _soundToStart = string.Empty;
    void Start()
    {
        AudioManager.Start3DSound(_soundToStart, transform);
    }


}
