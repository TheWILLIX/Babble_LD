using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class Steps : MonoBehaviour
{
    [SerializeField] private LayerMask _layermask;
    [SerializeField] private float _distanceToGround = 0.1f;
    private Transform _right;
    private Transform _left;

    void Start()
    {
        var animator = GetComponent<Animator>();
        _left = transform.FindDeep("R_heel_side_pivot");
        _right = transform.FindDeep("L_heel_side_pivot");
    }

    void Update()
    {
        var l = ClemCAddons.Utilities.GameTools.FindGround(_left.position + Vector3.up * 0.05f, 0, _distanceToGround + 0.05f, _layermask, out var hitL);
        if((l < _distanceToGround).OnceIfTrueGate("leftfootvfx".GetHashCode()))
        {
            VFXSpawner.Spawn("Steps", hitL.point);
            AudioManager.Start2DSound("S_MarcheSable");
        }
        var r = ClemCAddons.Utilities.GameTools.FindGround(_right.position + Vector3.up * 0.05f, 0, _distanceToGround + 0.05f, _layermask, out var hitR);
        if ((r < _distanceToGround).OnceIfTrueGate("rightfootvfx".GetHashCode()))
        {
            VFXSpawner.Spawn("Steps", hitR.point);
            AudioManager.Start2DSound("S_MarcheSable");
        }
    }
}
