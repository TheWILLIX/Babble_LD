using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class FreshBulleAnimation : MonoBehaviour
{
    [SerializeField] private float _transitionDuration;
    private Animator bulleAnimation;
    private ClemCAddons.Player.CharacterMovement _player;

    private static string currentRoutine = "Idle";
    private static string[] chainAnimations = new string[] { };
    private static string cutAnimation = "";
    private static string currentFace = "Face_Idle";
    private static string currentHand = "Hand_Idle";
    private static string currentPose = "Pose_Idle";

    private static float currentFaceDuration = -1;

    private static bool overriding;
    private static bool chainOverriding;
    private static bool previouslyFalling;
    private static bool previouslyMarineCurrent;
    private static bool currentlyMarineCurrent;
    private static bool previouslyGeyser;
    private static bool currentlyGeyser;

    private static bool finishedFirstStep = false;

    private bool previouslyGliding = false;
    private GameObject glidingVFX;


    #region Enums

    public enum FaceType
    {
        Happy,
        Angry,
        StaryEyes,
        Sad
    }

    public enum HandType
    {
        Recolting,
        Placing,
        Eating
    }

    public enum PoseType
    {
        Lifting,
        Coucou
    }

    #endregion Enums

    #region Start / Update

    void Start()
    {
        bulleAnimation = GetComponent <Animator>();
        _player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>();

    }



    void Update()
    {
        UpdateRoutine(); // update the current expected animation
        UpdateFace();
        CheckCut();
        
        if (cutAnimation != "") // if need be, override animation (priority overriding)
            CutAnimation();

        if(chainAnimations.Length > 0 && !chainOverriding && !overriding) // if not currently overriding, override (standard overriding)
        {
            RunAnimation(chainAnimations[0]);
            chainAnimations = chainAnimations.RemoveAt(0);
            chainOverriding = true;
        }

        if(!overriding && !chainOverriding) // if not overriding anything, set animation
            RunRoutine();
        RunFace();
        RunAstere();
        RunHand();
        RunPose();

        if(_player.IsGliding != previouslyGliding)
        {
            previouslyGliding = _player.IsGliding;
            if (previouslyGliding)
            {
                var left = transform.FindDeep("GrCtr_R_Elbow");
                var right = transform.FindDeep("GrCtr_L_Elbow");
                var y = ((left.position + right.position) / 2f).y;
                glidingVFX = VFXSpawner.Spawn("Gliding",  _player.transform, _player.transform.position.SetY(y));
            }
            else
            {
                var m = glidingVFX.GetComponent<ParticleSystem>().main;
                m.loop = false;
                glidingVFX = null;
            }
        }
    }

    #endregion Start / Update

    #region External Calls
    public static void StartFaceAnimation(FaceType face, float duration = -1)
    {
        switch (face)
        {
            case FaceType.Happy:
                currentFace = "Face_Happy";
                break;
            case FaceType.Angry:
                currentFace = "Face_Angry";
                break;
            case FaceType.StaryEyes:
                currentFace = "Face_StaryEyes";
                break;
            case FaceType.Sad:
                currentFace = "Face_Sad";
                break;
        }
        currentFaceDuration = duration;
    }

    public static void Coucou()
    {
        StartPoseAnimation(PoseType.Coucou);
    }

    public static void EndFaceAnimation()
    {
        currentFace = "Face_Idle";
        currentFaceDuration = -1;
    }

    public static void StartPriorityAnimation(string animation, bool maxPriority)
    {
        if (maxPriority)
        {
            cutAnimation = animation;
        }
        else if (!overriding)
        {
            chainAnimations.Add(animation);
        }
    }

    public static void StartHandAnimation(HandType hand)
    {
        switch (hand)
        {
            case HandType.Placing:
                currentHand = "Hand_Posage";
                break;
            case HandType.Recolting:
                currentHand = "Hand_Recolte";
                break;
            case HandType.Eating:
                currentHand = "Hand_Manger";
                break;
        }
    }

    public static void StartPoseAnimation(PoseType pose)
    {
        switch (pose)
        {
            case PoseType.Lifting:
                currentPose = "Pose_Portage";
                break;
            case PoseType.Coucou:
                currentPose = "Pose_Coucou";
                break;
        }
    }
    public static void ResetPoseAnimation()
    {
        currentPose = "Pose_Idle";
    }

    public static void ResetHandAnimation()
    {
        currentHand = "Hand_Idle";
    }

    #endregion External Calls

    #region State Calls

    public static void FinishedFirstStepAnimation()
    {
        finishedFirstStep = true;
    }
    public static void FinishedMajorAnimation(int layer)
    {
        if(layer == 0)
        {
            chainOverriding = false;
            overriding = false;
        } else if (layer == 1)
        {
            currentHand = "Hand_Idle";
        }
    }
    public static void FinishedMajorAnimationSoft(int layer)
    {
        if (layer == 0)
        {
            chainOverriding = false;
            overriding = false;
        } else if (layer == 1)
        {
            currentHand = "Hand_Idle";
        }
    }
    #endregion State Calls

    #region Internal Calls
    private static string[] latest = new string[5];
    private void RunAnimation(string animation, int layer = 0)
    {
        if (latest[layer] == animation)
            return;
        latest[layer] = animation;
        bulleAnimation.CrossFade(animation, _transitionDuration, layer);
    }
    private void CutAnimation()
    {
        overriding = true;
        chainOverriding = false;
        RunAnimation(cutAnimation);
        cutAnimation = "";
    }
    #endregion Internal Calls

    #region RunAnimations
    private void RunPose()
    {
        RunAnimation(currentPose, 4);
    }

    private void RunHand()
    {
        RunAnimation(currentHand, 1);
    }

    private void RunAstere()
    {
        if (DialogueManager.Instance.CharacterCurrentlySpeaking == "Astere")
            RunAnimation("Astere_Talking", 2);
        else
            RunAnimation("Astere_Idle", 2);
    }

    private void RunRoutine()
    {
        RunAnimation(currentRoutine);
    }

    private void RunFace()
    {
        RunAnimation(currentFace, 3);
    }
    #endregion RunAnimations

    #region Updates

    private void UpdateFace()
    {
        if (currentFaceDuration != -1)
        {
            currentFaceDuration -= Time.deltaTime;
            if (currentFaceDuration <= 0)
            {
                currentFace = "Idle";
                currentFaceDuration = -1;
            }
        }
    }

    private void UpdateRoutine()
    {
        if(BeamPush.CurrentReservation != null)
        {
            if(!previouslyMarineCurrent)
                cutAnimation = "Primary_Enter_CourantMarin";
            currentRoutine = "Routine_CourantMarin";
            previouslyMarineCurrent = true;
            return;
        }
        if (PushScript.PlayerInsideOne > 0)
        {
            if (!previouslyGeyser)
                cutAnimation = "Primary_Enter_Geyser";
            currentRoutine = PushScript.CurrentlyBoosted ? "Routine_GeyserPoulpe" : "Routine_Geyser";
            previouslyGeyser = true;
            return;
        }
        if (_player.IsOnGround)
        {
            if (_player.IsWalking)
            {
                if ((Seum.Instance.GetSeum() / Seum.Instance.SeumMax) >= 0.66)
                    currentRoutine = "Routine_RunAngry";
                else if ((Seum.Instance.GetSeum() / Seum.Instance.SeumMax) <= 0.33)
                    currentRoutine = "Routine_RunHappy";
                else
                    currentRoutine = "Routine_Run";
            }
            else
            {
                currentRoutine = "Routine_Idle";
            }
        } else if (_player.IsFalling || alreadyJumped)
        {
            if (_player.IsGliding)
            {
                currentRoutine = "Routine_Gliding";
                if (overriding && alreadyJumped && finishedFirstStep)
                {
                    overriding = false;
                    finishedFirstStep = false;
                    RunAnimation("Routine_Gliding");
                }
            }
            else
                currentRoutine = "Routine_Falling";
            previouslyFalling = true;
        }
        else // not falling, isn't on ground (at least detected as so)
        {
            if (_player.IsMoving)
            {
                currentRoutine = "Routine_Run";
            }
            else
            {
                currentRoutine = "Routine_Falling";
            }
        }
    }
    private bool alreadyJumped = false;
    private void CheckCut()
    {
        if(BeamPush.CurrentReservation == null && previouslyMarineCurrent)
        {
            previouslyMarineCurrent = false;
            currentlyMarineCurrent = true;
            cutAnimation = "Primary_Exit_CourantMarin";
            return;
        }
        if (previouslyMarineCurrent || currentlyMarineCurrent)
            return;
        if (PushScript.PlayerInsideOne == 0 && previouslyGeyser)
        {
            previouslyGeyser = false;
            currentlyGeyser = true;
            cutAnimation = "Primary_Exit_Geyser";
            return;
        }
        if (previouslyGeyser || currentlyGeyser)
            return;
        if (_player.GroundDistance < 0.05 && previouslyFalling)
        {
            cutAnimation = "Primary_Landing";
            previouslyFalling = false;
            alreadyJumped = false;
        }
        if (_player.GroundDistance > 0.05 && !_player.IsFalling && !alreadyJumped)
        {
            cutAnimation = "Primary_Jump";
            previouslyFalling = true;
            alreadyJumped = true;
        }
    }
    #endregion Updates
    
}
