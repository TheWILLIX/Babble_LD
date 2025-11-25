using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
public class MusicNPCTriggerHotu : MonoBehaviour
{

    [SerializeField] private InteractNPC _npcInteraction = null;
    [SerializeField] private float _maxVolume = 0.5f;
    [SerializeField, Range(0, 1)] private float _minDistancePercentage = 0.3f;
    private SphereCollider _sphereCollider = null;
    private bool _talkActivated = false;

    private Collider _player;


    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && (EpaveCustomBehaviour.HasInstance == false || EpaveCustomBehaviour.Instance.IsInChallenge == false))
        {
            _player = collider;

        }


    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player" && (EpaveCustomBehaviour.HasInstance == false || EpaveCustomBehaviour.Instance?.IsInChallenge == false))
        {
            _player = null;
            Debug.Log("Oscour?  ");


            if (AudioManager.Instance.IsPlayingChallengeMusic == true)
            {
                Debug.Log("IsWorking Yeaaah");
                AudioManager.Instance?.SwitchAmbiantTransition("M_Epave", 2f , 1f);
                AudioManager.Instance.IsPlayingChallengeMusic = false;

            }

        }

    }

    private void Update()
    {
        /*if (_player == null)
        return;
        if (_player.tag == "Player")
        {

            if(UIManager.Instance.UIController.DialogueManager.IsInDialog == true)
            {
                _talkActivated = true;
            }

            if(_talkActivated == false)
            {
                var thirtyPercent = _sphereCollider.radius * _minDistancePercentage;
                float perc = 1 - (transform.position.Distance(_player.transform.position).Max(thirtyPercent) / _sphereCollider.radius - _minDistancePercentage) / (1 - _minDistancePercentage);
                perc *= _maxVolume;

                if (perc <= 0.01) //Small Security to avoid having the npc audio source near 0 but never exactly at 0
                {
                    perc = 0;
                }

                AudioManager.Instance.NPCNearbyVolume = perc;
            }
            else
            {
                var thirtyPercent = _sphereCollider.radius * _minDistancePercentage;
                float perc = 1 - (transform.position.Distance(_player.transform.position).Max(thirtyPercent) / _sphereCollider.radius - _minDistancePercentage) / (1 - _minDistancePercentage);
                perc *= _maxVolume;

                if(perc <= 0.01) //Small Security to avoid having the npc audio source near 0 but never exactly at 0
                {
                    perc = 0;
                }

                AudioManager.Instance.NPCThemeVolume = perc;
            }

           // Debug.Log(transform.position.Distance(_player.transform.position).Max(thirtyPercent)+": " +perc);
        }*/
    }


}
