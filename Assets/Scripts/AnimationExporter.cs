using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationExporter : MonoBehaviour
{
    public WeaponLoader weaponLoader;
    public int frames = 15;
    public string path = "Frames/";
    int frameCount;
    void OnEnable()
    {
        frameCount = 0;
        weaponLoader.time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int totalFrames = frames * weaponLoader.CurrentWeapon().patterns.Length;
        if (frameCount <= totalFrames)
        {
            weaponLoader.time = (float)frameCount / totalFrames;
            ScreenCapture.CaptureScreenshot(path + frameCount.ToString("0000") + ".png");
            frameCount++;
        }
        else
        {
            enabled = false;
        }
    }
}
