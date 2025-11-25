using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Luminosity.IO;
using ClemCAddons.Minigames;
using ClemCAddons.NPCMovement;
using ClemCAddons.Utilities;
using ClemCAddons;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Yarn.Unity;

public class TriggerCoffreSkate : MonoBehaviour
{

    [Header("Mode")]
    [SerializeField] private bool _oldMode = true;

    [SerializeField] private string _searchingInteractionText = string.Empty;
    [SerializeField] private string _pickupText = string.Empty;

    [SerializeField] private Sprite _skateBulleSprite = null;


    [Header("UI Interact Button")]
    [SerializeField] private GameObject _UIInteractPosition = null;

    [SerializeField] private GameObject _objectToRemove = null;
    [SerializeField] private GameObject _objectToMove = null;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable = null;
    [SerializeField] private float _buttonHintRightOffset = 2f;
    [SerializeField] private float _buttonHintUpOffset = 5f;

    [Header("Other")]
    [SerializeField] private Animator _chestAnimator = null;
    [SerializeField] private string _cardName = null;

    private static List<ResourcePickUp> _allInRange = new List<ResourcePickUp>();
    private bool _ressourcePickedUp = false;

    private Minigames _minigames;

    #region Properties
    public GameObject ObjectToRemove { get => _objectToRemove; set => _objectToRemove = value; }
    public GameObject ObjectToMove { get => _objectToMove; set => _objectToMove = value; }


    private Collider _player;

    public GameObject UIInteractPosition { get => _UIInteractPosition; }

    private int pickupStep = 0;

    private bool truckFrigger = false; // lmaoooo fuck trigger!!!!

    // fuck, I'm tired.
    #endregion Properties

    void Start()
    {
        _minigames = FindObjectOfType<Minigames>();
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && _ressourcePickedUp == false && !truckFrigger)
        {
           
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = transform.GetComponent<Collider>();
            if (t.Character == null)
                t.Character = collider.transform;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = _searchingInteractionText;

            // Vector3 bottomLeft = UIManager.Instance.UIController.UIInteractText.textInfo.characterInfo[UIManager.Instance.UIController.UIInteractText.textInfo.characterCount - 1].topRight;


            // UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<Image>().gameObject.transform.position = bottomLeft;

            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
            _player = collider;
        }
    }

    private bool pickupLock = false;


    void Update() // replace trigger stay (unity bug)
    {
        if (_player == null)
            return;
        if (_player.tag == "Player")
        {
            if (_ressourcePickedUp == false)
            {
                
                if (pickupLock == true)
                    return;
                if (InputManager.GetButtonDown("Interaction"))
                {
                   
                        var data = _minigames.GenerateMashData("Interaction", 0.14f, 0f,
                            () =>
                            {
                                Pickup();
                            }, onPress: (f) => {
                                pickupStep = (f * 8).Round(); // in 7-8 steps
                                if(pickupStep > 3)
                                    AudioManager.Start3DSound("S_AlgueTir" + (pickupStep - 1), transform);
                                transform.GetChild(0).localPosition = transform.GetChild(0).localPosition.SetY(pickupStep * (0.8f / 8));
                                FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                                BabblesVibration.CustomVibration(0.2f, 0.05f);
                            },
                            shouldStopOnSuccess: true);
                        _minigames.SetTitle("Mash");
                        _minigames.StartMinigame(Minigame.mash, data);
                    
                    pickupLock = true;
                }
                if (_oldMode && UIManager.Instance.UIController.InventoryOpen == false || UIManager.Instance.UIController.CarnetOpen == false)
                {
                    Vector3 interactTextPosition = Camera.allCameras[0].WorldToScreenPoint(_UIInteractPosition.transform.position);
                    UIManager.Instance.UIController.UIInteractText.transform.position = interactTextPosition;
                }
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Player" && !truckFrigger)
        {
            _minigames.StopMinigame();
            pickupLock = false;

            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);
        
            
            _player = null;
        }
    }

    private void Pickup()
    {
        UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$skateTaken", true);

        //Start a Dialogue at the end 
        StartCoroutine(OpeningDelay());         //We Start the Pickup Coroutine 


        BabblesVibration.CustomVibration(0.15f, 0.3f); //Vibration



        UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);



      
        Debug.Log("Ressource Picked Up");
        //InventoryManager.Instance.AddItem(_resource, 1);
        if (TryGetComponent<PickupEffect>(out var effect))
        {
            foreach (var script in _scriptsToDisable)
            {
                script.enabled = false;
            }
            effect.Pickup(() =>
            {
                pickupLock = false;
               // _objectToRemove.SetActive(false);
            });
        }
        else
        {
           // _objectToRemove.SetActive(false);
        }
    }

    private IEnumerator OpeningDelay()
    {

        UIManager.Instance.UIController.Character.SetCanMove(false);   //We Lock the player in his position
        AudioManager.Start2DSound("S_ZeldaChest"); //We Start a sound
        FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.StaryEyes, 1.5f); //Bulle Animation
        _chestAnimator.SetTrigger("ChestOpen");

        //Custom Vibration -> To Define
        //In the Coroutine we start the chest animation, at the end we trigger all of this below -> _chestAnimator

        yield return new WaitForSeconds(1.5f);
        FreshBulleAnimation.StartFaceAnimation(FreshBulleAnimation.FaceType.Happy);

        //Sprite Skate Bulle appearing 
        _ressourcePickedUp = true;

        var notificationData = new Notification.NotificationData()
        {
            Title = _pickupText,
            Subtitle = "$SkateBulleGet.Subtitle",
            Description = "$SkateBulleGet.Description",
            KeyIcon = UIManager.Instance.UIController.HUDBank.InventoryInteractionKey,
            Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
            ElementIcon = _skateBulleSprite,
            ActionUponOpening = () =>
            {
                UIManager.Instance.UIController.BananeManager.ShowNotebookByCard(_cardName);
            }
        };
        Notification.TriggerNotification(notificationData, 0);

        UIManager.Instance.UIController.StartDialogue("Trigger_Coffre", true);

        yield return new WaitForSeconds(1.5f);

        UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjective(EQuestType.SKATE, 3);

        //Finished, back to the game
        UIManager.Instance.UIController.Character.SetCanMove(true);   //We unlock the player in his position

        FreshBulleAnimation.EndFaceAnimation();
       /* bool result;
        _storage.TryGetValue("$skateTaken", out result);
        Debug.Log("$skateTaken variable : " + result);*/

    }

    private void CancelPickup()
    {
        FreshBulleAnimation.ResetHandAnimation();
        pickupLock = false;
    }

}
