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

public class RecipePickUp : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _oldMode = true;

    [SerializeField] private string _pickupName = string.Empty;
    [SerializeField] private Recipe _recipe = null;

    [Header("UI Interact Button")]
    [SerializeField] private GameObject _UIInteractPosition = null;

    [SerializeField] private GameObject _objectToRemove = null;
    [SerializeField] private GameObject _objectToMove = null;
    [SerializeField] private MonoBehaviour[] _scriptsToDisable = null;
    [SerializeField] private float _buttonHintRightOffset = 2f;
    [SerializeField] private float _buttonHintUpOffset = 5f;

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
           
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = transform.GetComponent<Collider>();
            if (t.Character == null)
                t.Character = collider.transform;
            UIManager.Instance.UIController.UIInteractText.gameObject.SetActive(true);
            UIManager.Instance.UIController.UIInteractText.GetComponentInChildren<TMP_Text>().text = _pickupName;

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
                   
                        FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Recolting);
                        Pickup();
                    
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

        BabblesVibration.CustomVibration(0.15f, 0.3f); //Vibration

        _ressourcePickedUp = true;

       
        Debug.Log("Recette Picked Up");


        if(TryGetComponent<PickupEffect>(out var effect))
        {
            foreach(var script in _scriptsToDisable)
            {
                script.enabled = false;
            }
            effect.Pickup(() =>
            {
                pickupLock = false;
                _objectToRemove.SetActive(false);
            });
        }
        else
        {
            _objectToRemove.SetActive(false);
        }

        InventoryManager.Instance.UnlockRecipeByRecipe(_recipe);


        //ATTENTION CECI EST COD… DE FAÁON DURE CAR C'EST LA SEULE RECETTE TROUVABLE DANS LA DEMI
        UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$recetteTaken", true);

        UIManager.Instance.UIController.ObjectifsUpdate.NotificationObjective(EQuestType.SLOOP, 2);

        //Notification et Card du carnet a jour
    }


    private void CancelPickup()
    {
        FreshBulleAnimation.ResetHandAnimation();
        pickupLock = false;
    }

}
