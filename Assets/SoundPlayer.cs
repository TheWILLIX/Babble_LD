using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    public void Play2D(string key)
    {
        AudioManager.Start2DSound(key);
    }

    public void Play3D(string key)
    {
        AudioManager.Start3DSound(key, GetComponent<AudioSource>(), doNotDestroy: true);
    }
}
