using ClemCAddons.Player;
using ClemCAddons.Utilities.Serializable;
using Luminosity.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalButton : MonoBehaviour
{
    [SerializeField] private GameObject _uiPosition;
    [SerializeField] private string _text;
    public GameObject UIPosition { get => _uiPosition; }
    public string Text { get => _text; }
    private bool isIn;
    

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractButtonUpdater.Instance.StartInteraction(this);
            isIn = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InteractButtonUpdater.Instance.EndInteraction(this);
            isIn = false;
        }
    }

    void Update()
    {
        if (isIn && InputManager.GetButtonDown("Interaction"))
        {
            isIn = false; // so it doesn't do it multiple times
            GetComponentInChildren<AudioSource>().Play();
            var player = FindObjectOfType<CharacterMovement>();
            player.gameObject.AddComponent<PickupEffect>().Pickup(() =>
            {
                if (SceneManager.GetActiveScene().name == "LD_Modules")
                {
                    SceneManager.LoadSceneAsync(PlayerPrefs.GetString("Konami")).completed += KonamiCode.LoadPosition;
                }
                else
                {
                    PlayerPrefs.SetString("Konami", SceneManager.GetActiveScene().name);
                    var pos = (player.transform.position + Vector3.up);
                    PlayerPrefs.SetString("KonamiPosition", JsonConvert.SerializeObject(
                        new SerializableVector3(pos.x, pos.y, pos.z)));
                    SceneManager.LoadScene("LD_Modules");
                }
            });
        }
    }
}
