using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsApplier : MonoBehaviour
{
    void Awake()
    {
        if (PlayerPrefs.HasKey("SettingFOV") && TryGetComponent<Camera>(out var cam))
        {
            cam.fieldOfView = PlayerPrefs.GetFloat("SettingFOV");
        }
        if (PlayerPrefs.HasKey("SettingYAxis") && TryGetComponent<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>(out var tpscam))
        {
            tpscam.CamInversed = PlayerPrefs.GetInt("SettingYAxis") == 0 ? false : true;
        }
        if (PlayerPrefs.HasKey("SettingMouseKeyboardMode"))
        {
            Cursor.visible = PlayerPrefs.GetInt("SettingMouseKeyboardMode") == 1;
            if (TryGetComponent<BananeManager>(out var bananeManager))
                bananeManager.ControlMode = PlayerPrefs.GetInt("SettingMouseKeyboardMode") == 0 ? BananeManager.InventoryControlMode.Slots : BananeManager.InventoryControlMode.Cursor;
        }
        if (PlayerPrefs.HasKey("SettingJumpInputMode") && TryGetComponent<ClemCAddons.Player.CharacterMovement>(out var playerMovement))
        {
            playerMovement.JumpInputMode = (ClemCAddons.Player.CharacterMovement.JumpInputs)PlayerPrefs.GetInt("SettingJumpInputMode");
        }
        if (PlayerPrefs.HasKey("SettingThermometerVisibility") && TryGetComponent<SeumUI>(out var seumUI))
        {
            seumUI.ThermometerMode = (SeumUI.E_ThermometerMode)PlayerPrefs.GetInt("SettingThermometerVisibility");
        }
    }

    // expensive, use once only
    public static void ApplyAll()
    {
        var all = Resources.FindObjectsOfTypeAll<SettingsApplier>();
        foreach(var a in all)
        {
            a.Apply();
        }
    }

    public void Apply()
    {
        Awake();
    }
}
