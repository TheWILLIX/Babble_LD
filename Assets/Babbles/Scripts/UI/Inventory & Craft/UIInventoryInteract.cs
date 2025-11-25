using ClemCAddons.Utilities;
using Luminosity.IO;
using UnityEngine;
using ClemCAddons;
using ClemTimer = ClemCAddons.Utilities.Timer;

public class UIInventoryInteract : MonoBehaviour
{

    [Header("Main")]



    [SerializeField] private GameObject _inventorySelectWindow = null;


    private string _ressourceSelected = string.Empty;
    private EObjectType _objectSelected;

    private bool _selectionIsRessource = false;

    [Header("Object & Ressource Usage")]
    [SerializeField] private UISlider _slider;
    [SerializeField] private UISlider _sliderObjects;
    [SerializeField] private Transform _usageContaineer = null;

    [SerializeField] private Transform _playerMesh;

    [SerializeField] private GameObject _meduseCoussinPrefab;
    [SerializeField] private float _distanceOfMeduseCoussinSpawn;
    [SerializeField] private GameObject _meduseLightPrefab;
    [SerializeField] private float _distanceOfMeduseLightSpawn;

    [SerializeField] private GameObject _fleurPrefab;
    [SerializeField] private float _distanceOfFleurSpawn;

    [SerializeField] private LayerMask _groundLayer;

    private GameObject _spawnedMeduse;

    private float _delay = 0;



    #region Methods
    void Start()
    {
        _slider.Callback = UseItem;
    }

    void Update()
    {
        _delay = Mathf.Max(0, _delay - Time.deltaTime);
        if ((InputManager.GetButton("UI_Down") || InputManager.GetButton("UI_Up"))&& _delay <= 0)
        {
            _delay += 0.5f;
            _sliderObjects.EnabledInputs = _slider.EnabledInputs;
            _slider.EnabledInputs = !_slider.EnabledInputs;
        }
    }

    private void DisableTransparent(GameObject gameObject)
    {
        GameObject r = gameObject.FindDeep("BU_build");
        GameObject res = gameObject.FindDeep("BU_final");
        if (r != null)
        {
            res.SetActive(true);
            r.SetActive(false);
        }
    }

    #region Object & Ressource Usage
    private void MeduseCoussin()
    {
        Vector3 meduseCoussinPos = _playerMesh.position + (_playerMesh.forward * _distanceOfMeduseCoussinSpawn);
        _spawnedMeduse = Instantiate(_meduseCoussinPrefab, meduseCoussinPos, Quaternion.identity, _usageContaineer);
        ClemTimer.StartTimer(7175, (int)(Time.smoothDeltaTime * 1000), MeduseCoussinUpdate, true);
    }

    public void MeduseCoussinUpdate()
    {
        if (InputManager.GetButtonDown("Interaction"))
        {
            DisableTransparent(_spawnedMeduse);
            _spawnedMeduse = null;
            ClemTimer.EndTimer(7175);
        }
        if(_spawnedMeduse != null)
        {
            Vector3 meduseCoussinPos = _playerMesh.position + (_playerMesh.forward * _distanceOfMeduseCoussinSpawn);
            meduseCoussinPos.y += 1.5f;
            meduseCoussinPos.y -= GameTools.FindGround(meduseCoussinPos, 0, 3, _groundLayer);
            _spawnedMeduse.transform.position = meduseCoussinPos;
        } else
        {
            ClemTimer.EndTimer(7175); // just a precaution
        }
    }

    private void Fleur()
    {
        Vector3 fleurPos = _playerMesh.position + (_playerMesh.forward * _distanceOfFleurSpawn);
        Instantiate(_fleurPrefab, fleurPos, Quaternion.identity, _usageContaineer);
    }

    #endregion Object & Ressource Usage

    public void UseItem(int item)
    {
        string result = "";
        switch (item)
        {
            case 0:
                result = "meduse";
                break;
            case 1:
                result = "fruit";
                break;
            case 2:
                result = "fleur";
                break;
            case 3:
                result = "algue";
                break;
            case 4:
                result = "crevette";
                break;
            case 5:
                result = "poulpe";
                break;
            default:
                Debug.LogError("Item n°"+item+" doesn't exist");
                break;
        }
        SelectRessource(result);
    }

    public void SelectRessource(string ressourceName)
    {
        _selectionIsRessource = true; //MAKE THE DIFFERENCE BETWEEN A RESSOURCE AND A CRAFTED OBJECT (ITS A RESSOURCE)

        _ressourceSelected = ressourceName; //PUT IN MEMORY THE RESSOURCE NAME FOR LATER PURPOSE IF THE PLAYER DECIDE TO USE THAT RESSOURCE

        _inventorySelectWindow.SetActive(true); //OPEN THE SELECT WINDOW
    }

