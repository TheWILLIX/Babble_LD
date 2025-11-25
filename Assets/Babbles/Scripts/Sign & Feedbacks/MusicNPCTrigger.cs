using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
public class MusicNPCTrigger : MonoBehaviour
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
                switch (_npcInteraction.NPCData.name)
                {                        

                    case "Jeunes":
                    AudioManager.Instance?.StartNearbyNPCMusic("M_JeunesNearby");
                    AudioManager.Instance?.TryPausingAmbiant(0.5f);
                    _player = collider;
                        break;

                    case "Marline":
                    AudioManager.Instance?.StartNearbyNPCMusic("M_MarlineNearby");
                    AudioManager.Instance?.TryPausingAmbiant(0.5f);
                    _player = collider;
                        break;

                    case "Urfe":
                    AudioManager.Instance?.StartNearbyNPCMusic("M_UrfeNearby");
                    AudioManager.Instance?.TryPausingAmbiant(0.5f);
                    _player = collider;
                    break;
                }
            
        }


    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player" && (EpaveCustomBehaviour.HasInstance == false || EpaveCustomBehaviour.Instance?.IsInChallenge == false))
        {
            _player = null;

                switch (_npcInteraction.NPCData.name)
                {
                    case "Jeunes":
                    AudioManager.Instance?.TryResumingAmbiant(ELevelType.EPAVE, 1.5f);
                        _talkActivated = false;
                        break;

                    case "Marline":
                        AudioManager.Instance?.TryResumingAmbiant(ELevelType.CIMETIERE, 1.5f);
                        _talkActivated = false;
                        break;

                    case "Urfe":
                        AudioManager.Instance?.TryResumingAmbiant(ELevelType.EPAVE, 1.5f);
                        _talkActivated = false;
                        break;
                }
            
        }

    }

    private void Update()
    {
        if (_player == null)
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
        }
    }


}
