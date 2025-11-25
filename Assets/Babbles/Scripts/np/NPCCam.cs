using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Utilities;
using ClemCAddons.CameraAndNodes;
using ClemCAddons;
using System;

public class NPCCam : MonoBehaviour
{
    private Transform player;
    private Transform npc;
    private Vector3 characterPos;
    private static NPCCam instance;
    private new Camera camera;
    private bool active;
    private Camera npcCam;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    [Serializable]
    public class NPCCamSettings
    {
        public float distance;
        public float height;
        public float angle;
    }

    void Start()
    {
        instance = this;
        player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>().transform;
    }

    void Update()
    {
        if (!active)
            return;
        // CAMERA PAN
    }

    private void TransitionOut()
    {
        camera.transform.position = npcCam.transform.position;
        camera.GetComponent<TPSCameraWithNodeSupport>().enabled = true;
        _ = GameTools.DelayedCall(10, () =>
        {
            Lerper.ConstantLerp(npcCam.transform.position, camera.transform.position, 1, (v) => { npcCam.transform.position = v; });
            Lerper.ConstantLerp(npcCam.transform.rotation, camera.transform.rotation, 1, (q) => { npcCam.transform.rotation = q; },
                () =>
                {
                    npcCam.enabled = false;
                    camera.enabled = true;
                    npcCam.transform.position = defaultPos;
                    npcCam.transform.rotation = defaultRot;
                });
        });
    }

    private void TransitionIn()
    {
        camera = Array.Find(Camera.allCameras, t => t.gameObject.activeInHierarchy && t.TryGetComponent<TPSCameraWithNodeSupport>(out _));
        camera.GetComponent<TPSCameraWithNodeSupport>().enabled = false;
        FreshBulleAnimation.StartPriorityAnimation("Routine_Run", true);
        Lerper.ConstantLerp(camera.transform.position, npcCam.transform.position, 1,
        (v) => { camera.transform.position = v; },
        () =>
        {
            npcCam.enabled = true;
            FreshBulleAnimation.FinishedMajorAnimation(0);
            camera.enabled = false;
        });
        Lerper.ConstantLerp(camera.transform.rotation, npcCam.transform.rotation, 1, (v) => { camera.transform.rotation = v; });
        var playerPosY = player.position.y - npc.position.y;
        Lerper.ConstantSlerp((player.position - npc.position).SetY(0), (characterPos - npc.position).SetY(0), 1, (v) => { player.position = npc.position + new Vector3(v.x, playerPosY,v.z); });
        Lerper.ConstantLerp(player.GetComponentInChildren<FollowVelocity>().transform.rotation, Quaternion.LookRotation(characterPos.Direction(npc.position).SetY(0)),1, (q) => { player.GetComponentInChildren<FollowVelocity>().transform.rotation = q; });
    }

    public static void Show(Transform npc, Vector3 characterPos)
    {
        instance.npc = npc;
        instance.characterPos = characterPos;
        instance.npcCam = npc.GetComponentInChildren<Camera>(true);
        instance.defaultPos = instance.npcCam.transform.position;
        instance.defaultRot = instance.npcCam.transform.rotation;
        instance.TransitionIn();
        instance.active = true;
    }

    public static void Hide()
    {
        if (!instance.active)
            return;
        instance.TransitionOut();
        instance.active = false;
    }
}