    public void SelectObject(string craft)
    {
        _selectionIsRessource = false; //MAKE THE DIFFERENCE BETWEEN A RESSOURCE AND A CRAFTED OBJECT (ITS A CRAFTED OBJECT)

        switch (craft)
        {
            case "medusefruit":
            case "fruitmedeuse":
            case "meduseCoussin":
                _objectSelected = EObjectType.MEDUSECOUSSIN;
                break;

            case "medusealgue":
            case "alguemeduse":
            case "meduseGourmande":
                _objectSelected = EObjectType.MEDUSEGOURMANDE;
                break;

            case "fleuralgue":
            case "alguefleur":
            case "campementFeu":
                _objectSelected = EObjectType.SUSHI;
                break;
            default:
                Debug.LogError("Craft string in Button Click is incorrect");
                break;
        }

        _inventorySelectWindow.SetActive(true); //OPEN THE SELECT WINDOW
    }

    public void CancelSelect()
    {
        _selectionIsRessource = false;
        _inventorySelectWindow.SetActive(false);
    }

    /*public void UseRessourceOrObject()
    {
        if(_selectionIsRessource == true)
        {
            switch(_ressourceSelected)
            {
                case "meduse":
                    if(DialogueManager.Instance.IsInGiveSituation == false) //UTILISE L'ITEM NORMALEMENT
                    {
                        var r = Instantiate(_meduseLightPrefab, _playerMesh.parent.parent);
                        r.transform.localPosition = r.transform.localPosition.SetY(_distanceOfMeduseLightSpawn);
                    }
                    else //DONNE LA RESSOURCE A UN PNJ
                    {
                        DialogueManager.Instance.GiveRessourcePNJ(ERessourceType.MEDUSE);
                        UIManager.Instance.UIController.CloseInventory();
                    }
                  //  UIManager.Instance.UIController.InventoryManager.RemoveRessources(ERessourceType.MEDUSE, 1);
                    break;

                case "fruit":
                    if (DialogueManager.Instance.IsInGiveSituation == false) //UTILISE L'ITEM NORMALEMENT
                    {
                        Debug.Log("Eat Fonction");
                    }
                    else //DONNE LA RESSOURCE A UN PNJ
                    {
                        DialogueManager.Instance.GiveRessourcePNJ(ERessourceType.FRUIT);
                        UIManager.Instance.UIController.CloseInventory();
                    }
                  //  UIManager.Instance.UIController.InventoryManager.RemoveRessources(ERessourceType.FRUIT, 1);

                    break;

                case "algue":
                    if (DialogueManager.Instance.IsInGiveSituation == false) //UTILISE L'ITEM NORMALEMENT
                    {
                        Debug.Log("Eat Fonction");

                    }
                    else  //DONNE LA RESSOURCE A UN PNJ
                    {
                        DialogueManager.Instance.GiveRessourcePNJ(ERessourceType.ALGUE);
                        UIManager.Instance.UIController.CloseInventory();
                    }
                   // UIManager.Instance.UIController.InventoryManager.RemoveRessources(ERessourceType.ALGUE, 1);
                    break;
               
                case "fleur": 
                    if (DialogueManager.Instance.IsInGiveSituation == false) //UTILISE L'ITEM NORMALEMENT
                    {
                        Debug.Log("Fire Fonction");
                        Fleur();
                    }
                    else  //DONNE LA RESSOURCE A UN PNJ
                    {
                        DialogueManager.Instance.GiveRessourcePNJ(ERessourceType.FLEUR);
                    }
//                    UIManager.Instance.UIController.InventoryManager.RemoveRessources(ERessourceType.FLEUR, 1);
                    UIManager.Instance.UIController.CloseInventory();
                    break;
            }

           // UIManager.Instance.UIController.UpdateInventory();
            _inventorySelectWindow.SetActive(false);


        }
        else
        {
         //   UIManager.Instance.UIController.InventoryManager.RemoveCraftObject(_objectSelected);


            switch (_objectSelected)
            {
                case EObjectType.MEDUSECOUSSIN:
                    //FAIT APPARAITRE UNE MEDUSE COUSSIN
                    MeduseCoussin();
                    _inventorySelectWindow.SetActive(false);
                    Debug.Log("Meduse Coussin Used");
                    break;

                case EObjectType.MEDUSEGOURMANDE:
                    //FAIT APPARAITRE UNE MEDUSE GOURMANDE
                    Debug.Log("Meduse Gourmande Used");
                    break;

                case EObjectType.SUSHI:
                    //FAIT APPARAITRE UN CAMPEMENT
                    Debug.Log("Campement Feu Used");
                    break;


            }

            _inventorySelectWindow.SetActive(false);
    //        UIManager.Instance.UIController.UpdateInventory();

        }
    }*/

    #endregion Methods
}
