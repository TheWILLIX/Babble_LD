using ClemCAddons;
using ClemCAddons.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LaserCamTester : MonoBehaviour
{
    [SerializeField] private float _distance;
    [SerializeField] private int _samplePoints = 100;
    LayerMask _playerLayerMask;
    void OnEnable()
    {
        _playerLayerMask = FindObjectOfType<CharacterMovement>(true).CollisionLayer;
    }
    void Update()
    {
        for(int i = 0; i < _samplePoints; i++)
        {
            var dir = Random.onUnitSphere;
            DrawDirection(dir);
        }

    }

    private void DrawDirection(Vector3 targetDir)
    {
        if(transform.position.CastToLineOnly(transform.position + targetDir * _distance, _playerLayerMask, "Hittable", out RaycastHit hit))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red, 0.1f);
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + targetDir * _distance, Color.red, 0.1f);
        }
    }
}
