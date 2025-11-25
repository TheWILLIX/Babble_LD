using ClemCAddons.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using UnityEngine.Serialization;

public class PushScript : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField, LabelOverride("Push Strength (per second)")] private float _pushStrength = 1f;
    [SerializeField, LabelOverride("Stabilization Strength")] private float _falling = 2f;
    [SerializeField] private bool _alternative = false;
    [SerializeField, DrawIf("_alternative", true, ComparisonType.Equals)] private float _upTime = 5;
    [SerializeField, DrawIf("_alternative", true, ComparisonType.Equals)] private float _downTime = 5;
    [Header("ExternalFactors")]
    [SerializeField] private float _boostObjectMultiplier;
    [SerializeField, FormerlySerializedAs("_boostObjectToSearch")] private string _componentToSearch;
    [Header("VFX")]
    [SerializeField] private float _transitionDuration = 1;
    [SerializeField] private Renderer _renderer = null;
    [SerializeField] private ParticleSystem _bulleBurstContinueParticles = null;
    [SerializeField] private bool _geyserActivated = true;
    [SerializeField] private AudioSource _geyserAudioSource = null;

    private float _alphaClip;

    private float _currentAlphaClip;

    private float _timer = 0;
    private bool _pushEnabled = true;

    private Collider[] _colliders = new Collider[] { };

    private static int _playerInsideOne = 0;
    private static bool _currentlyBoosted = false;

    public bool GeyserActivated
    {
        get
        {
            return _geyserActivated;
        }
        set
        {
            _geyserActivated = value;
        }
    }

    public bool PushEnabled { get => _pushEnabled; set => _pushEnabled = value; }
    public static int PlayerInsideOne { get => _playerInsideOne; }
    public static bool CurrentlyBoosted { get => _currentlyBoosted; set => _currentlyBoosted = value; }

    void Start()
    {
        _alphaClip = _renderer.material.GetFloat("_AlphaClip");
        _currentAlphaClip = _alphaClip;

        if (_alternative)
        {
            var r = _bulleBurstContinueParticles.main;
            var t = r.startSpeed;
            t.constant = 20f;
            r.startSpeed = t;
        }

        //Son de Geyser
        AudioManager.Start3DSound("S_Geyser", _geyserAudioSource, transform);

    }

    void OnTriggerEnter(Collider collider)
    {
        //Quick Fix because it kept searching the destroyed rocks and cause bugs in the script, so it's not placing the collider info in _colliders if it's a rock.
        if(collider.tag == "RockShard")
        {
            return;
        }
        else
        {
            _colliders = _colliders.Add(collider);

        }

        if (collider.CompareTag("Player") && _geyserActivated == true)
        {
            _playerInsideOne++;
        }
    }

    void FixedUpdate()
    {
        if (_pushEnabled)
        {
            if(_geyserActivated == true)
            {
                foreach (Collider collider in _colliders)
                {
                   
                        CharacterMovement _player = collider.GetComponent<CharacterMovement>();
                        if (_player != null)
                        {
                            var t = FindObjectsOfType<ExceptionItem>().ToList().Select(t => t.Type == _componentToSearch);
                            float boost = 1;
                            _currentlyBoosted = t != null && t.Count() > 0;
                            if (_currentlyBoosted)
                            {
                                boost = _boostObjectMultiplier;
                            }
                            var r = _player.GetComponent<Rigidbody>().velocity.y.Min(0);
                            _player.Push(_pushStrength * boost * ((r < 0).ToInt() * _falling).Max(1));
                            BabblesVibration.CustomVibration(0.1f, 0.1f); //Vibration Manette
                        }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        _colliders = _colliders.RemoveAll(other);
        CharacterMovement _player = other.GetComponent<CharacterMovement>();
        if (_player != null)
        {
            _player.Push(0);
            _playerInsideOne--;
        }

    }

    void Update()
    {
        _renderer.sharedMaterial.SetFloat("_AlphaClip", _currentAlphaClip);

        if (_alternative)
        {
            _timer += Time.deltaTime;
            if (_pushEnabled)
            {
                if (_timer > _downTime)
                {
                    _timer = 0;
                    _pushEnabled = false;
                    ClemCAddons.Utilities.Lerper.ConstantLerp(ref _currentAlphaClip, 5, _transitionDuration.Min(_upTime));

                    //Particles Desactivation
                    var r = _bulleBurstContinueParticles.main;
                    var t = r.maxParticles;
                    t = 0;
                    r.maxParticles = t;

                    _geyserAudioSource.Pause();
                }
            }
            else
            {
                if (_timer > _upTime)
                {
                    _timer = 0;
                    _pushEnabled = true;
                    ClemCAddons.Utilities.Lerper.ConstantLerp(ref _currentAlphaClip, _alphaClip, _transitionDuration.Min(_downTime));
                    
                    //Particles Activation
                    var r = _bulleBurstContinueParticles.main;
                    var t = r.maxParticles;
                    t = 1000;
                    r.maxParticles = t;

                    _geyserAudioSource.UnPause();

                }
            }
        }
        else
        {
            _pushEnabled = true;
        }
    }
}