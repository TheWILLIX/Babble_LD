using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoucouAnimTrigger : MonoBehaviour
{

    [SerializeField] private Animator[] _npcAnimator = null;

    [SerializeField] private bool _npcDoesCoucouToBulle = true;
    [SerializeField] private bool _bulleDoesCoucouToNpc = true;

    private bool _isJeunes = false;

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.tag == "Player")
        {
            if(_bulleDoesCoucouToNpc == true)
            {
                FreshBulleAnimation.Coucou();
            }

            if(_npcDoesCoucouToBulle == true)
            {
                foreach(Animator animator in _npcAnimator)
                {
                    animator.SetTrigger("CoucouOn");
                }

            }


           

         
        }
    }
}
