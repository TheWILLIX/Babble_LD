using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Player;
using ClemCAddons;
using ClemCAddons.Utilities;

public class MeduseFake : MonoBehaviour
{

    [SerializeField] private float _bounceStrength = 10f;
    [SerializeField, LabelOverride("(optional) Delay Trigger")] private float _delayTrigger = 0.1f;
    [SerializeField] private GameObject _VFXElectricity = null;
    [SerializeField] private Transform _VFXPosition = null;
    [SerializeField] private Animator _bouncerAnimator = null;
    [SerializeField] private GameObject _objectToHide = null;
    [SerializeField] private Transform _VFXDisapearPosition = null;

    [SerializeField] private Transform _uiPosition = null;



    private int _amountOfTimeJumpedOnMeduse = 1;


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

                    AudioManager.Start2DSound("S_RebondMedusePoulpe");
                    //AudioManager.Start3DSound("S_RebondMedusePoulpe", transform);
                    
                    var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
                    t.Target = _uiPosition.GetComponent<Collider>();


                    if (_amountOfTimeJumpedOnMeduse == 4)
                    {
                        UIManager.Instance.UIController.StartDialogueOverride("Trigger_Meduse_" + _amountOfTimeJumpedOnMeduse, false);
                        StartCoroutine(MeduseLeaveDelay());
                    }
                    else if(_amountOfTimeJumpedOnMeduse <= 3)
                    {
                        UIManager.Instance.UIController.StartDialogueOverride("Trigger_Meduse_" + _amountOfTimeJumpedOnMeduse, false);
                    }
                    
                    _amountOfTimeJumpedOnMeduse++;
                }
            }
        }
    }
    void Update()
    {
        _time = Mathf.Max(0, _time - Time.deltaTime);
    }

    IEnumerator MeduseLeaveDelay()
    {
        yield return new WaitForSeconds(2.5f);
        UIManager.Instance.UIController.DialogueStopped();
        Transform vfxPos = transform;
        vfxPos.transform.parent = null;
        VFXSpawner.Spawn("Skin", vfxPos.position);

        _objectToHide.SetActive(false);

        AudioManager.Start3DSound("S_BulleLook", _VFXDisapearPosition);  //Son a démarrez sur le VFXDisapearPosition
        


    }
}
