using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeyserAnim : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    void Update()
    {
        if (PushScript.PlayerInsideOne > 0)
            _animator.SetTrigger("EnterGeyser");
        else
            _animator.SetTrigger("ExitGeyser");
    }
}
