using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RenderingOptimizer : MonoBehaviour
{
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // is inside the camera frustum?
        if (IsVisibleFrom(Camera.allCameras[0]))
        {
            // enable the renderer and the collider
            _renderer.enabled = true;
        }
        else
        {
            // disable the renderer and the collider
            _renderer.enabled = false;
        }
    }

    private bool IsVisibleFrom(Camera camera)
    {
        float fov = camera.fieldOfView;
        camera.fieldOfView *= 1.2f;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        camera.fieldOfView = fov;
        return GeometryUtility.TestPlanesAABB(planes, _renderer.bounds);
    }
}
