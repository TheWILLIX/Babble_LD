using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using ClemCAddons.Utilities.Serializable;

public class SimpleTeleport : MonoBehaviour
{

    [SerializeField] private string _keyToTeleport = null;

    [Header("Debug")]
    [SerializeField] private bool _endLevelTeleport = false;
    [SerializeField] private Item _itemForEndLevelTeleport;


    private void Update()
    {

        if (InputManager.GetButtonDown(_keyToTeleport) == true)
        {
            UIManager.Instance?.UIController.Character.Teleport(transform.position);
            Debug.Log("Teleporting player !");

            if (_endLevelTeleport == true){
                InventoryManager.Instance.AddItem(_itemForEndLevelTeleport);
            }
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (UIManager.Instance == null)
                return;
            var pos = UIManager.Instance.UIController.Character.transform.position;

            PlayerPrefs.SetString("F9Pos",JsonConvert.SerializeObject(new SerializableVector3(pos.x, pos.y, pos.z)));
            PlayerPrefs.Save();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            var r = PlayerPrefs.GetString("F9Pos");
            if (r == "")
                return;
            var pos = JsonConvert.DeserializeObject<SerializableVector3>(r);
            UIManager.Instance?.UIController.Character.Teleport(pos);
        }
     

    }


}
