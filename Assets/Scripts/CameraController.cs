using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public MeshCollider mapCollider;
    public Transform hitPoint;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;
        if (mapCollider.Raycast(new Ray(cam.transform.position, cam.transform.forward), out hitInfo, float.MaxValue))
        {
            // Debug.DrawRay(hitInfo.point, Vector3.up);
            Debug.DrawLine(cam.transform.position, hitInfo.point, Color.green);
            hitPoint.transform.position = hitInfo.point;
        }
    }
}
