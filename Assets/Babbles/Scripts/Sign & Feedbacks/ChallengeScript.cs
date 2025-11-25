using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class ChallengeScript : MonoBehaviour
{


    [SerializeField] private GameObject[] _hotuChallenges = null;


    private void HotuChallengeActivation(int levelOfChallenge)
    {
        switch(levelOfChallenge)
        {
            case 1:
                //Teleport player to this pos 
                // _hotuChallenges[0].FindDeep("PlayerStartinPos").transform.position;

                _hotuChallenges[0].SetActive(true);
                ChallengeStart();
                break;
            case 2:
                _hotuChallenges[1].SetActive(true);
                ChallengeStart();
                break;
            case 3:
                _hotuChallenges[2].SetActive(true);
                ChallengeStart();
                break;

            default:
                break;
        }
    }

    private void ChallengeStart()
    {
        //Fade Out
        //Fade In Rapide
        //COMPTE A REBOURS (Coroutine)
        //Activation du timer qui met fin au mini-jeu
    }
}
