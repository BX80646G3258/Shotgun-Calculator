using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using static TMPro.TMP_Dropdown;
using UnityEngine.UI;
using System;

public class CameraController : MonoBehaviour
{
    new public Camera camera;
    public WeaponLoader weaponLoader;
    public Material spreadMat;
    public TMP_Dropdown movementDropdown;
    public TMP_Dropdown weaponDropdown;
    public TMP_Dropdown spreadDropdown;
    public Slider shotSlider;
    public float shotSmoothing = .5f;
    string[] spreadModes = { "_MODE_NOSPREAD", "_MODE_SHOWSPREAD", "_MODE_AVERAGE", "_MODE_MINIMUM", "_MODE_MAXIMUM" };
    int samplingID = Shader.PropertyToID("_Samples");
    int shotIndex;
    float shotTime;
    // Start is called before the first frame update
    void Start()
    {
        foreach (WeaponLoader.MovementState mState in Enum.GetValues(typeof(WeaponLoader.MovementState)))
            movementDropdown.options.Add(new OptionData(mState.ToString().ToLower()));

        foreach (Weapon weapon in weaponLoader.weapons)
            weaponDropdown.options.Add(new OptionData(weapon.name));

        movementDropdown.value = 0;
        weaponDropdown.value = 0;
        SetSpreadMode(spreadDropdown.value);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            spreadMat.EnableKeyword("_SAMPLES_FIXED");
            spreadMat.DisableKeyword("_SAMPLES_DYNAMIC");
        }
        if (Input.GetMouseButtonUp(1))
        {
            spreadMat.DisableKeyword("_SAMPLES_FIXED");
            spreadMat.EnableKeyword("_SAMPLES_DYNAMIC");
        }

        int patternCount = weaponLoader.CurrentWeapon().patterns.Length;
        int scrollAxis = (int)Input.mouseScrollDelta.y;
        if (scrollAxis == 0)
        {
            shotIndex = (int)(shotSlider.value * patternCount);
        }
        else
        {
            shotIndex += scrollAxis;
            shotIndex = Math.Clamp(shotIndex, 0, patternCount);
        }
        shotSlider.value = (float)shotIndex / patternCount;

        shotTime = Mathf.Lerp(shotTime, (float)shotIndex / patternCount, GetLerpFract(shotSmoothing));
        weaponLoader.time = shotTime;

        // if (Input.GetKeyDown(KeyCode.Tab))
        //     IncLoop(movementDropdown, 1);
        // if (Input.GetKeyDown(KeyCode.Space))
        //     IncLoop(weaponDropdown, 1);

        weaponLoader.movementState = (WeaponLoader.MovementState)movementDropdown.value;
        weaponLoader.weaponIndex = weaponDropdown.value;
    }

    void IncLoop(TMP_Dropdown dropdown, int step)
    {
        dropdown.value = IncLoop(dropdown.value, dropdown.options.Count, step);
    }

    int IncLoop(int index, int max, int step)
    {
        index = (index + step) % max;
        if (index < 0)
            index += max;
        return index;
    }

    float GetLerpFract(float smoothing, float percent = .99f)
    {
        return 1 - Mathf.Exp((Mathf.Log(1f - percent) / smoothing) * Time.deltaTime);
    }

    public void SetSpreadMode(int mode)
    {
        for (int i = 0; i < spreadModes.Length; i++)
        {
            string keyword = spreadModes[i];
            if (i == mode)
                spreadMat.EnableKeyword(keyword);
            else
                spreadMat.DisableKeyword(keyword);
        }
    }
}
