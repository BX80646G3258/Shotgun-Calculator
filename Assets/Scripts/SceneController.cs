using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using static TMPro.TMP_Dropdown;
using UnityEngine.UI;
using System;

namespace ShotgunCalculator
{
    public class SceneController : MonoBehaviour
    {
        new public Camera camera;
        public WeaponLoader weaponLoader;
        public Material spreadMat;
        public TMP_Dropdown movementDropdown;
        public TMP_Dropdown weaponDropdown;
        public TMP_Dropdown spreadDropdown;
        public Slider shotSlider;
        public RectTransform crosshair;
        public float shotSmoothing = .5f;
        string[] spreadModes = { "_MODE_NOSPREAD", "_MODE_SHOWSPREAD", "_MODE_AVERAGE", "_MODE_FLATAVERAGE", "_MODE_MINIMUM", "_MODE_MAXIMUM" };
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

            Cursor.visible = false;

            SetDynamicSamples();
        }

        // Update is called once per frame
        void Update()
        {
            crosshair.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(1))
                SetFixedSamples();

            if (Input.GetMouseButtonUp(1))
                SetDynamicSamples();

            int patternCount = weaponLoader.CurrentWeapon().patterns.Length;
            if (patternCount > 1)
            {
                int scrollAxis = (int)Input.mouseScrollDelta.y;
                if (scrollAxis == 0)
                {
                    shotIndex = Mathf.RoundToInt(shotSlider.value * (patternCount - 1));
                }
                else
                {
                    shotIndex += scrollAxis;
                    shotIndex = Math.Clamp(shotIndex, 0, patternCount - 1);
                }
                shotSlider.value = (float)shotIndex / (patternCount - 1);

                shotTime = Mathf.Lerp(shotTime, (float)shotIndex / (patternCount - 1), Util.GetLerpFract(shotSmoothing));
                weaponLoader.time = shotTime;
            }
            else
            {
                shotTime = 0;
                shotSlider.value = 0;
                weaponLoader.time = 0;
            }

            // if (Input.GetKeyDown(KeyCode.Tab))
            //     IncLoop(movementDropdown, 1);
            // if (Input.GetKeyDown(KeyCode.Space))
            //     IncLoop(weaponDropdown, 1);

            weaponLoader.movementState = (WeaponLoader.MovementState)movementDropdown.value;
            weaponLoader.weaponIndex = weaponDropdown.value;
        }

        void SetFixedSamples()
        {
            spreadMat.EnableKeyword("_SAMPLES_FIXED");
            spreadMat.DisableKeyword("_SAMPLES_DYNAMIC");
        }

        void SetDynamicSamples()
        {
            spreadMat.DisableKeyword("_SAMPLES_FIXED");
            spreadMat.EnableKeyword("_SAMPLES_DYNAMIC");
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
}