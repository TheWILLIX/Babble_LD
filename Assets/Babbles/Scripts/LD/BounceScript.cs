using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Player;
using ClemCAddons;

public class BounceScript : MonoBehaviour
{

    [SerializeField] private float _bounceStrength = 10f;
    [SerializeField, LabelOverride("(optional) Delay Trigger")] private float _delayTrigger = 0.1f;
    [SerializeField] private GameObject _VFXElectricity = null;
    [SerializeField] private Transform _VFXPosition = null;
    [SerializeField] private Animator _bouncerAnimator = null;


    private float _time = 0;

    void OnTriggerEnter(Collider collider)
    {
        CharacterMovement _player = collider.GetComponent<CharacterMovement>();
        if (_player != null)
        {
            if(_time <= 0)
            {
                if (_player.Rigidbody.velocity.y < 0)
                {
                    _player.Bounce(_bounceStrength, true);
                    //ENDROIT OU FAUT METTRE L'ANIMATION DE LA MEDUSE COUSSIN 
                    Instantiate(_VFXElectricity, _VFXPosition.position, Quaternion.identity);
                    _time += _delayTrigger;
                   // transform.GetComponentInChildren<Animator>().SetBool("ShouldAnimate",true);
                    FreshBulleAnimation.StartPriorityAnimation("Primary_Jump", true);
                    BabblesVibration.CustomVibration(0.22f, 0.3f); //Vibration.
                    _bouncerAnimator.SetTrigger("BounceOn");

                    AudioManager.Start3DSound("S_RebondMedusePoulpe", transform);
                    //AudioManager.Start3DSound("S_RebondMedusePoulpe", transform);
                }
            }
        }
    }
    void Update()
    {
        _time = Mathf.Max(0, _time - Time.deltaTime);
    }
}
