using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Player;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using Newtonsoft.Json;
using ClemCAddons.Utilities.Serializable;

public class KonamiCode : MonoBehaviour
{
    private List<KeyCode> _konamiCode = new List<KeyCode>()
        {
            KeyCode.UpArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.B,
            KeyCode.A};
    private List<KeyCode> _code = new List<KeyCode>();
    private KeyValuePair<InputType, dynamic>? input;

    // Update is called once per frame
    void Update()
    {
        var down = InputManager.anyKeyDown;
        if (down || InputManager.AnyInput())
        {
            KeyValuePair<InputType, dynamic>? r = InputManager.GetAnyInput();
            if (input.HasValue && r.HasValue && input.Value.ToString() == r.Value.ToString())
            {
                return;
            }
            if (r.HasValue && r.Value.Key == InputType.Button)
            {
                _code.Add(r.Value.Value);
                input = r;
            } else if(r.HasValue && r.Value.Key == InputType.GamepadButton)
            {
                input = r;
                GamepadButton t = r.Value.Value;
                switch (t)
                {
                    case GamepadButton.DPadLeft: _code.Add(KeyCode.LeftArrow); break;
                    case GamepadButton.DPadUp: _code.Add(KeyCode.UpArrow); break;
                    case GamepadButton.DPadDown: _code.Add(KeyCode.DownArrow); break;
                    case GamepadButton.DPadRight: _code.Add(KeyCode.RightArrow); break;
                    case GamepadButton.ActionBottom: _code.Add(KeyCode.A); break;
                    case GamepadButton.ActionRight: _code.Add(KeyCode.B); break;
                    default: _code.Add(KeyCode.None); break;
                }
            }
            else if (r.HasValue && r.Value.Key != InputType.GamepadAxis && r.Value.Key != InputType.MouseAxis)
            {
                input = null;
                _code.Add(KeyCode.None);
            }
            else if (down)
            {
                input = null;
                _code.Add(KeyCode.None);
            }
            while (_code.Count > 10)
                _code.RemoveAt(0);
            if (_code.SequenceEqual(_konamiCode))
            {
                var player = FindObjectOfType<CharacterMovement>();
                player.gameObject.AddComponent<PickupEffect>().Pickup(() =>
                {
                    if (SceneManager.GetActiveScene().name == "LD_Modules")
                    {
                        _code.Clear(); // so it doesn't do it multiple times
                        SceneManager.LoadSceneAsync(PlayerPrefs.GetString("Konami")).completed += LoadPosition;
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
        else
        {
            input = null;
        }
    }


    public static void LoadPosition(AsyncOperation operation)
    {
        var player = FindObjectOfType<CharacterMovement>();
        player.transform.position = JsonConvert.DeserializeObject<SerializableVector3>(PlayerPrefs.GetString("KonamiPosition"));
    }
}
