using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVLocker : MonoBehaviour
{
    public Camera cam;
    public float fov = 90;

    void Update()
    {
        float vFov = Camera.HorizontalToVerticalFieldOfView(fov, cam.aspect);
        if (cam.fieldOfView != vFov)
        {
            // Debug.Log(Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect));
            cam.fieldOfView = vFov;
        }
    }
}
