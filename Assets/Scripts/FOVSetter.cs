using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVSetter : MonoBehaviour
{
    public Camera cam;
    [Range(1, 180)]
    public float fov = 90;
    public float actualFOV = 0;
    public float vertFOV = 0;

    void Update()
    {
        actualFOV = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(fov * Mathf.Deg2Rad / 2) * cam.aspect / (4.0f / 3));
        vertFOV = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(fov * Mathf.Deg2Rad / 2) * (3.0f / 4));
        vertFOV = Mathf.Clamp(vertFOV, 1, 179);
        cam.fieldOfView = vertFOV;
        float t = cam.projectionMatrix.GetRow(1)[1];
        float matFov = Mathf.Atan(1 / t) * 2 * Mathf.Rad2Deg;
        // Debug.Log(matFov);
        // Debug.Log(t);
    }
}
