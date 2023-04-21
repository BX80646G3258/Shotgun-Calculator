using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationExporter : MonoBehaviour
{
    public WeaponLoader weaponLoader;
    public int frames = 100;
    public string path = "Frames/";
    int frameCount;
    void OnEnable()
    {
        frameCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCount < frames)
        {
            weaponLoader.time = (float) frameCount / frames;
            ScreenCapture.CaptureScreenshot(path + frameCount.ToString("0000") + ".png");
            frameCount++;
        }
        else
        {
            enabled = false;
        }
    }
}
