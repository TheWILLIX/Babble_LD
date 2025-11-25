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

public class CollectiblePickUp : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _oldMode = true;

    [SerializeField] private string _collectibleName = "Souvenirs";
    private string _codeName = "Collectible";
    [SerializeField] private ELevelType _levelType = ELevelType.EPAVE;

    [Header("UI Interact Button")]
    [SerializeField] private GameObject _UIInteractPosition = null;

    [SerializeField] private GameObject _objectToRemove = null;
    [SerializeField] private GameObject _objectToMove = null;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable = null;
    [SerializeField] private float _buttonHintRightOffset = 2f;
    [SerializeField] private float _buttonHintUpOffset = 5f;

    [SerializeField] private Sprite _collectibleSprite = null;

    private static List<CollectiblePickUp> _allInRange = new List<CollectiblePickUp>();
    private bool _ressourcePickedUp = false;

    private Minigames _minigames;

    public GameObject ObjectToRemove { get => _objectToRemove; set => _objectToRemove = value; }
    public GameObject ObjectToMove { get => _objectToMove; set => _objectToMove = value; }


    private Collider _player;

    public GameObject UIPosition { get => _UIInteractPosition; }

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
            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = _collectibleName;

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
                foreach (CollectiblePickUp ressourcePickUp in _allInRange)
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
                    /*if (_resource.Name == "Algue")
                    {
                        var data = _minigames.GenerateMashData("Interaction", 0.2f, 0.3f,
                            () =>
                                {
                                    Pickup();
                                }, onPress: (f) => {
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
                    }*/
                    //else
                    //{
                        FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                        Pickup();
                    //}
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
          
            _player = null;
        }
    }

    private void Pickup()
    {

        BabblesVibration.CustomVibration(0.15f, 0.3f); //Vibration

        InteractButtonUpdater.Instance.EndInteraction(this);

        _ressourcePickedUp = true;


        if (_levelType == ELevelType.CLEMCABONUS)
        {
            UIManager.Instance.UIController.Paging.AddCard(_codeName + "." + _collectibleName, "Collectibles");
            var notificationData = new Notification.NotificationData()
            {
                Title = "DOM DOM DOM",
                Subtitle = "$Palmi.Subtitle",
                Description = "$Carnet.Description",
                KeyIcon = UIManager.Instance.UIController.HUDBank.NotebookKey,
                Background = UIManager.Instance.UIController.HUDBank.BackgroundCollectibles,
                ElementIcon = _collectibleSprite,
                ActionUponOpening = () =>
                {
                    _ = GameTools.DelayedCall(10, () =>
                    {
                        UIManager.Instance.UIController.BananeManager.ShowNotebookByCard(_codeName + "."+_collectibleName);
                    });
                }
            };
            Notification.TriggerNotification(notificationData, 1);
        }
        else
        {
            int collectibleNumber = collectibleNumber = InventoryManager.Instance.AddCollectible(_levelType);
            UIManager.Instance.UIController.Paging.AddCard(_codeName + "." + _levelType.ToString() + collectibleNumber, "Collectibles");

            int numberMaxOfCollectibles;

            switch (_levelType)
            {
                case ELevelType.EPAVE:
                    numberMaxOfCollectibles = 13;
                    break;

                case ELevelType.CIMETIERE:
                    numberMaxOfCollectibles = 3;
                    break;

                default:
                    numberMaxOfCollectibles = 10;
                    Debug.LogError("LevelType not Correct");
                    break;
            }
            var notificationData = new Notification.NotificationData()
            {
                Title = collectibleNumber.ToString() + "/" + numberMaxOfCollectibles.ToString(),
                Subtitle = "Souvenir n°" + collectibleNumber + " %%collectible.Subtitle%%",
                Description = "$Carnet.Description",
                KeyIcon = UIManager.Instance.UIController.HUDBank.NotebookKey,
                Background = UIManager.Instance.UIController.HUDBank.BackgroundCollectibles,
                ElementIcon = _collectibleSprite,
                ActionUponOpening = () =>
                {
                    _ = GameTools.DelayedCall(10, () =>
                    {
                        UIManager.Instance.UIController.BananeManager.ShowNotebookByCard(_codeName + "." + _levelType.ToString() + collectibleNumber);
                    });
                }
            };
            Notification.TriggerNotification(notificationData, 1);
            Debug.Log("Collectible Picked Up");
        }


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

        AudioManager.Start2DSound("S_RecolteCollectibles");
    }


    private void CancelPickup()
    {
        FreshBulleAnimation.ResetHandAnimation();
        pickupLock = false;
    }

}
