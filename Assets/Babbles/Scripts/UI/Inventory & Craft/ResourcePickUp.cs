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

public class ResourcePickUp : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _oldMode = true;

    [Header("Type Of Ressource")]
    [SerializeField, FormerlySerializedAs("_ressource")] private Item _resource = null;

    [Header("UI Interact Button")]
    [SerializeField] private GameObject _UIInteractPosition = null;

    [SerializeField] private GameObject _objectToRemove = null;
    [SerializeField] private GameObject _objectToMove = null;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable = null;
    [SerializeField] private float _buttonHintRightOffset = 2f;
    [SerializeField] private float _buttonHintUpOffset = 5f;

    private static List<ResourcePickUp> _allInRange = new List<ResourcePickUp>();
    private bool _ressourcePickedUp = false;

    private Minigames _minigames;

    public GameObject ObjectToRemove { get => _objectToRemove; set => _objectToRemove = value; }
    public GameObject ObjectToMove { get => _objectToMove; set => _objectToMove = value; }


    private Collider _player;

    public GameObject UIInteractPosition { get => _UIInteractPosition; }
    public Item Resource { get => _resource; }

    private int pickupStep = 0;

    private bool truckFrigger = false; // lmaoooo fuck trigger!!!!

    // fuck, I'm tired.

    void Start()
    {
        _minigames = FindObjectOfType<Minigames>();
    }
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player" && _ressourcePickedUp == false && !truckFrigger)
        {
            _allInRange.Add(this);
            if (!_oldMode)
            {
                InteractButtonUpdater.Instance.StartInteraction(this);
                _player = collider;
                return;
            }
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = transform.GetComponent<Collider>();
            if (t.Character == null)
                t.Character = collider.transform;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = _resource.CleanName;

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
                var closest = _allInRange[0];
                float closestDistance = _allInRange[0].transform.Distance(_player.transform);
                foreach (ResourcePickUp ressourcePickUp in _allInRange)
                {
                    if (ressourcePickUp.transform.Distance(_player.transform) < closestDistance)
                    {
                        closest = ressourcePickUp;
                        closestDistance = ressourcePickUp.transform.Distance(_player.transform);
                    }
                }
                if (closest != this)
                    return;
                if (pickupLock == true)
                    return;
                if (InputManager.GetButtonDown("Interaction"))
                {
                    if (_resource.Name == "Algue")
                    {
                        var data = _minigames.GenerateMashData("Interaction", 0.2f, 0.3f,
                            () =>
                                {
                                    Pickup();
                                }, onPress: (f) => {
                                    AudioManager.Start3DSound("S_AlgueTir" + (f * 5 + 1).Round(), transform);
                                    pickupStep = (f * 5).Round(); // in 5 steps
                                    transform.GetChild(1).localPosition = transform.GetChild(1).localPosition.SetY(pickupStep * (0.1f / 5));
                                    FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                                    BabblesVibration.CustomVibration(0.2f, 0.05f);
                                },
                            shouldStopOnSuccess: true);
                        _minigames.SetTitle("Mash");
                        _minigames.StartMinigame(Minigame.mash, data);
                    }
                    else if (_resource.Name == "Fleur")
                    {
                        var data = _minigames.GenerateTimingData("Interaction", 0.5f,
                            () =>
                                {
                                    truckFrigger = true;
                                    Pickup();
                                    FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                                },
                            CancelPickup, shouldStopOnSuccess: true);
                        _minigames.SetTitle("Timing");
                        _minigames.StartMinigame(Minigame.timing, data);
                    }
                    else
                    {
                        FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                        Pickup();
                    }
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
            _allInRange.Remove(this);
            _minigames.StopMinigame();
            pickupLock = false;
            if(_oldMode)
                UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(false);
            else
                InteractButtonUpdater.Instance.EndInteraction(this);
            if (_resource.Name == "MEDUSE")
            {
                GetComponent<Medusa>().EnableRoaming();
            }
            _player = null;
        }
    }

    private void Pickup()
    {

        BabblesVibration.CustomVibration(0.15f, 0.3f); //Vibration

        InteractButtonUpdater.Instance.EndInteraction(this);

        _ressourcePickedUp = true;
        var notificationData = new Notification.NotificationData()
        {
            Title = _resource.CleanName,
            Subtitle = "$Resource.Subtitle",
            Description = "$Resource.Description",
            KeyIcon = UIManager.Instance.UIController.HUDBank.InventoryInteractionKey,
            Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
            ElementIcon = _resource.Sprite,
            ActionUponOpening = () =>
            {
                UIManager.Instance.UIController.OpenInventory(true, false, _resource.Name);
            }
        };
        Notification.TriggerNotification(notificationData, 0);
        Debug.Log("Ressource Picked Up");
        InventoryManager.Instance.AddItem(_resource, 1);
        if(TryGetComponent<PickupEffect>(out var effect))
        {
            foreach(var script in _scriptsToDisable)
            {
                script.enabled = false;
            }
            effect.Pickup(() =>
            {
                _allInRange.Remove(this);
                pickupLock = false;
                _objectToRemove.SetActive(false);
            });
        }
        else
        {
            _objectToRemove.SetActive(false);
        }


        AudioManager.Start3DSound(_resource.Audio, transform, true);

        AudioManager.Start2DSound("S_Recolte");
    }


    private void CancelPickup()
    {
        FreshBulleAnimation.ResetHandAnimation();
        pickupLock = false;
    }

}
