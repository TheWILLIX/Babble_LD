using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class BulleAnimation : MonoBehaviour
{
    [SerializeField] private float _transitionDuration;
    private Animator bulleAnimation;
    private ClemCAddons.Player.CharacterMovement _player;
    private static string currentRoutine = "Idle";
    private static string[] chainAnimations = new string[] { };
    private static string cutAnimation = "";

    private static bool overriding;
    private static bool chainOverriding;
    private static bool previouslyFalling;

    void Start()
    {
        bulleAnimation = GetComponent <Animator>();
        _player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>();


    }

    void Update()
    {
        UpdateRoutine(); // update the current expected animation
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

    public static void FinishedMajorAnimation()
    {
        chainOverriding = false;
        overriding = false;
        latest = "";
    }
    public static void FinishedMajorAnimationSoft()
    {
        chainOverriding = false;
        overriding = false;
    }


    private void RunRoutine()
    {
        RunAnimation(currentRoutine);
    }

    private void CutAnimation()
    {
        RunAnimation(cutAnimation);
        cutAnimation = "";
        overriding = true;
        chainOverriding = false;
        chainAnimations = new string[] { };
    }
    private void UpdateRoutine()
    {
        if (_player.IsWalking && _player.GroundDistance < 0.05)
        {
            currentRoutine = "Walk";
        }
        else if (_player.GroundDistance < 0.05)
        {
            currentRoutine = "Idle";
        }

        if (_player.GroundDistance > 0.05 && _player.IsFalling)
        {
            currentRoutine = "Falling";
            previouslyFalling = true;
        }
    }

    private void CheckCut()
    {
        if (_player.GroundDistance < 0.05 && previouslyFalling)
        {
            cutAnimation = "Landing";
            previouslyFalling = false;
        }
        if (_player.GroundDistance > 0.05 && !_player.IsFalling)
        {
            cutAnimation = "Jump";
            previouslyFalling = true;
        }
    }

    private static string latest;
    private void RunAnimation(string animation)
    {
        if (latest == animation)
            return;
        bulleAnimation.CrossFade(animation, _transitionDuration);
        latest = animation;
    }
}
