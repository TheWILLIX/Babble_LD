using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Database/Engine/SoundData")]
public class SoundData : ScriptableObject
{

    [SerializeField] private string _key = string.Empty;

    [SerializeField] private AudioClip _clip = null;

    [Range(0.0f, 1.0f)]
    [SerializeField] private float _volume = 1;

    [Range(-3.0f, 3.0f)]
    [SerializeField] private float _pitch = 1;

    [SerializeField] private bool _loop = false;

    [Space]
    [SerializeField] private bool _uniqueSound = false;

    [Header("Pitch Variation")]
    [SerializeField] private bool _pitchVariation = false;

    [Range(0f, 3.0f)]
    [SerializeField] private float _pitchMinimum = 0.9f;
    [Range(0f, 3.0f)]
    [SerializeField] private float _pitchMaximum = 1.1f;
 

    [Space]
    [Header("Multiple Clip Random")]
    [SerializeField] private bool _multipleSound = false;
    [SerializeField] private AudioClip[] _additionalClips = null;



    public string Key => _key;

    public AudioClip Clip => _clip;

    public float Volume  => _volume;

    public float Pitch => _pitch;

    public bool Loop => _loop;

    public bool PitchVariation => _pitchVariation;

    public float PitchMinimum => _pitchMinimum;

    public float PitchMaximum => _pitchMaximum;


    public bool MultipleSound => _multipleSound;

    public AudioClip[] AdditionalClips => _additionalClips;


    public bool UniqueSound => _uniqueSound;


}
