using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquashMeduse : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        
        animator.SetBool("ShouldAnimate", false);
    }
}
