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
        public AnimationLoader animationLoader;
        public Material spreadMat;
        public TMP_Dropdown movementDropdown;
        public TMP_Dropdown weaponDropdown;
        public TMP_Dropdown spreadDropdown;
        public TMP_Dropdown animationDropdown;
        public Slider shotSlider;
        public RectTransform crosshair;
        public MeshCollider mapCollider;
        public Transform target;
        public float taregtTurnSensitivity = -.5f;
        public float targetPanSensitivity = .01f;
        public float shotSmoothing = .5f;
        public TextAsset[] animations;
        string[] spreadModes = { "_MODE_NOSPREAD", "_MODE_SHOWSPREAD", "_MODE_AVERAGE", "_MODE_MINIMUM", "_MODE_MAXIMUM" };
        int samplingID = Shader.PropertyToID("_Samples");
        int shotIndex;
        float shotTime;
        bool lastFixedSampling;
        Vector3 mouseDelta;
        // Start is called before the first frame update
        void Start()
        {
            foreach (WeaponLoader.MovementState mState in Enum.GetValues(typeof(WeaponLoader.MovementState)))
                movementDropdown.options.Add(new OptionData(mState.ToString().ToLower()));

            foreach (Weapon weapon in weaponLoader.weapons)
                weaponDropdown.options.Add(new OptionData(weapon.name));

            foreach (TextAsset textAsset in animations)
                animationDropdown.options.Add(new OptionData(textAsset.name));

            movementDropdown.value = 0;
            weaponDropdown.value = 0;
            animationDropdown.value = 0;

            SetSpreadMode(spreadDropdown.value);
            UpdateAnimation();

            lastFixedSampling = true;
        }

        // Update is called once per frame
        void Update()
        {
            Cursor.visible = false;
            
            crosshair.position = Input.mousePosition;
            mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);

            bool useFixedSampling = false;

            useFixedSampling |= Input.GetMouseButton(1);
            useFixedSampling |= Input.GetMouseButton(2);


            if (Input.GetMouseButton(2))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                    PlaceTarget();
                else if (Input.GetKey(KeyCode.LeftShift))
                    PanTarget();
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
                    RaiseTarget();
                else
                    TurnTarget();
            }

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

            if (useFixedSampling != lastFixedSampling)
                SetFixedSampling(useFixedSampling);

            weaponLoader.movementState = (WeaponLoader.MovementState)movementDropdown.value;
            weaponLoader.weaponIndex = weaponDropdown.value;

            lastFixedSampling = useFixedSampling;
        }

        void SetFixedSampling(bool b)
        {
            if (b)
            {
                spreadMat.EnableKeyword("_SAMPLES_FIXED");
                spreadMat.DisableKeyword("_SAMPLES_DYNAMIC");
            }
            else
            {
                spreadMat.DisableKeyword("_SAMPLES_FIXED");
                spreadMat.EnableKeyword("_SAMPLES_DYNAMIC");
            }
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

        public void UpdateAnimation()
        {
            animationLoader.asset = animations[animationDropdown.value];
            animationLoader.rebuild = true;
        }

        void PlaceTarget()
        {
            RaycastHit hitInfo;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            // Debug.Log("placetarget: " + Input.mousePosition + "\t" + ray);
            if (mapCollider.Raycast(ray, out hitInfo, float.MaxValue))
            {
                // Debug.DrawRay(hitInfo.point, Vector3.up);
                Debug.DrawLine(camera.transform.position, hitInfo.point, Color.green);
                target.transform.position = hitInfo.point;
            }
        }

        void TurnTarget()
        {
            target.transform.rotation *= Quaternion.Euler(0, mouseDelta.x * taregtTurnSensitivity, 0);
        }

        void PanTarget()
        {
            Quaternion axisRotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
            target.transform.position += targetPanSensitivity * (axisRotation * new Vector3(mouseDelta.x, 0, mouseDelta.y));
        }

        void RaiseTarget()
        {
            target.transform.position += targetPanSensitivity * mouseDelta.y * Vector3.up;
        }
    }
}